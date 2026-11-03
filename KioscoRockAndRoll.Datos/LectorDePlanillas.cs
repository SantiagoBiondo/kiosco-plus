using System.Globalization;
using System.IO.Compression;
using System.Text;
using System.Xml.Linq;
using KioscoRockAndRoll.Dominio;
using KioscoRockAndRoll.Dominio.Entidades;

namespace KioscoRockAndRoll.Datos;

/// <summary>
/// Lee planillas exportadas por otros programas: CSV (separado por punto y coma, coma o tabulación)
/// y Excel .xlsx (la primera hoja). La primera fila con datos se toma como los títulos de las columnas.
/// </summary>
public static class LectorDePlanillas
{
    /// <exception cref="ReglaDeNegocioException">Si el formato no se puede leer o el archivo está vacío.</exception>
    public static TablaImportada Leer(string ruta)
    {
        var extension = Path.GetExtension(ruta).ToLowerInvariant();
        var filas = extension switch
        {
            ".csv" or ".txt" => LeerCsv(LeerTexto(ruta)),
            ".xlsx" => LeerXlsx(ruta),
            ".xls" => throw new ReglaDeNegocioException(
                "Los archivos .xls (Excel viejo) no se pueden leer. Abrilo en Excel y guardalo como .xlsx o CSV."),
            _ => throw new ReglaDeNegocioException("Elegí un archivo de Excel (.xlsx) o CSV.")
        };

        var conDatos = filas.SkipWhile(f => f.All(string.IsNullOrWhiteSpace)).ToList();
        if (conDatos.Count == 0)
            throw new ReglaDeNegocioException("El archivo está vacío.");
        var encabezados = conDatos[0].Select(t => t.Trim()).ToList();
        // Las filas más cortas que los títulos (celdas vacías al final) se completan, así todas tienen el mismo ancho.
        var datos = conDatos.Skip(1)
            .Select(f => f.Count >= encabezados.Count ? f : (IReadOnlyList<string>)[.. f, .. Enumerable.Repeat("", encabezados.Count - f.Count)])
            .ToList();
        return new TablaImportada(encabezados, datos);
    }

    // ---------- CSV ----------

    /// <summary>
    /// Lee el texto aunque el archivo esté abierto en Excel. Si no es UTF-8 válido, se toma como
    /// Latin-1 (lo que usa Excel en Windows en castellano para las tildes y la ñ).
    /// </summary>
    private static string LeerTexto(string ruta)
    {
        byte[] bytes;
        using (var archivo = new FileStream(ruta, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete))
        using (var memoria = new MemoryStream())
        {
            archivo.CopyTo(memoria);
            bytes = memoria.ToArray();
        }
        try
        {
            return new UTF8Encoding(false, throwOnInvalidBytes: true).GetString(bytes).TrimStart('﻿');
        }
        catch (DecoderFallbackException)
        {
            return Encoding.Latin1.GetString(bytes);
        }
    }

    /// <summary>CSV con comillas ("" adentro de un valor entre comillas es una comilla) y saltos de línea dentro de valores.</summary>
    public static List<IReadOnlyList<string>> LeerCsv(string texto)
    {
        // Excel a veces agrega una primera línea "sep=;" que indica el separador.
        char separador;
        if (texto.StartsWith("sep=", StringComparison.OrdinalIgnoreCase) && texto.Length > 4)
        {
            separador = texto[4];
            var finDeLinea = texto.IndexOf('\n');
            texto = finDeLinea < 0 ? "" : texto[(finDeLinea + 1)..];
        }
        else
        {
            separador = DetectarSeparador(texto);
        }

        var filas = new List<IReadOnlyList<string>>();
        var fila = new List<string>();
        var valor = new StringBuilder();
        var entreComillas = false;

        for (var i = 0; i < texto.Length; i++)
        {
            var c = texto[i];
            if (entreComillas)
            {
                if (c == '"' && i + 1 < texto.Length && texto[i + 1] == '"') { valor.Append('"'); i++; }
                else if (c == '"') entreComillas = false;
                else valor.Append(c);
            }
            else if (c == '"') entreComillas = true;
            else if (c == separador) { fila.Add(valor.ToString()); valor.Clear(); }
            else if (c is '\r' or '\n')
            {
                if (c == '\r' && i + 1 < texto.Length && texto[i + 1] == '\n') i++;
                fila.Add(valor.ToString()); valor.Clear();
                filas.Add(fila); fila = [];
            }
            else valor.Append(c);
        }
        if (valor.Length > 0 || fila.Count > 0)
        {
            fila.Add(valor.ToString());
            filas.Add(fila);
        }
        return filas;
    }

    /// <summary>El separador que más aparece en la primera línea (fuera de comillas). Ante un empate, punto y coma.</summary>
    private static char DetectarSeparador(string texto)
    {
        var cuenta = new Dictionary<char, int> { [';'] = 0, [','] = 0, ['\t'] = 0 };
        var entreComillas = false;
        foreach (var c in texto)
        {
            if (c == '"') entreComillas = !entreComillas;
            else if (!entreComillas && c is '\r' or '\n') break;
            else if (!entreComillas && cuenta.ContainsKey(c)) cuenta[c]++;
        }
        return cuenta.OrderByDescending(par => par.Value).ThenBy(par => par.Key == ';' ? 0 : 1).First().Key;
    }

    // ---------- Excel .xlsx ----------

    private static readonly XNamespace Hoja = "http://schemas.openxmlformats.org/spreadsheetml/2006/main";
    private static readonly XNamespace Relaciones = "http://schemas.openxmlformats.org/officeDocument/2006/relationships";
    private static readonly XNamespace RelacionesDelPaquete = "http://schemas.openxmlformats.org/package/2006/relationships";

    /// <summary>
    /// Un .xlsx es un zip con archivos XML: se busca la primera hoja del libro, los textos compartidos
    /// y se arma cada fila con sus celdas en la columna que corresponde.
    /// </summary>
    private static List<IReadOnlyList<string>> LeerXlsx(string ruta)
    {
        try
        {
            using var archivo = new FileStream(ruta, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete);
            using var zip = new ZipArchive(archivo, ZipArchiveMode.Read);

            var textos = Xml(zip, "xl/sharedStrings.xml")?.Root?.Elements(Hoja + "si")
                .Select(si => string.Concat(si.Descendants(Hoja + "t").Select(t => t.Value)))
                .ToList() ?? [];

            var hoja = Xml(zip, RutaDeLaPrimeraHoja(zip))
                       ?? throw new ReglaDeNegocioException("El archivo de Excel no tiene hojas.");

            var filas = new SortedDictionary<int, SortedDictionary<int, string>>();
            var siguienteFila = 0;
            foreach (var row in hoja.Descendants(Hoja + "row"))
            {
                var numeroFila = int.TryParse((string?)row.Attribute("r"), out var r) ? r - 1 : siguienteFila;
                siguienteFila = numeroFila + 1;
                var celdas = new SortedDictionary<int, string>();
                var siguienteColumna = 0;
                foreach (var c in row.Elements(Hoja + "c"))
                {
                    var columna = (string?)c.Attribute("r") is { } referencia ? Columna(referencia) : siguienteColumna;
                    siguienteColumna = columna + 1;
                    celdas[columna] = ValorDeCelda(c, textos);
                }
                filas[numeroFila] = celdas;
            }

            // Se rellenan los huecos (filas o celdas vacías que Excel no guarda).
            var resultado = new List<IReadOnlyList<string>>();
            var ultimaFila = filas.Count == 0 ? -1 : filas.Keys.Max();
            for (var f = 0; f <= ultimaFila; f++)
            {
                if (!filas.TryGetValue(f, out var celdas) || celdas.Count == 0)
                {
                    resultado.Add([]);
                    continue;
                }
                resultado.Add(Enumerable.Range(0, celdas.Keys.Max() + 1).Select(col => celdas.GetValueOrDefault(col, "")).ToList());
            }
            return resultado;
        }
        catch (Exception ex) when (ex is InvalidDataException or System.Xml.XmlException)
        {
            throw new ReglaDeNegocioException("No se pudo leer el archivo de Excel. Probá guardarlo de nuevo como .xlsx o como CSV.");
        }
    }

    private static string RutaDeLaPrimeraHoja(ZipArchive zip)
    {
        var libro = Xml(zip, "xl/workbook.xml");
        var idRelacion = (string?)libro?.Root?.Element(Hoja + "sheets")?.Elements(Hoja + "sheet").FirstOrDefault()?.Attribute(Relaciones + "id");
        var destino = Xml(zip, "xl/_rels/workbook.xml.rels")?.Root?.Elements(RelacionesDelPaquete + "Relationship")
            .FirstOrDefault(r => (string?)r.Attribute("Id") == idRelacion)?.Attribute("Target")?.Value;
        if (destino == null)
            return "xl/worksheets/sheet1.xml";
        return destino.StartsWith('/') ? destino.TrimStart('/') : "xl/" + destino;
    }

    private static XDocument? Xml(ZipArchive zip, string ruta)
    {
        var entrada = zip.GetEntry(ruta);
        if (entrada == null)
            return null;
        using var flujo = entrada.Open();
        return XDocument.Load(flujo);
    }

    /// <summary>"C12" → 2 (la columna C, contando desde 0).</summary>
    private static int Columna(string referencia)
    {
        var columna = 0;
        foreach (var c in referencia.TakeWhile(char.IsLetter))
            columna = columna * 26 + (char.ToUpperInvariant(c) - 'A' + 1);
        return columna - 1;
    }

    private static string ValorDeCelda(XElement c, List<string> textos)
    {
        var tipo = (string?)c.Attribute("t");
        var v = (string?)c.Element(Hoja + "v") ?? "";
        return tipo switch
        {
            "s" => int.TryParse(v, out var i) && i >= 0 && i < textos.Count ? textos[i] : "",
            "inlineStr" => string.Concat(c.Descendants(Hoja + "t").Select(t => t.Value)),
            "str" or "e" => v,
            "b" => v == "1" ? "1" : "0",
            _ => Numero(v)
        };
    }

    /// <summary>
    /// Excel guarda los números en formato inglés y a veces en notación científica (un código de barras
    /// largo queda "7.790895000997E+12"). Se pasan a texto como se escriben acá: enteros sin decimales
    /// (así los códigos quedan completos) y el resto con coma decimal.
    /// </summary>
    private static string Numero(string v)
    {
        if (!decimal.TryParse(v, NumberStyles.Float, CultureInfo.InvariantCulture, out var numero))
            return v;
        return numero == decimal.Truncate(numero)
            ? numero.ToString("0", CultureInfo.InvariantCulture)
            : Math.Round(numero, 2).ToString("0.##", CultureInfo.InvariantCulture).Replace('.', ',');
    }
}

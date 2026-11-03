using System.Globalization;
using System.Text;

namespace KioscoRockAndRoll.Dominio.Entidades;

/// <summary>
/// Una planilla leída de un archivo (Excel o CSV): la primera fila son los títulos de las columnas
/// y el resto, los datos. Todo como texto, tal cual venía.
/// </summary>
public sealed class TablaImportada
{
    public TablaImportada(IReadOnlyList<string> encabezados, IReadOnlyList<IReadOnlyList<string>> filas)
    {
        Encabezados = encabezados;
        Filas = filas;
    }

    public IReadOnlyList<string> Encabezados { get; }
    public IReadOnlyList<IReadOnlyList<string>> Filas { get; }

    /// <summary>El valor de una celda ("" si la fila es más corta).</summary>
    public static string Celda(IReadOnlyList<string> fila, int columna) =>
        columna >= 0 && columna < fila.Count ? fila[columna].Trim() : "";

    /// <summary>"A", "B", ..., "Z", "AA"... como en Excel.</summary>
    public static string LetraDeColumna(int columna)
    {
        var letras = "";
        for (var n = columna + 1; n > 0; n = (n - 1) / 26)
            letras = (char)('A' + (n - 1) % 26) + letras;
        return letras;
    }
}

/// <summary>Los datos de un producto que se pueden traer de otro programa.</summary>
public enum CampoImportable
{
    Codigo,
    Nombre,
    Costo,
    Venta,
    Stock,
    Categoria,
    Proveedor
}

public static class CampoImportableExtensiones
{
    public static string Descripcion(this CampoImportable campo) => campo switch
    {
        CampoImportable.Codigo => "Código de barras",
        CampoImportable.Nombre => "Nombre",
        CampoImportable.Costo => "Precio de costo",
        CampoImportable.Venta => "Precio de venta",
        CampoImportable.Stock => "Stock",
        CampoImportable.Categoria => "Rubro / categoría",
        CampoImportable.Proveedor => "Proveedor",
        _ => campo.ToString()
    };

    /// <summary>Sin estos tres no se puede dar de alta un producto.</summary>
    public static bool EsObligatorio(this CampoImportable campo) =>
        campo is CampoImportable.Codigo or CampoImportable.Nombre or CampoImportable.Venta;
}

/// <summary>
/// Qué columna de la planilla corresponde a cada dato. Se adivina por los títulos
/// (por ejemplo "Descripción" o "Artículo" es el nombre) y se puede corregir a mano.
/// </summary>
public sealed class MapeoDeColumnas
{
    // Palabras con las que suelen titularse las columnas en otros programas, sin tildes ni mayúsculas.
    // El orden importa: "precio costo" tiene que reconocerse como costo antes de que "precio" lo tome la venta.
    private static readonly (CampoImportable Campo, string[] Palabras)[] Sinonimos =
    [
        (CampoImportable.Codigo, ["codigo de barras", "cod barras", "codigo barras", "codbarras", "ean", "barras", "codigo", "cod", "sku", "plu"]),
        (CampoImportable.Costo, ["precio de costo", "precio costo", "costo", "compra", "precio compra", "p costo"]),
        (CampoImportable.Venta, ["precio de venta", "precio venta", "venta", "pvp", "precio final", "precio publico", "precio", "p venta", "importe"]),
        (CampoImportable.Nombre, ["nombre", "descripcion", "producto", "articulo", "detalle"]),
        (CampoImportable.Stock, ["stock", "existencia", "cantidad", "cant", "unidades"]),
        (CampoImportable.Categoria, ["rubro", "categoria", "familia", "seccion", "grupo"]),
        (CampoImportable.Proveedor, ["proveedor", "distribuidor", "distribuidora"])
    ];

    private readonly Dictionary<CampoImportable, int> _columnas = [];

    /// <summary>La columna asignada a un dato, o null si ese dato no se importa.</summary>
    public int? Columna(CampoImportable campo) => _columnas.TryGetValue(campo, out var columna) ? columna : null;

    /// <summary>Asigna una columna a un dato (null = no importarlo). Una columna no puede ser dos datos a la vez.</summary>
    public void Asignar(CampoImportable campo, int? columna)
    {
        if (columna is null)
        {
            _columnas.Remove(campo);
            return;
        }
        foreach (var otro in _columnas.Where(par => par.Value == columna && par.Key != campo).Select(par => par.Key).ToList())
            _columnas.Remove(otro);
        _columnas[campo] = columna.Value;
    }

    public IEnumerable<CampoImportable> FaltanObligatorios() =>
        Enum.GetValues<CampoImportable>().Where(c => c.EsObligatorio() && Columna(c) is null);

    /// <summary>Adivina el mapeo por los títulos de las columnas.</summary>
    public static MapeoDeColumnas Detectar(IReadOnlyList<string> encabezados)
    {
        var mapeo = new MapeoDeColumnas();
        var titulos = encabezados.Select(Normalizar).ToList();

        // Primero los títulos exactos ("Precio" = venta), después los que contienen la palabra ("Precio unitario").
        foreach (var exacto in new[] { true, false })
        {
            foreach (var (campo, palabras) in Sinonimos.Where(s => mapeo.Columna(s.Campo) is null))
            {
                var columna = palabras.Select(palabra => BuscarLibre(palabra, exacto)).FirstOrDefault(i => i >= 0, -1);
                if (columna >= 0)
                    mapeo._columnas[campo] = columna;
            }
        }
        return mapeo;

        // La primera columna todavía sin asignar cuyo título es (o contiene) la palabra.
        int BuscarLibre(string palabra, bool exacto)
        {
            for (var i = 0; i < titulos.Count; i++)
                if (!mapeo._columnas.ContainsValue(i) && (exacto ? titulos[i] == palabra : titulos[i].Contains(palabra)))
                    return i;
            return -1;
        }
    }

    /// <summary>Minúsculas, sin tildes y sin signos: "Código de Barras:" → "codigo de barras".</summary>
    private static string Normalizar(string texto)
    {
        var sinTildes = new StringBuilder();
        foreach (var c in texto.Trim().ToLowerInvariant().Normalize(NormalizationForm.FormD))
        {
            if (CharUnicodeInfo.GetUnicodeCategory(c) == UnicodeCategory.NonSpacingMark)
                continue;
            sinTildes.Append(char.IsLetterOrDigit(c) ? c : ' ');
        }
        return string.Join(' ', sinTildes.ToString().Split(' ', StringSplitOptions.RemoveEmptyEntries));
    }
}

/// <summary>
/// Lee números como los escriben los programas de acá: "1.234,50", "$ 1500", "1234.5" o "1.500" (mil quinientos).
/// </summary>
public static class NumeroImportado
{
    public static bool TryLeer(string texto, out decimal numero)
    {
        numero = 0;
        var limpio = new string(texto.Where(c => char.IsDigit(c) || c is '.' or ',' or '-').ToArray());
        if (limpio is "" or "-")
            return false;

        var ultimoPunto = limpio.LastIndexOf('.');
        var ultimaComa = limpio.LastIndexOf(',');
        string normalizado;
        if (ultimoPunto >= 0 && ultimaComa >= 0)
        {
            // Los dos: el que aparece último separa los decimales ("1.234,50" o "1,234.50").
            var decimalSep = ultimaComa > ultimoPunto ? ',' : '.';
            var milesSep = decimalSep == ',' ? '.' : ',';
            normalizado = limpio.Replace(milesSep.ToString(), "").Replace(decimalSep, '.');
        }
        else if (ultimoPunto >= 0 || ultimaComa >= 0)
        {
            var sep = ultimoPunto >= 0 ? '.' : ',';
            var partes = limpio.Split(sep);
            // Uno solo, repetido o seguido de exactamente 3 cifras, es de miles ("1.500", "1.234.567");
            // si no, es la coma decimal ("1500,5", "12.75").
            var esDeMiles = partes.Length > 2 || partes[^1].Length == 3;
            normalizado = esDeMiles ? limpio.Replace(sep.ToString(), "") : limpio.Replace(sep, '.');
        }
        else
        {
            normalizado = limpio;
        }

        return decimal.TryParse(normalizado, NumberStyles.AllowLeadingSign | NumberStyles.AllowDecimalPoint,
            CultureInfo.InvariantCulture, out numero);
    }
}

/// <summary>Qué hacer con los productos de la planilla cuyo código ya está cargado.</summary>
public enum SiElCodigoYaExiste
{
    /// <summary>Se actualizan con los datos de la planilla (las columnas vacías no cambian nada).</summary>
    Actualizar = 1,

    /// <summary>Se dejan como están.</summary>
    DejarComoEsta = 2
}

public enum EstadoFilaImportada
{
    Nuevo,
    Actualiza,
    YaExiste,
    Error
}

/// <summary>Una fila de la planilla ya interpretada: qué va a pasar con ella al importar.</summary>
/// <param name="NumeroDeFila">El número de fila en la planilla (como lo muestra Excel, contando los títulos).</param>
/// <param name="Costo">Null si la planilla no trae costo para este producto (si ya existe, conserva el suyo).</param>
/// <param name="Stock">Null si la planilla no trae stock para este producto (si ya existe, conserva el suyo).</param>
public sealed record FilaImportada(
    int NumeroDeFila, EstadoFilaImportada Estado, string Codigo, string Nombre,
    decimal? Costo, decimal Venta, int? Stock, string? Categoria, string? Proveedor, string? Error = null)
{
    public bool SeImporta => Estado is EstadoFilaImportada.Nuevo or EstadoFilaImportada.Actualiza;

    public string EstadoDescripcion => Estado switch
    {
        EstadoFilaImportada.Nuevo => "Nuevo",
        EstadoFilaImportada.Actualiza => "Se actualiza",
        EstadoFilaImportada.YaExiste => "Ya existe: no se toca",
        _ => $"Error: {Error}"
    };
}

/// <summary>Cómo quedaría la importación, antes de hacerla.</summary>
public sealed record VistaPreviaImportacion(IReadOnlyList<FilaImportada> Filas)
{
    public int Nuevos => Filas.Count(f => f.Estado == EstadoFilaImportada.Nuevo);
    public int Actualizados => Filas.Count(f => f.Estado == EstadoFilaImportada.Actualiza);
    public int SinTocar => Filas.Count(f => f.Estado == EstadoFilaImportada.YaExiste);
    public int ConError => Filas.Count(f => f.Estado == EstadoFilaImportada.Error);
    public bool HayAlgoParaImportar => Nuevos + Actualizados > 0;
}

/// <summary>Lo que se hizo al importar.</summary>
public sealed record ResultadoImportacion(int Nuevos, int Actualizados, int CategoriasCreadas, int ProveedoresCreados)
{
    /// <summary>"4 productos nuevos y 1 actualizado. Se crearon 3 rubros y 1 proveedor."</summary>
    public string Descripcion
    {
        get
        {
            var productos = Contar(Nuevos, "producto nuevo", "productos nuevos") +
                            (Actualizados > 0 ? $" y {Contar(Actualizados, "actualizado", "actualizados")}" : "") + ".";
            var creados = new List<string>();
            if (CategoriasCreadas > 0) creados.Add(Contar(CategoriasCreadas, "rubro", "rubros"));
            if (ProveedoresCreados > 0) creados.Add(Contar(ProveedoresCreados, "proveedor", "proveedores"));
            return creados.Count == 0
                ? productos
                : $"{productos} Se {(CategoriasCreadas + ProveedoresCreados == 1 ? "creó" : "crearon")} {string.Join(" y ", creados)}.";
        }
    }

    private static string Contar(int cantidad, string singular, string plural) =>
        cantidad == 1 ? $"1 {singular}" : $"{cantidad} {plural}";
}

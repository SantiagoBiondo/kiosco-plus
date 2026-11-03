using KioscoRockAndRoll.Dominio.Entidades;
using KioscoRockAndRoll.Dominio.Repositorios;

namespace KioscoRockAndRoll.Dominio.Servicios;

/// <summary>
/// Trae los productos de otro programa a partir de una planilla (Excel o CSV) exportada por él.
/// Primero se arma una vista previa con lo que va a pasar con cada fila; después se importa todo junto.
/// </summary>
public class ImportadorDeProductos(
    IProductoRepositorio productos, CatalogoServicio<Categoria> categorias, CatalogoServicio<Proveedor> proveedores)
{
    /// <summary>Interpreta cada fila de la planilla sin guardar nada.</summary>
    /// <exception cref="ReglaDeNegocioException">Si falta elegir la columna de algún dato obligatorio.</exception>
    public VistaPreviaImportacion Previsualizar(TablaImportada tabla, MapeoDeColumnas mapeo, SiElCodigoYaExiste siYaExiste)
    {
        var faltan = mapeo.FaltanObligatorios().Select(c => c.Descripcion()).ToList();
        if (faltan.Count > 0)
            throw new ReglaDeNegocioException($"Elegí en qué columna está: {string.Join(", ", faltan)}.");

        var cargados = productos.Buscar("").Select(p => p.CodigoBarras).ToHashSet();
        var filaDeCadaCodigo = new Dictionary<string, int>();
        var filas = new List<FilaImportada>();

        for (var i = 0; i < tabla.Filas.Count; i++)
        {
            var celdas = tabla.Filas[i];
            if (celdas.All(string.IsNullOrWhiteSpace))
                continue;
            var fila = Interpretar(celdas, i + 2, mapeo, cargados, filaDeCadaCodigo, siYaExiste);
            if (fila.Estado != EstadoFilaImportada.Error)
                filaDeCadaCodigo[fila.Codigo] = fila.NumeroDeFila;
            filas.Add(fila);
        }
        return new VistaPreviaImportacion(filas);
    }

    /// <summary>
    /// Guarda los productos nuevos y actualiza los existentes (si así se eligió), todo en una sola operación.
    /// Antes crea los rubros y proveedores que no existían. Las filas con error no se importan.
    /// </summary>
    public ResultadoImportacion Importar(VistaPreviaImportacion vistaPrevia)
    {
        var aImportar = vistaPrevia.Filas.Where(f => f.SeImporta).ToList();
        if (aImportar.Count == 0)
            throw new ReglaDeNegocioException("No hay productos para importar.");

        var (idDeCategoria, categoriasCreadas) = AsegurarCatalogo(categorias, aImportar.Select(f => f.Categoria), n => new Categoria(n));
        var (idDeProveedor, proveedoresCreados) = AsegurarCatalogo(proveedores, aImportar.Select(f => f.Proveedor), n => new Proveedor(n));

        var existentes = productos.Buscar("").ToDictionary(p => p.CodigoBarras);
        var nuevos = new List<Producto>();
        var actualizados = new List<Producto>();
        foreach (var fila in aImportar)
        {
            long? categoriaId = fila.Categoria is { } c ? idDeCategoria[c] : null;
            long? proveedorId = fila.Proveedor is { } p ? idDeProveedor[p] : null;

            if (existentes.TryGetValue(fila.Codigo, out var producto))
            {
                // Lo que la planilla no trae, el producto lo conserva.
                producto.ActualizarDatos(fila.Codigo, fila.Nombre, fila.Costo ?? producto.PrecioCosto, fila.Venta, producto.StockMinimo);
                if (fila.Stock is { } stock)
                    producto.AjustarStock(stock);
                producto.Clasificar(categoriaId ?? producto.CategoriaId, proveedorId ?? producto.ProveedorId);
                actualizados.Add(producto);
            }
            else
            {
                var nuevo = new Producto(fila.Codigo, fila.Nombre, fila.Costo ?? 0, fila.Venta, fila.Stock ?? 0);
                nuevo.Clasificar(categoriaId, proveedorId);
                nuevos.Add(nuevo);
            }
        }

        productos.GuardarImportacion(nuevos, actualizados);
        return new ResultadoImportacion(nuevos.Count, actualizados.Count, categoriasCreadas, proveedoresCreados);
    }

    private static FilaImportada Interpretar(IReadOnlyList<string> celdas, int numero, MapeoDeColumnas mapeo,
        HashSet<string> cargados, Dictionary<string, int> filaDeCadaCodigo, SiElCodigoYaExiste siYaExiste)
    {
        string Texto(CampoImportable campo) => mapeo.Columna(campo) is { } columna ? TablaImportada.Celda(celdas, columna) : "";

        var codigo = Texto(CampoImportable.Codigo);
        var nombre = Texto(CampoImportable.Nombre);
        FilaImportada ConError(string motivo) => new(numero, EstadoFilaImportada.Error, codigo, nombre, null, 0, null, null, null, motivo);

        if (codigo == "")
            return ConError("falta el código");
        if (filaDeCadaCodigo.TryGetValue(codigo, out var otraFila))
            return ConError($"el código se repite (ya está en la fila {otraFila})");
        if (nombre == "")
            return ConError("falta el nombre");

        if (!NumeroImportado.TryLeer(Texto(CampoImportable.Venta), out var venta))
            return ConError("el precio de venta no es un número");

        decimal? costo = null;
        var textoCosto = Texto(CampoImportable.Costo);
        if (textoCosto != "")
        {
            if (!NumeroImportado.TryLeer(textoCosto, out var leido))
                return ConError("el costo no es un número");
            costo = leido;
        }

        int? stock = null;
        var textoStock = Texto(CampoImportable.Stock);
        if (textoStock != "")
        {
            if (!NumeroImportado.TryLeer(textoStock, out var leido) || leido != decimal.Truncate(leido))
                return ConError("el stock tiene que ser un número entero");
            if (leido < 0)
                return ConError("el stock no puede ser negativo");
            stock = (int)leido;
        }

        var categoria = NullSiVacio(Texto(CampoImportable.Categoria));
        var proveedor = NullSiVacio(Texto(CampoImportable.Proveedor));
        if ((categoria?.Length ?? 0) > ElementoDeCatalogo.LargoMaximoNombre || (proveedor?.Length ?? 0) > ElementoDeCatalogo.LargoMaximoNombre)
            return ConError($"el rubro o el proveedor tiene más de {ElementoDeCatalogo.LargoMaximoNombre} letras");

        // Las mismas reglas que al cargar un producto a mano (precios no negativos, etc.).
        try
        {
            _ = new Producto(codigo, nombre, costo ?? 0, venta, stock ?? 0);
        }
        catch (ReglaDeNegocioException ex)
        {
            return ConError(ex.Message.TrimEnd('.').ToLowerInvariant());
        }

        var estado = !cargados.Contains(codigo) ? EstadoFilaImportada.Nuevo
            : siYaExiste == SiElCodigoYaExiste.Actualizar ? EstadoFilaImportada.Actualiza
            : EstadoFilaImportada.YaExiste;
        return new FilaImportada(numero, estado, codigo, nombre, costo, venta, stock, categoria, proveedor);
    }

    /// <summary>Crea los rubros o proveedores que todavía no existen. Devuelve el Id de cada nombre (sin importar mayúsculas).</summary>
    private static (Dictionary<string, long> IdDe, int Creados) AsegurarCatalogo<T>(
        CatalogoServicio<T> catalogo, IEnumerable<string?> nombres, Func<string, T> crear) where T : ElementoDeCatalogo
    {
        var idDe = catalogo.Listar().ToDictionary(e => e.Nombre, e => e.Id, StringComparer.CurrentCultureIgnoreCase);
        var creados = 0;
        foreach (var nombre in nombres.OfType<string>().Distinct(StringComparer.CurrentCultureIgnoreCase))
        {
            if (idDe.ContainsKey(nombre))
                continue;
            var elemento = crear(nombre);
            catalogo.Guardar(elemento);
            idDe[nombre] = elemento.Id;
            creados++;
        }
        return (idDe, creados);
    }

    private static string? NullSiVacio(string texto) => texto == "" ? null : texto;
}

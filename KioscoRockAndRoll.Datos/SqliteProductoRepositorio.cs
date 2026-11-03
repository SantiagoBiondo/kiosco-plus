using KioscoRockAndRoll.Dominio.Entidades;
using KioscoRockAndRoll.Dominio.Repositorios;
using Microsoft.Data.Sqlite;
using static KioscoRockAndRoll.Datos.Conversiones;

namespace KioscoRockAndRoll.Datos;

public sealed class SqliteProductoRepositorio(ConexionSqlite conexion) : IProductoRepositorio
{
    // Junto con cada producto suelto se lee el stock de su paquete (los paquetes cerrados que quedan).
    private const string SelectProductos = """
        SELECT p.Id, p.CodigoBarras, p.Nombre, p.PrecioCostoCentavos, p.PrecioVentaCentavos, p.Stock, p.StockMinimo,
               p.CategoriaId, p.ProveedorId, p.PaqueteId, p.UnidadesPorPaquete, COALESCE(paq.Stock, 0), p.PrecioAlVender
        FROM Productos p
        LEFT JOIN Productos paq ON paq.Id = p.PaqueteId
        """;

    public Producto? ObtenerPorId(long id) =>
        ConsultarUno($"{SelectProductos} WHERE p.Id = $id", ("$id", id));

    public Producto? ObtenerPorCodigo(string codigoBarras) =>
        ConsultarUno($"{SelectProductos} WHERE p.CodigoBarras = $codigo", ("$codigo", codigoBarras));

    public IReadOnlyList<Producto> Buscar(string texto)
    {
        using var db = conexion.Abrir();
        using var comando = db.CreateCommand();
        // ESCAPE permite buscar textos que contienen % o _ sin que se tomen como comodines.
        comando.CommandText = $"""
            {SelectProductos}
            WHERE p.Nombre LIKE $patron ESCAPE '\' OR p.CodigoBarras LIKE $patron ESCAPE '\'
            ORDER BY p.Nombre COLLATE NOCASE
            """;
        comando.Parameters.AddWithValue("$patron", $"%{EscaparLike(texto.Trim())}%");

        var productos = new List<Producto>();
        using var lector = comando.ExecuteReader();
        while (lector.Read())
            productos.Add(Leer(lector));
        return productos;
    }

    private const string InsertarProducto = """
        INSERT INTO Productos (CodigoBarras, Nombre, PrecioCostoCentavos, PrecioVentaCentavos, Stock, StockMinimo,
                               CategoriaId, ProveedorId, PaqueteId, UnidadesPorPaquete, PrecioAlVender)
        VALUES ($codigo, $nombre, $costo, $venta, $stock, $minimo, $categoria, $proveedor, $paquete, $unidades, $alVender)
        RETURNING Id
        """;

    private const string ActualizarProducto = """
        UPDATE Productos
        SET CodigoBarras = $codigo, Nombre = $nombre, PrecioCostoCentavos = $costo,
            PrecioVentaCentavos = $venta, Stock = $stock, StockMinimo = $minimo,
            CategoriaId = $categoria, ProveedorId = $proveedor,
            PaqueteId = $paquete, UnidadesPorPaquete = $unidades, PrecioAlVender = $alVender
        WHERE Id = $id
        """;

    public void Agregar(Producto producto)
    {
        using var db = conexion.Abrir();
        using var comando = db.CreateCommand();
        comando.CommandText = InsertarProducto;
        CargarParametros(comando, producto);
        producto.AsignarId((long)comando.ExecuteScalar()!);
    }

    public void Actualizar(Producto producto) => ActualizarVarios([producto]);

    public void ActualizarVarios(IReadOnlyCollection<Producto> productos) => GuardarImportacion([], productos);

    public void GuardarImportacion(IReadOnlyCollection<Producto> nuevos, IReadOnlyCollection<Producto> modificados)
    {
        using var db = conexion.Abrir();
        using var transaccion = db.BeginTransaction();

        using (var insertar = db.CreateCommand())
        {
            insertar.CommandText = InsertarProducto;
            foreach (var producto in nuevos)
            {
                insertar.Parameters.Clear();
                CargarParametros(insertar, producto);
                producto.AsignarId((long)insertar.ExecuteScalar()!);
            }
        }

        using (var actualizar = db.CreateCommand())
        {
            actualizar.CommandText = ActualizarProducto;
            foreach (var producto in modificados)
            {
                actualizar.Parameters.Clear();
                CargarParametros(actualizar, producto);
                actualizar.Parameters.AddWithValue("$id", producto.Id);
                actualizar.ExecuteNonQuery();
            }
        }

        transaccion.Commit();
    }

    public void Eliminar(long id)
    {
        using var db = conexion.Abrir();
        using var comando = db.CreateCommand();
        comando.CommandText = "DELETE FROM Productos WHERE Id = $id";
        comando.Parameters.AddWithValue("$id", id);
        comando.ExecuteNonQuery();
    }

    public bool TieneVentas(long id)
    {
        using var db = conexion.Abrir();
        using var comando = db.CreateCommand();
        comando.CommandText = "SELECT EXISTS (SELECT 1 FROM VentaLineas WHERE ProductoId = $id)";
        comando.Parameters.AddWithValue("$id", id);
        return (long)comando.ExecuteScalar()! == 1;
    }

    private Producto? ConsultarUno(string sql, (string Nombre, object Valor) parametro)
    {
        using var db = conexion.Abrir();
        using var comando = db.CreateCommand();
        comando.CommandText = sql;
        comando.Parameters.AddWithValue(parametro.Nombre, parametro.Valor);

        using var lector = comando.ExecuteReader();
        return lector.Read() ? Leer(lector) : null;
    }

    private static void CargarParametros(SqliteCommand comando, Producto producto)
    {
        comando.Parameters.AddWithValue("$codigo", producto.CodigoBarras);
        comando.Parameters.AddWithValue("$nombre", producto.Nombre);
        comando.Parameters.AddWithValue("$costo", ACentavos(producto.PrecioCosto));
        comando.Parameters.AddWithValue("$venta", ACentavos(producto.PrecioVenta));
        comando.Parameters.AddWithValue("$stock", producto.Stock);
        comando.Parameters.AddWithValue("$minimo", producto.StockMinimo);
        comando.Parameters.AddWithValue("$categoria", (object?)producto.CategoriaId ?? DBNull.Value);
        comando.Parameters.AddWithValue("$proveedor", (object?)producto.ProveedorId ?? DBNull.Value);
        comando.Parameters.AddWithValue("$paquete", (object?)producto.PaqueteId ?? DBNull.Value);
        comando.Parameters.AddWithValue("$unidades", producto.UnidadesPorPaquete);
        comando.Parameters.AddWithValue("$alVender", producto.PrecioAlVender);
    }

    private static Producto Leer(SqliteDataReader lector) => Producto.Restaurar(
        id: lector.GetInt64(0),
        codigoBarras: lector.GetString(1),
        nombre: lector.GetString(2),
        precioCosto: DeCentavos(lector.GetInt64(3)),
        precioVenta: DeCentavos(lector.GetInt64(4)),
        stock: lector.GetInt32(5),
        stockMinimo: lector.GetInt32(6),
        categoriaId: lector.IsDBNull(7) ? null : lector.GetInt64(7),
        proveedorId: lector.IsDBNull(8) ? null : lector.GetInt64(8),
        paqueteId: lector.IsDBNull(9) ? null : lector.GetInt64(9),
        unidadesPorPaquete: lector.GetInt32(10),
        paquetesCerrados: lector.GetInt32(11),
        precioAlVender: lector.GetBoolean(12));

    private static string EscaparLike(string texto) =>
        texto.Replace(@"\", @"\\").Replace("%", @"\%").Replace("_", @"\_");
}

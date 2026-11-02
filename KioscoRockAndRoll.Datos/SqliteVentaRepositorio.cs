using KioscoRockAndRoll.Dominio.Entidades;
using KioscoRockAndRoll.Dominio.Repositorios;
using static KioscoRockAndRoll.Datos.Conversiones;

namespace KioscoRockAndRoll.Datos;

public sealed class SqliteVentaRepositorio(ConexionSqlite conexion) : IVentaRepositorio
{
    public void Registrar(Venta venta)
    {
        using var db = conexion.Abrir();
        using var transaccion = db.BeginTransaction();

        using (var comandoVenta = db.CreateCommand())
        {
            comandoVenta.CommandText = """
                INSERT INTO Ventas (Fecha, MedioPago, TurnoId) VALUES ($fecha, $medio, $turno) RETURNING Id
                """;
            comandoVenta.Parameters.AddWithValue("$fecha", AFechaTexto(venta.Fecha));
            comandoVenta.Parameters.AddWithValue("$turno", venta.TurnoId);
            comandoVenta.Parameters.AddWithValue("$medio", (int)venta.MedioPrincipal);
            venta.AsignarId((long)comandoVenta.ExecuteScalar()!);
        }

        using (var comandoPago = db.CreateCommand())
        {
            comandoPago.CommandText = "INSERT INTO VentaPagos (VentaId, Medio, MontoCentavos) VALUES ($venta, $medio, $monto)";
            var pVentaPago = comandoPago.Parameters.Add("$venta", Microsoft.Data.Sqlite.SqliteType.Integer);
            var pMedio = comandoPago.Parameters.Add("$medio", Microsoft.Data.Sqlite.SqliteType.Integer);
            var pMonto = comandoPago.Parameters.Add("$monto", Microsoft.Data.Sqlite.SqliteType.Integer);
            foreach (var pago in venta.Pagos)
            {
                pVentaPago.Value = venta.Id;
                pMedio.Value = (int)pago.Medio;
                pMonto.Value = ACentavos(pago.Monto);
                comandoPago.ExecuteNonQuery();
            }
        }

        using var comandoLinea = db.CreateCommand();
        comandoLinea.CommandText = """
            INSERT INTO VentaLineas (VentaId, ProductoId, NombreProducto, Cantidad, PrecioUnitarioCentavos, CostoUnitarioCentavos)
            VALUES ($venta, $producto, $nombre, $cantidad, $precio, $costo);
            UPDATE Productos SET Stock = Stock - $cantidad WHERE Id = $producto AND PrecioAlVender = 0;
            """;
        var pVenta = comandoLinea.Parameters.Add("$venta", Microsoft.Data.Sqlite.SqliteType.Integer);
        var pProducto = comandoLinea.Parameters.Add("$producto", Microsoft.Data.Sqlite.SqliteType.Integer);
        var pNombre = comandoLinea.Parameters.Add("$nombre", Microsoft.Data.Sqlite.SqliteType.Text);
        var pCantidad = comandoLinea.Parameters.Add("$cantidad", Microsoft.Data.Sqlite.SqliteType.Integer);
        var pPrecio = comandoLinea.Parameters.Add("$precio", Microsoft.Data.Sqlite.SqliteType.Integer);
        var pCosto = comandoLinea.Parameters.Add("$costo", Microsoft.Data.Sqlite.SqliteType.Integer);

        foreach (var linea in venta.Lineas)
        {
            pVenta.Value = venta.Id;
            pProducto.Value = linea.ProductoId;
            pNombre.Value = linea.NombreProducto;
            pCantidad.Value = linea.Cantidad;
            pPrecio.Value = ACentavos(linea.PrecioUnitario);
            pCosto.Value = ACentavos(linea.CostoUnitario);
            comandoLinea.ExecuteNonQuery();
            AbrirPaquetesSiHaceFalta(db, linea.ProductoId, linea.Cantidad);
        }

        transaccion.Commit();
    }

    /// <summary>
    /// Si el producto se vende suelto y no alcanzaban las unidades sueltas, abre los paquetes necesarios:
    /// cada paquete abierto descuenta 1 del stock del paquete y suma sus unidades a las sueltas.
    /// Se ejecuta dentro de la transacción de la venta, después de descontar lo vendido.
    /// </summary>
    private static void AbrirPaquetesSiHaceFalta(Microsoft.Data.Sqlite.SqliteConnection db, long productoId, int cantidad)
    {
        using var consulta = db.CreateCommand();
        consulta.CommandText = """
            SELECT u.Stock, u.UnidadesPorPaquete, paq.Id, paq.Stock
            FROM Productos u
            JOIN Productos paq ON paq.Id = u.PaqueteId
            WHERE u.Id = $producto
            """;
        consulta.Parameters.AddWithValue("$producto", productoId);

        long paqueteId;
        int paquetes, unidadesPorPaquete;
        using (var lector = consulta.ExecuteReader())
        {
            if (!lector.Read())
                return; // no se vende suelto
            var sueltasAntes = lector.GetInt32(0) + cantidad;
            unidadesPorPaquete = lector.GetInt32(1);
            paqueteId = lector.GetInt64(2);
            paquetes = Producto.PaquetesQueHayQueAbrir(sueltasAntes, cantidad, unidadesPorPaquete, lector.GetInt32(3));
        }
        if (paquetes == 0)
            return;

        using var abrir = db.CreateCommand();
        abrir.CommandText = """
            UPDATE Productos SET Stock = Stock - $paquetes WHERE Id = $paquete;
            UPDATE Productos SET Stock = Stock + $unidades WHERE Id = $producto;
            """;
        abrir.Parameters.AddWithValue("$paquetes", paquetes);
        abrir.Parameters.AddWithValue("$paquete", paqueteId);
        abrir.Parameters.AddWithValue("$unidades", paquetes * unidadesPorPaquete);
        abrir.Parameters.AddWithValue("$producto", productoId);
        abrir.ExecuteNonQuery();
    }

    private const string SelectResumenes = """
        SELECT v.Id, v.Fecha, v.MedioPago,
               COALESCE(SUM(l.Cantidad), 0),
               COALESCE(SUM(l.Cantidad * l.PrecioUnitarioCentavos), 0),
               v.TurnoId, v.AnuladaEn, v.MotivoAnulacion
        FROM Ventas v
        LEFT JOIN VentaLineas l ON l.VentaId = v.Id
        """;

    public IReadOnlyList<ResumenVenta> ListarDelDia(DateOnly dia)
    {
        using var db = conexion.Abrir();
        using var comando = db.CreateCommand();
        comando.CommandText = $"""
            {SelectResumenes}
            WHERE v.Fecha >= $desde AND v.Fecha < $hasta
            GROUP BY v.Id
            ORDER BY v.Fecha
            """;
        comando.Parameters.AddWithValue("$desde", AFechaTexto(dia.ToDateTime(TimeOnly.MinValue)));
        comando.Parameters.AddWithValue("$hasta", AFechaTexto(dia.AddDays(1).ToDateTime(TimeOnly.MinValue)));

        var ventas = new List<ResumenVenta>();
        using (var lector = comando.ExecuteReader())
        {
            while (lector.Read())
                ventas.Add(LeerResumen(lector));
        }

        // Los pagos de todas las ventas del día, en una sola consulta.
        using var comandoPagos = db.CreateCommand();
        comandoPagos.CommandText = """
            SELECT p.VentaId, p.Medio, p.MontoCentavos
            FROM VentaPagos p
            JOIN Ventas v ON v.Id = p.VentaId
            WHERE v.Fecha >= $desde AND v.Fecha < $hasta
            ORDER BY p.Id
            """;
        comandoPagos.Parameters.AddWithValue("$desde", AFechaTexto(dia.ToDateTime(TimeOnly.MinValue)));
        comandoPagos.Parameters.AddWithValue("$hasta", AFechaTexto(dia.AddDays(1).ToDateTime(TimeOnly.MinValue)));
        var pagos = LeerPagos(comandoPagos);

        return ventas.Select(v => v with { Pagos = pagos.GetValueOrDefault(v.Id) }).ToList();
    }

    private static Dictionary<long, List<Pago>> LeerPagos(Microsoft.Data.Sqlite.SqliteCommand comando)
    {
        var pagos = new Dictionary<long, List<Pago>>();
        using var lector = comando.ExecuteReader();
        while (lector.Read())
        {
            var ventaId = lector.GetInt64(0);
            if (!pagos.TryGetValue(ventaId, out var deLaVenta))
                pagos[ventaId] = deLaVenta = [];
            deLaVenta.Add(new Pago((MedioDePago)lector.GetInt32(1), DeCentavos(lector.GetInt64(2))));
        }
        return pagos;
    }

    public IReadOnlyList<ProductoVendido> ProductosVendidosDelDia(DateOnly dia)
    {
        using var db = conexion.Abrir();
        using var comando = db.CreateCommand();
        // Se agrupa por producto; el nombre se toma de las ventas (es el que tenía al venderse).
        comando.CommandText = """
            SELECT MAX(l.NombreProducto), SUM(l.Cantidad), SUM(l.Cantidad * l.PrecioUnitarioCentavos) AS Total
            FROM VentaLineas l
            JOIN Ventas v ON v.Id = l.VentaId
            WHERE v.Fecha >= $desde AND v.Fecha < $hasta AND v.AnuladaEn IS NULL
            GROUP BY l.ProductoId
            ORDER BY Total DESC
            """;
        comando.Parameters.AddWithValue("$desde", AFechaTexto(dia.ToDateTime(TimeOnly.MinValue)));
        comando.Parameters.AddWithValue("$hasta", AFechaTexto(dia.AddDays(1).ToDateTime(TimeOnly.MinValue)));

        var productos = new List<ProductoVendido>();
        using var lector = comando.ExecuteReader();
        while (lector.Read())
            productos.Add(new ProductoVendido(lector.GetString(0), lector.GetInt32(1), DeCentavos(lector.GetInt64(2))));
        return productos;
    }

    public IReadOnlyList<VentasDeUnDia> TotalesPorDia(PeriodoReporte periodo)
    {
        using var db = conexion.Abrir();
        using var comando = db.CreateCommand();
        // Las fechas se guardan como "yyyy-MM-dd HH:mm:ss": los primeros 10 caracteres son el día.
        comando.CommandText = """
            SELECT substr(v.Fecha, 1, 10) AS Dia,
                   COUNT(DISTINCT v.Id),
                   SUM(l.Cantidad * l.PrecioUnitarioCentavos),
                   SUM(l.Cantidad * l.CostoUnitarioCentavos)
            FROM Ventas v
            JOIN VentaLineas l ON l.VentaId = v.Id
            WHERE v.Fecha >= $desde AND v.Fecha < $hasta AND v.AnuladaEn IS NULL
            GROUP BY Dia
            ORDER BY Dia
            """;
        CargarPeriodo(comando, periodo);

        var dias = new List<VentasDeUnDia>();
        using var lector = comando.ExecuteReader();
        while (lector.Read())
        {
            dias.Add(new VentasDeUnDia(
                DateOnly.ParseExact(lector.GetString(0), "yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture),
                lector.GetInt32(1), DeCentavos(lector.GetInt64(2)), DeCentavos(lector.GetInt64(3))));
        }
        return dias;
    }

    public IReadOnlyList<RendimientoProducto> RendimientoPorProducto(PeriodoReporte periodo)
    {
        using var db = conexion.Abrir();
        using var comando = db.CreateCommand();
        comando.CommandText = """
            SELECT l.ProductoId, MAX(l.NombreProducto), SUM(l.Cantidad),
                   SUM(l.Cantidad * l.PrecioUnitarioCentavos),
                   SUM(l.Cantidad * l.CostoUnitarioCentavos)
            FROM VentaLineas l
            JOIN Ventas v ON v.Id = l.VentaId
            WHERE v.Fecha >= $desde AND v.Fecha < $hasta AND v.AnuladaEn IS NULL
            GROUP BY l.ProductoId
            """;
        CargarPeriodo(comando, periodo);

        var productos = new List<RendimientoProducto>();
        using var lector = comando.ExecuteReader();
        while (lector.Read())
        {
            productos.Add(new RendimientoProducto(
                lector.GetInt64(0), lector.GetString(1), lector.GetInt32(2),
                DeCentavos(lector.GetInt64(3)), DeCentavos(lector.GetInt64(4))));
        }
        return productos;
    }

    private static void CargarPeriodo(Microsoft.Data.Sqlite.SqliteCommand comando, PeriodoReporte periodo)
    {
        comando.Parameters.AddWithValue("$desde", AFechaTexto(periodo.Desde.ToDateTime(TimeOnly.MinValue)));
        comando.Parameters.AddWithValue("$hasta", AFechaTexto(periodo.Hasta.AddDays(1).ToDateTime(TimeOnly.MinValue)));
    }

    public DetalleVenta? ObtenerDetalle(long ventaId)
    {
        using var db = conexion.Abrir();
        ResumenVenta resumen;
        using (var comando = db.CreateCommand())
        {
            comando.CommandText = $"{SelectResumenes} WHERE v.Id = $id GROUP BY v.Id";
            comando.Parameters.AddWithValue("$id", ventaId);
            using var lector = comando.ExecuteReader();
            if (!lector.Read())
                return null;
            resumen = LeerResumen(lector);
        }

        using var comandoLineas = db.CreateCommand();
        comandoLineas.CommandText = """
            SELECT ProductoId, NombreProducto, Cantidad, PrecioUnitarioCentavos, CostoUnitarioCentavos
            FROM VentaLineas WHERE VentaId = $id ORDER BY Id
            """;
        comandoLineas.Parameters.AddWithValue("$id", ventaId);

        var lineas = new List<LineaVenta>();
        using (var lectorLineas = comandoLineas.ExecuteReader())
        {
            while (lectorLineas.Read())
            {
                lineas.Add(new LineaVenta(
                    lectorLineas.GetInt64(0), lectorLineas.GetString(1), lectorLineas.GetInt32(2),
                    DeCentavos(lectorLineas.GetInt64(3)), DeCentavos(lectorLineas.GetInt64(4))));
            }
        }

        using var comandoPagos = db.CreateCommand();
        comandoPagos.CommandText = "SELECT VentaId, Medio, MontoCentavos FROM VentaPagos WHERE VentaId = $id ORDER BY Id";
        comandoPagos.Parameters.AddWithValue("$id", ventaId);
        var pagos = LeerPagos(comandoPagos);

        return new DetalleVenta(resumen with { Pagos = pagos.GetValueOrDefault(ventaId) }, lineas);
    }

    public void Anular(long ventaId, string motivo, DateTime fecha)
    {
        using var db = conexion.Abrir();
        using var transaccion = db.BeginTransaction();
        using var comando = db.CreateCommand();

        // "AnuladaEn IS NULL" evita anular dos veces aunque se llegue acá por dos caminos a la vez.
        comando.CommandText = """
            UPDATE Ventas SET AnuladaEn = $fecha, MotivoAnulacion = $motivo
            WHERE Id = $id AND AnuladaEn IS NULL
            """;
        comando.Parameters.AddWithValue("$fecha", AFechaTexto(fecha));
        comando.Parameters.AddWithValue("$motivo", motivo);
        comando.Parameters.AddWithValue("$id", ventaId);
        if (comando.ExecuteNonQuery() != 1)
            throw new InvalidOperationException($"La venta {ventaId} no existe o ya estaba anulada.");

        comando.CommandText = """
            UPDATE Productos
            SET Stock = Stock + (SELECT SUM(l.Cantidad) FROM VentaLineas l
                                 WHERE l.VentaId = $id AND l.ProductoId = Productos.Id)
            WHERE Id IN (SELECT ProductoId FROM VentaLineas WHERE VentaId = $id) AND PrecioAlVender = 0
            """;
        comando.ExecuteNonQuery();

        transaccion.Commit();
    }

    private static ResumenVenta LeerResumen(Microsoft.Data.Sqlite.SqliteDataReader lector) => new(
        Id: lector.GetInt64(0),
        Fecha: DeFechaTexto(lector.GetString(1)),
        MedioDePago: (MedioDePago)lector.GetInt32(2),
        Unidades: lector.GetInt32(3),
        Total: DeCentavos(lector.GetInt64(4)),
        TurnoId: lector.IsDBNull(5) ? null : lector.GetInt64(5),
        AnuladaEn: lector.IsDBNull(6) ? null : DeFechaTexto(lector.GetString(6)),
        MotivoAnulacion: lector.IsDBNull(7) ? null : lector.GetString(7));
}

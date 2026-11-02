using System.Text;
using KioscoRockAndRoll.Datos;
using KioscoRockAndRoll.Dominio.Entidades;
using KioscoRockAndRoll.Dominio.Servicios;
using KioscoRockAndRoll.Reportes;

namespace KioscoRockAndRoll.Tests.Integracion;

public sealed class ReporteDeVentasConSqliteTests : IDisposable
{
    private readonly BaseDeDatosDePrueba _base = new();
    private readonly InventarioServicio _inventario;
    private readonly CajaServicio _caja;
    private readonly VentaServicio _ventas;
    private readonly ReporteDeVentasServicio _reportes;
    private readonly string _carpeta = Path.Combine(Path.GetTempPath(), "KioscoTests", Guid.NewGuid().ToString("N"));

    public ReporteDeVentasConSqliteTests()
    {
        var reloj = TimeProvider.System;
        _inventario = new InventarioServicio(new SqliteProductoRepositorio(_base.Conexion));
        _caja = new CajaServicio(new SqliteCajaRepositorio(_base.Conexion), reloj);
        _caja.AbrirCaja(0);
        _ventas = new VentaServicio(new SqliteVentaRepositorio(_base.Conexion), _caja, reloj);
        _reportes = new ReporteDeVentasServicio(new SqliteVentaRepositorio(_base.Conexion), new SqliteProductoRepositorio(_base.Conexion));
    }

    public void Dispose()
    {
        _base.Dispose();
        try { Directory.Delete(_carpeta, recursive: true); } catch (IOException) { }
    }

    private static PeriodoReporte Hoy => PeriodoReporte.Dia(DateOnly.FromDateTime(DateTime.Now));

    private Venta Vender(Producto producto, int cantidad)
    {
        var carrito = new Carrito();
        carrito.Agregar(_inventario.ObtenerPorId(producto.Id)!, cantidad);
        return _ventas.Cobrar(carrito, Cobro.Crear(carrito.Total, MedioDePago.Efectivo));
    }

    [Fact]
    public void Ganancia_UsaElCostoDelMomentoDeLaVenta()
    {
        var coca = _inventario.Crear(new DatosProducto("1", "Coca", 1000, 1500, 50, 2));
        Vender(coca, 2);   // vendido 3000, costo 2000

        // Después aumenta el costo: la venta ya hecha no tiene que cambiar.
        _inventario.AplicarAjuste([coca.Id], new AjustePrecios(50, PreciosAAjustar.CostoYVenta));
        Vender(coca, 1);   // vendido 2250, costo 1500

        var reporte = _reportes.Generar(Hoy, Agrupacion.PorDia);

        Assert.Equal(5250, reporte.Vendido);
        Assert.Equal(3500, reporte.Costo);
        Assert.Equal(1750, reporte.Ganancia);
        var producto = Assert.Single(reporte.Productos);
        Assert.Equal(3, producto.Unidades);
    }

    [Fact]
    public void LasVentasAnuladasNoCuentan()
    {
        var coca = _inventario.Crear(new DatosProducto("1", "Coca", 1000, 1500, 50, 2));
        Vender(coca, 1);
        var anulada = Vender(coca, 5);
        _ventas.Anular(anulada.Id, "Error");

        var reporte = _reportes.Generar(Hoy, Agrupacion.PorDia);

        Assert.Equal(1, reporte.CantidadVentas);
        Assert.Equal(1500, reporte.Vendido);
        Assert.Equal(1, Assert.Single(reporte.Productos).Unidades);
    }

    [Fact]
    public void Productos_ConSuGananciaCadaUno()
    {
        var coca = _inventario.Crear(new DatosProducto("1", "Coca", 1000, 1500, 50, 2));
        var chicle = _inventario.Crear(new DatosProducto("2", "Chicle", 100, 300, 50, 2));
        Vender(coca, 1);
        Vender(chicle, 10);

        var porGanancia = _reportes.Generar(Hoy, Agrupacion.PorDia).ProductosOrdenados(OrdenProductos.PorGanancia);

        Assert.Equal(["Chicle", "Coca"], porGanancia.Select(p => p.Nombre));
        Assert.Equal(2000, porGanancia[0].Ganancia);
        Assert.Equal(2m, porGanancia[0].PorcentajeSobreCosto);   // 200%
    }

    [Fact]
    public void FueraDelPeriodo_NoAparece()
    {
        var coca = _inventario.Crear(new DatosProducto("1", "Coca", 1000, 1500, 50, 2));
        Vender(coca, 1);

        var ayer = PeriodoReporte.Dia(DateOnly.FromDateTime(DateTime.Now).AddDays(-1));
        var reporte = _reportes.Generar(ayer, Agrupacion.PorDia);

        Assert.Equal(0, reporte.Vendido);
        Assert.Empty(reporte.Productos);
        Assert.Single(reporte.Filas); // el día aparece igual, en cero
    }

    [Fact]
    public void Pdf_DelReporte_SeGenera()
    {
        var coca = _inventario.Crear(new DatosProducto("1", "Coca", 1000, 1500, 50, 2));
        Vender(coca, 3);
        var exportador = new ExportadorDeReportes(new ReporteDelDiaServicio(_ventas, _caja), TimeProvider.System);
        var reporte = _reportes.Generar(PeriodoReporte.MesHasta(DateOnly.FromDateTime(DateTime.Now)), Agrupacion.PorDia);
        var ruta = Path.Combine(_carpeta, ExportadorDeReportes.NombreDeArchivo(reporte.Periodo));

        exportador.GuardarReporte(reporte, OrdenProductos.PorUnidades, ruta);

        var bytes = File.ReadAllBytes(ruta);
        Assert.Equal("%PDF", Encoding.ASCII.GetString(bytes, 0, 4));
    }

    [Fact]
    public void Migracion_CompletaElCostoDeLasVentasViejas()
    {
        var carpeta = Path.Combine(Path.GetTempPath(), "KioscoTests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(carpeta);
        try
        {
            // Una base en la versión 5: las ventas todavía no guardan el costo.
            var conexion = new ConexionSqlite(Path.Combine(carpeta, "v5.db"));
            using (var db = conexion.Abrir())
            using (var comando = db.CreateCommand())
            {
                comando.CommandText = string.Join(";\n", InicializadorBaseDeDatos.Migraciones.Take(5)) + """
                    ;
                    PRAGMA user_version = 5;
                    INSERT INTO Productos (CodigoBarras, Nombre, PrecioCostoCentavos, PrecioVentaCentavos, Stock, StockMinimo)
                    VALUES ('1', 'Coca', 100000, 150000, 10, 2);
                    INSERT INTO Ventas (Fecha, MedioPago) VALUES ('2026-10-01 10:00:00', 1);
                    INSERT INTO VentaLineas (VentaId, ProductoId, NombreProducto, Cantidad, PrecioUnitarioCentavos)
                    VALUES (1, 1, 'Coca', 2, 150000);
                    """;
                comando.ExecuteNonQuery();
            }

            new InicializadorBaseDeDatos(conexion).Inicializar();

            var reporte = new ReporteDeVentasServicio(new SqliteVentaRepositorio(conexion), new SqliteProductoRepositorio(conexion))
                .Generar(PeriodoReporte.Dia(new DateOnly(2026, 10, 1)), Agrupacion.PorDia);
            Assert.Equal(3000, reporte.Vendido);
            Assert.Equal(2000, reporte.Costo); // se completó con el costo actual del producto
        }
        finally
        {
            try { Directory.Delete(carpeta, recursive: true); } catch (IOException) { }
        }
    }
}

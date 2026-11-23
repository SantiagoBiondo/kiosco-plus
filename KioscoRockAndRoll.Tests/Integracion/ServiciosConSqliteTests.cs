using KioscoRockAndRoll.Datos;
using KioscoRockAndRoll.Dominio;
using KioscoRockAndRoll.Dominio.Entidades;
using KioscoRockAndRoll.Dominio.Servicios;

namespace KioscoRockAndRoll.Tests.Integracion;

/// <summary>Prueba los servicios contra una base SQLite real (temporal).</summary>
public sealed class ServiciosConSqliteTests : IDisposable
{
    private static readonly DateTimeOffset Ahora = new(2026, 10, 20, 15, 30, 0, TimeSpan.Zero);

    private readonly BaseDeDatosDePrueba _base = new();
    private readonly SqliteProductoRepositorio _productos;
    private readonly InventarioServicio _inventario;
    private readonly VentaServicio _ventas;

    public ServiciosConSqliteTests()
    {
        _productos = new SqliteProductoRepositorio(_base.Conexion);
        _inventario = new InventarioServicio(_productos);
        var reloj = new RelojFijo(Ahora);
        var caja = new CajaServicio(new SqliteCajaRepositorio(_base.Conexion), reloj);
        caja.AbrirCaja(0); // para cobrar hace falta la caja abierta
        _ventas = new VentaServicio(new SqliteVentaRepositorio(_base.Conexion), caja, reloj);
    }

    public void Dispose() => _base.Dispose();

    private Producto CrearCoca(int stock = 10) =>
        _inventario.Crear(new DatosProducto("7790895000997", "Coca-Cola 500ml", 512.35m, 899.99m, stock, 3));

    [Fact]
    public void Crear_GuardaYRecuperaElProducto_ConPreciosExactos()
    {
        var creado = CrearCoca();

        var leido = _inventario.ObtenerPorCodigo("7790895000997");

        Assert.NotNull(leido);
        Assert.Equal(creado.Id, leido.Id);
        Assert.Equal(512.35m, leido.PrecioCosto);
        Assert.Equal(899.99m, leido.PrecioVenta);
        Assert.Equal(10, leido.Stock);
    }

    [Fact]
    public void Crear_CodigoRepetido_Falla()
    {
        CrearCoca();
        Assert.Throws<ReglaDeNegocioException>(() => CrearCoca());
    }

    [Fact]
    public void Modificar_CambiaDatosYStock()
    {
        var coca = CrearCoca();

        _inventario.Modificar(coca.Id, new DatosProducto("7790895000997", "Coca 500", 600, 1000, 25, 5));

        var leido = _inventario.ObtenerPorId(coca.Id)!;
        Assert.Equal("Coca 500", leido.Nombre);
        Assert.Equal(1000, leido.PrecioVenta);
        Assert.Equal(25, leido.Stock);
    }

    [Fact]
    public void IngresarMercaderia_SumaStock_OdevuelveNullSiNoExiste()
    {
        CrearCoca(stock: 10);

        var actualizado = _inventario.IngresarMercaderia("7790895000997", 24);

        Assert.Equal(34, actualizado!.Stock);
        Assert.Equal(34, _inventario.ObtenerPorCodigo("7790895000997")!.Stock);
        Assert.Null(_inventario.IngresarMercaderia("no-existe", 5));
    }

    [Fact]
    public void Buscar_PorParteDelNombre_SinDistinguirMayusculas()
    {
        CrearCoca();
        _inventario.Crear(new DatosProducto("111", "Alfajor 100% cacao", 100, 200, 0, 0));

        Assert.Single(_inventario.Buscar("coca"));
        Assert.Equal(2, _inventario.Buscar("").Count);
        Assert.Single(_inventario.Buscar("100%")); // el % se busca literal, no como comodín
    }

    [Fact]
    public void Cobrar_RegistraLaVenta_DescuentaStock_YVaciaElCarrito()
    {
        var coca = CrearCoca(stock: 10);
        var carrito = new Carrito();
        carrito.Agregar(coca, 3);

        var venta = _ventas.Cobrar(carrito, Cobro.Crear(carrito.Total, MedioDePago.Efectivo, 3000));

        Assert.True(venta.Id > 0);
        Assert.True(carrito.EstaVacio);
        Assert.Equal(7, _inventario.ObtenerPorId(coca.Id)!.Stock);

        var resumen = _ventas.ResumenDelDia(DateOnly.FromDateTime(Ahora.DateTime));
        var registrada = Assert.Single(resumen.Ventas);
        Assert.Equal(2699.97m, registrada.Total);
        Assert.Equal(3, registrada.Unidades);
        Assert.Equal(2699.97m, resumen.TotalPorMedio[MedioDePago.Efectivo]);
    }

    [Fact]
    public void ResumenDelDia_NoIncluyeOtrosDias()
    {
        var coca = CrearCoca();
        var carrito = new Carrito();
        carrito.Agregar(coca);
        _ventas.Cobrar(carrito, Cobro.Crear(carrito.Total, MedioDePago.Debito));

        var ayer = _ventas.ResumenDelDia(DateOnly.FromDateTime(Ahora.DateTime).AddDays(-1));

        Assert.Empty(ayer.Ventas);
        Assert.Equal(0, ayer.Total);
    }

    [Fact]
    public void Eliminar_ProductoConVentas_Falla()
    {
        var coca = CrearCoca();
        var carrito = new Carrito();
        carrito.Agregar(coca);
        _ventas.Cobrar(carrito, Cobro.Crear(carrito.Total, MedioDePago.Credito));

        Assert.Throws<ReglaDeNegocioException>(() => _inventario.Eliminar(coca.Id));
    }

    [Fact]
    public void Eliminar_ProductoSinVentas_LoBorra()
    {
        var coca = CrearCoca();

        _inventario.Eliminar(coca.Id);

        Assert.Null(_inventario.ObtenerPorId(coca.Id));
    }

    [Fact]
    public void AplicarAjuste_ActualizaSoloLosProductosIndicados()
    {
        var coca = CrearCoca();
        var alfajor = _inventario.Crear(new DatosProducto("111", "Alfajor", 400, 700, 0, 0));
        var chicle = _inventario.Crear(new DatosProducto("222", "Chicle", 100, 200, 0, 0));

        var actualizados = _inventario.AplicarAjuste([alfajor.Id, chicle.Id],
            new AjustePrecios(10, PreciosAAjustar.CostoYVenta, redondearVentaA: 10));

        Assert.Equal(2, actualizados);
        Assert.Equal(899.99m, _inventario.ObtenerPorId(coca.Id)!.PrecioVenta); // no se tocó
        var alfajorLeido = _inventario.ObtenerPorId(alfajor.Id)!;
        Assert.Equal(440m, alfajorLeido.PrecioCosto);
        Assert.Equal(770m, alfajorLeido.PrecioVenta);
        Assert.Equal(220m, _inventario.ObtenerPorId(chicle.Id)!.PrecioVenta);
    }

    [Fact]
    public void PrevisualizarAjuste_NoModificaNada()
    {
        var coca = CrearCoca();

        var cambios = _inventario.PrevisualizarAjuste([coca], new AjustePrecios(50, PreciosAAjustar.SoloVenta));

        var cambio = Assert.Single(cambios);
        Assert.Equal(899.99m, cambio.VentaAntes);
        Assert.Equal(1349.99m, cambio.VentaDespues); // 1349.985 redondeado
        Assert.Equal(899.99m, coca.PrecioVenta);
        Assert.Equal(899.99m, _inventario.ObtenerPorId(coca.Id)!.PrecioVenta);
    }

    [Fact]
    public void AplicarAjuste_APrecioFijo_DejaLasVariedadesAlMismoPrecio()
    {
        var coca = CrearCoca();
        var cofler = _inventario.Crear(new DatosProducto("111", "Cofler", 800, 1100, 0, 0));
        var coflerMani = _inventario.Crear(new DatosProducto("222", "Cofler con maní", 850, 1150, 0, 0));

        var actualizados = _inventario.AplicarAjuste([cofler.Id, coflerMani.Id],
            AjustePrecios.APrecioFijo(1200, PreciosAAjustar.SoloVenta));

        Assert.Equal(2, actualizados);
        Assert.Equal((800m, 1200m), Precios(cofler));
        Assert.Equal((850m, 1200m), Precios(coflerMani));   // el costo no se toca
        Assert.Equal(899.99m, _inventario.ObtenerPorId(coca.Id)!.PrecioVenta);

        (decimal Costo, decimal Venta) Precios(Producto p) =>
            (_inventario.ObtenerPorId(p.Id)!.PrecioCosto, _inventario.ObtenerPorId(p.Id)!.PrecioVenta);
    }

    [Fact]
    public void AplicarAjuste_SinProductos_Falla()
    {
        Assert.Throws<ReglaDeNegocioException>(() =>
            _inventario.AplicarAjuste([], new AjustePrecios(10, PreciosAAjustar.CostoYVenta)));
    }

    private sealed class RelojFijo(DateTimeOffset ahora) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => ahora;
        public override TimeZoneInfo LocalTimeZone => TimeZoneInfo.Utc;
    }
}

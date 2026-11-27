using KioscoRockAndRoll.Datos;
using KioscoRockAndRoll.Dominio;
using KioscoRockAndRoll.Dominio.Entidades;
using KioscoRockAndRoll.Dominio.Servicios;

namespace KioscoRockAndRoll.Tests.Integracion;

public sealed class CobroYPrecioAlVenderConSqliteTests : IDisposable
{
    private readonly BaseDeDatosDePrueba _base = new();
    private readonly InventarioServicio _inventario;
    private readonly VentaServicio _ventas;
    private readonly ReporteDeVentasServicio _reportes;
    private readonly AjustesDeCobro _ajustesDeCobro;
    private readonly Categoria _cigarrillos;
    private readonly Proveedor _arcor;
    private readonly Producto _atado;
    private readonly Producto _caramelos;
    private readonly Producto _coca;

    public CobroYPrecioAlVenderConSqliteTests()
    {
        var reloj = TimeProvider.System;
        var categorias = new CatalogoServicio<Categoria>(new SqliteCategoriaRepositorio(_base.Conexion));
        var proveedores = new CatalogoServicio<Proveedor>(new SqliteProveedorRepositorio(_base.Conexion));
        var productos = new SqliteProductoRepositorio(_base.Conexion);
        _inventario = new InventarioServicio(productos);
        var caja = new CajaServicio(new SqliteCajaRepositorio(_base.Conexion), reloj);
        _ventas = new VentaServicio(new SqliteVentaRepositorio(_base.Conexion), caja, reloj);
        _reportes = new ReporteDeVentasServicio(new SqliteVentaRepositorio(_base.Conexion), productos);
        _ajustesDeCobro = new AjustesDeCobro(new Ajustes(_base.Conexion), new SqliteCategoriaRepositorio(_base.Conexion));

        _cigarrillos = new Categoria("Cigarrillos");
        categorias.Guardar(_cigarrillos);
        _arcor = new Proveedor("Arcor");
        proveedores.Guardar(_arcor);

        _atado = _inventario.Crear(new DatosProducto("779001", "Marlboro x20", 4000, 5000, 10, 0, CategoriaId: _cigarrillos.Id));
        _caramelos = _inventario.Crear(new DatosProducto("301", "Caramelos sueltos", 1200, 1800, 0, 0,
            ProveedorId: _arcor.Id, PrecioAlVender: true));
        _coca = _inventario.Crear(new DatosProducto("779002", "Coca", 900, 1500, 10, 0));
        caja.AbrirCaja(0);
    }

    public void Dispose() => _base.Dispose();

    private Producto Leer(Producto producto) => _inventario.ObtenerPorId(producto.Id)!;
    private static DateOnly Hoy => DateOnly.FromDateTime(DateTime.Now);

    private Carrito CarritoCon(Producto producto, int cantidad, decimal? precio = null)
    {
        var carrito = new Carrito();
        carrito.Agregar(Leer(producto), cantidad, precio);
        return carrito;
    }

    [Fact]
    public void Reglas_PorDefecto_CigarrillosA200YMinimo500()
    {
        Assert.True(new ReglasDeCobro([new RecargoDeRubro(_cigarrillos.Id, 200)], 500).Iguales(_ajustesDeCobro.Leer()));
    }

    [Fact]
    public void Reglas_SeGuardanYSeLeen_ConVariosRubros()
    {
        var reglas = new ReglasDeCobro([new RecargoDeRubro(_cigarrillos.Id, 150), new RecargoDeRubro(99, 300.5m)], 700);

        _ajustesDeCobro.Guardar(reglas);

        Assert.True(reglas.Iguales(_ajustesDeCobro.Leer()));
    }

    [Fact]
    public void Reglas_SeGuardanYSeLeen_SinRubros()
    {
        _ajustesDeCobro.Guardar(new ReglasDeCobro([], 0));

        var leidas = _ajustesDeCobro.Leer();
        Assert.Empty(leidas.Recargos);
        Assert.Equal(0, leidas.MinimoTransferencia);
    }

    [Fact]
    public void Reglas_DeLaVersionAnterior_SeSiguenRespetando()
    {
        // Hasta la 1.8 se guardaba un solo rubro con su monto.
        var ajustes = new Ajustes(_base.Conexion);
        ajustes.Guardar("Cobro.CategoriaConRecargo", "42");
        ajustes.Guardar("Cobro.RecargoPorPaquete", "250");

        var leidas = _ajustesDeCobro.Leer();
        Assert.Equal([new RecargoDeRubro(42, 250)], leidas.Recargos);

        // Al guardar con el formato nuevo, lo anterior se borra.
        _ajustesDeCobro.Guardar(leidas);
        Assert.Null(ajustes.Leer("Cobro.CategoriaConRecargo"));
        Assert.Equal([new RecargoDeRubro(42, 250)], _ajustesDeCobro.Leer().Recargos);
    }

    [Fact]
    public void Reglas_DeLaVersionAnterior_SinRubro()
    {
        new Ajustes(_base.Conexion).Guardar("Cobro.CategoriaConRecargo", "ninguna");

        Assert.Empty(_ajustesDeCobro.Leer().Recargos);
    }

    [Fact]
    public void PagoPorTransferencia_ConDosRubros_CadaPaqueteConSuRecargo()
    {
        var categorias = new CatalogoServicio<Categoria>(new SqliteCategoriaRepositorio(_base.Conexion));
        var tabacos = new Categoria("Tabacos");
        categorias.Guardar(tabacos);
        var tabaco = _inventario.Crear(new DatosProducto("779009", "Tabaco 30g", 3000, 4000, 10, 0, CategoriaId: tabacos.Id));
        var reglas = new ReglasDeCobro([new RecargoDeRubro(_cigarrillos.Id, 200), new RecargoDeRubro(tabacos.Id, 300)], 500);
        var carrito = new Carrito();
        carrito.Agregar(Leer(_atado), 2);
        carrito.Agregar(Leer(tabaco), 1);

        var venta = _ventas.Cobrar(carrito, Cobro.Crear(reglas.TotalACobrar(carrito, true), MedioDePago.TransferenciaQr), reglas);

        Assert.Equal(14700, venta.Total);   // 2 × 5200 + 4300
        Assert.Equal([5200m, 4300m], _ventas.Detalle(venta.Id)!.Lineas.Select(l => l.PrecioUnitario));
    }

    [Fact]
    public void PagoPorTransferencia_RegistraElPaqueteConRecargo()
    {
        var reglas = _ajustesDeCobro.Leer();
        var carrito = CarritoCon(_atado, 2);
        var total = reglas.TotalACobrar(carrito, conTransferencia: true);

        var venta = _ventas.Cobrar(carrito, Cobro.Crear(total, MedioDePago.TransferenciaQr), reglas);

        Assert.Equal(10400, venta.Total);
        var linea = Assert.Single(_ventas.Detalle(venta.Id)!.Lineas);
        Assert.Equal(5200, linea.PrecioUnitario);
    }

    [Fact]
    public void PagoMixtoConTransferencia_LlevaElRecargoCompleto()
    {
        var reglas = _ajustesDeCobro.Leer();
        var carrito = CarritoCon(_atado, 2);
        var cobro = Cobro.Combinado(10400, [new Pago(MedioDePago.TransferenciaQr, 600)], efectivoRecibido: 10000);

        var venta = _ventas.Cobrar(carrito, cobro, reglas);

        Assert.Equal(10400, venta.Total);
        Assert.Equal(200, cobro.Vuelto);
    }

    [Fact]
    public void PagoEnEfectivo_NoLlevaRecargo()
    {
        var venta = _ventas.Cobrar(CarritoCon(_atado, 2), Cobro.Crear(10000, MedioDePago.Efectivo), _ajustesDeCobro.Leer());

        Assert.Equal(10000, venta.Total);
    }

    [Fact]
    public void TransferenciaMenorAlMinimo_NoSeRegistra()
    {
        var reglas = _ajustesDeCobro.Leer();
        var carrito = CarritoCon(_coca, 1);   // 1500
        var cobro = Cobro.Combinado(1500, [new Pago(MedioDePago.TransferenciaQr, 300)]);

        Assert.Throws<ReglaDeNegocioException>(() => _ventas.Cobrar(carrito, cobro, reglas));
        Assert.Empty(_ventas.ResumenDelDia(Hoy).Ventas);
        Assert.False(carrito.EstaVacio);
    }

    [Fact]
    public void PrecioAlVender_SeGuardaEnElProducto()
    {
        Assert.True(Leer(_caramelos).PrecioAlVender);
        Assert.False(Leer(_coca).PrecioAlVender);
    }

    [Fact]
    public void PrecioAlVender_NoTocaElStockNiAlAnular_YGuardaElCostoEstimado()
    {
        var venta = _ventas.Cobrar(CarritoCon(_caramelos, 1, 900), Cobro.Crear(900, MedioDePago.Efectivo));
        Assert.Equal(0, Leer(_caramelos).Stock);

        var vendido = Assert.Single(_reportes.VendidoPor(PeriodoReporte.Dia(Hoy), FiltroClasificacion.Todos, FiltroClasificacion.Todos));
        Assert.Equal(900, vendido.Vendido);
        Assert.Equal(600, vendido.Costo);

        _ventas.Anular(venta.Id, "Se equivocó");
        Assert.Equal(0, Leer(_caramelos).Stock);
    }

    [Fact]
    public void VendidoPor_FiltraPorRubroYPorProveedor()
    {
        _ventas.Cobrar(CarritoCon(_atado, 2), Cobro.Crear(10000, MedioDePago.Efectivo));
        _ventas.Cobrar(CarritoCon(_caramelos, 1, 900), Cobro.Crear(900, MedioDePago.Efectivo));
        _ventas.Cobrar(CarritoCon(_coca, 1), Cobro.Crear(1500, MedioDePago.Efectivo));
        var hoy = PeriodoReporte.Dia(Hoy);

        var deCigarrillos = _reportes.VendidoPor(hoy, FiltroClasificacion.Solo(_cigarrillos.Id), FiltroClasificacion.Todos);
        var deArcor = _reportes.VendidoPor(hoy, FiltroClasificacion.Todos, FiltroClasificacion.Solo(_arcor.Id));
        var sinRubro = _reportes.VendidoPor(hoy, FiltroClasificacion.SinAsignar, FiltroClasificacion.Todos);
        var todo = _reportes.VendidoPor(hoy, FiltroClasificacion.Todos, FiltroClasificacion.Todos);

        Assert.Equal(["Marlboro x20"], deCigarrillos.Select(r => r.Nombre));
        Assert.Equal(2, deCigarrillos[0].Unidades);
        Assert.Equal(["Caramelos sueltos"], deArcor.Select(r => r.Nombre));
        Assert.Equal(["Coca", "Caramelos sueltos"], sinRubro.Select(r => r.Nombre));
        Assert.Equal(["Marlboro x20", "Coca", "Caramelos sueltos"], todo.Select(r => r.Nombre));   // de mayor a menor
    }
}

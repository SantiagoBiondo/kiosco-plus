using KioscoRockAndRoll.Dominio;
using KioscoRockAndRoll.Dominio.Entidades;

namespace KioscoRockAndRoll.Tests.Dominio;

public class ReglasDeCobroTests
{
    private const long Cigarrillos = 7;
    private static readonly ReglasDeCobro Reglas = new(Cigarrillos, 200, 500);

    private static readonly Producto Atado = Producto.Restaurar(1, "779001", "Marlboro x20", 4000, 5000, 10, 0, categoriaId: Cigarrillos);
    private static readonly Producto Suelto = Producto.Restaurar(2, "101", "Cigarrillo suelto", 200, 300, 0, 0,
        categoriaId: Cigarrillos, paqueteId: 1, unidadesPorPaquete: 20);
    private static readonly Producto Coca = Producto.Restaurar(3, "779002", "Coca", 900, 1500, 10, 0, categoriaId: 8);

    private static Carrito Carrito(params (Producto Producto, int Cantidad)[] items)
    {
        var carrito = new Carrito();
        foreach (var (producto, cantidad) in items)
            carrito.Agregar(producto, cantidad);
        return carrito;
    }

    [Fact]
    public void Recargo_SoloParaLosPaquetesDelRubro()
    {
        var carrito = Carrito((Atado, 2), (Suelto, 3), (Coca, 1));

        Assert.Equal(2, Reglas.PaquetesConRecargo(carrito));
        Assert.Equal(400, Reglas.RecargoPorTransferencia(carrito));
    }

    [Fact]
    public void TotalACobrar_SumaElRecargoSoloConTransferencia()
    {
        var carrito = Carrito((Atado, 2), (Coca, 1));   // 10000 + 1500

        Assert.Equal(11500, Reglas.TotalACobrar(carrito, conTransferencia: false));
        Assert.Equal(11900, Reglas.TotalACobrar(carrito, conTransferencia: true));
    }

    [Fact]
    public void SinRubroElegido_NoHayRecargo()
    {
        var carrito = Carrito((Atado, 2));
        Assert.Equal(0, new ReglasDeCobro(null, 200, 500).RecargoPorTransferencia(carrito));
    }

    [Fact]
    public void ElPrecioDelRenglon_IncluyeElRecargo()
    {
        var carrito = Carrito((Atado, 1), (Coca, 1));

        Assert.Equal(5200, Reglas.PrecioUnitario(carrito.Items[0], conTransferencia: true));
        Assert.Equal(5000, Reglas.PrecioUnitario(carrito.Items[0], conTransferencia: false));
        Assert.Equal(1500, Reglas.PrecioUnitario(carrito.Items[1], conTransferencia: true));
    }

    [Theory]
    [InlineData(499.99, false)]
    [InlineData(500, true)]
    [InlineData(2000, true)]
    public void MinimoParaTransferir(decimal transferido, bool permitido)
    {
        var cobro = Cobro.Combinado(3000, [new Pago(MedioDePago.TransferenciaQr, transferido)]);

        if (permitido)
            Reglas.ValidarTransferencia(cobro);
        else
            Assert.Throws<ReglaDeNegocioException>(() => Reglas.ValidarTransferencia(cobro));
    }

    [Fact]
    public void SinTransferencia_NoImportaElMinimo()
    {
        Reglas.ValidarTransferencia(Cobro.Crear(100, MedioDePago.Efectivo));
        Reglas.ValidarTransferencia(Cobro.Crear(100, MedioDePago.Debito));
    }
}

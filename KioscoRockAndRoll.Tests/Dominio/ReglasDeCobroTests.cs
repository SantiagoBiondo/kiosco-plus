using KioscoRockAndRoll.Dominio;
using KioscoRockAndRoll.Dominio.Entidades;

namespace KioscoRockAndRoll.Tests.Dominio;

public class ReglasDeCobroTests
{
    private const long Cigarrillos = 7;
    private const long Tabacos = 9;
    private static readonly ReglasDeCobro Reglas = new([new RecargoDeRubro(Cigarrillos, 200)], 500);

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
        Assert.Equal(0, new ReglasDeCobro([], 500).RecargoPorTransferencia(carrito));
    }

    // ---------- Varios rubros ----------

    private static readonly Producto Tabaco = Producto.Restaurar(4, "779003", "Tabaco armar 30g", 3000, 4000, 10, 0, categoriaId: Tabacos);
    private static readonly ReglasDeCobro DosRubros =
        new([new RecargoDeRubro(Cigarrillos, 200), new RecargoDeRubro(Tabacos, 300)], 500);

    [Fact]
    public void VariosRubros_CadaUnoConSuMonto()
    {
        var carrito = Carrito((Atado, 2), (Tabaco, 1), (Coca, 1));

        Assert.Equal(200, DosRubros.RecargoDe(Atado));
        Assert.Equal(300, DosRubros.RecargoDe(Tabaco));
        Assert.Equal(0, DosRubros.RecargoDe(Coca));
        Assert.Equal(3, DosRubros.PaquetesConRecargo(carrito));
        Assert.Equal(700, DosRubros.RecargoPorTransferencia(carrito));   // 2 × 200 + 1 × 300
        Assert.Equal(4300, DosRubros.PrecioUnitario(carrito.Items[1], conTransferencia: true));
    }

    [Fact]
    public void UnRubroNoPuedeEstarDosVeces()
    {
        Assert.Throws<ReglaDeNegocioException>(() =>
            new ReglasDeCobro([new RecargoDeRubro(Cigarrillos, 200), new RecargoDeRubro(Cigarrillos, 300)], 0));
    }

    [Fact]
    public void LosMontosNoPuedenSerNegativos()
    {
        Assert.Throws<ReglaDeNegocioException>(() => new ReglasDeCobro([new RecargoDeRubro(Cigarrillos, -1)], 0));
        Assert.Throws<ReglaDeNegocioException>(() => new ReglasDeCobro([], -1));
    }

    [Fact]
    public void UnRecargoEnCero_NoCuenta()
    {
        Assert.Empty(new ReglasDeCobro([new RecargoDeRubro(Cigarrillos, 0)], 0).Recargos);
    }

    [Fact]
    public void Iguales_SinImportarElOrden()
    {
        var otroOrden = new ReglasDeCobro([new RecargoDeRubro(Tabacos, 300), new RecargoDeRubro(Cigarrillos, 200)], 500);

        Assert.True(DosRubros.Iguales(otroOrden));
        Assert.False(DosRubros.Iguales(Reglas));
        Assert.False(DosRubros.Iguales(new ReglasDeCobro(DosRubros.Recargos, 1000)));
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

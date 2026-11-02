using KioscoRockAndRoll.Dominio;
using KioscoRockAndRoll.Dominio.Entidades;

namespace KioscoRockAndRoll.Tests.Dominio;

public class PrecioAlVenderTests
{
    // Referencia: 100 g de caramelos cuestan 1200 y se venden a 1800.
    private static readonly Producto Caramelos =
        Producto.Restaurar(1, "301", "Caramelos sueltos", 1200, 1800, 0, 0, precioAlVender: true);

    private static readonly Producto Coca = Producto.Restaurar(2, "779", "Coca", 900, 1500, 10, 0);

    [Fact]
    public void SinPrecio_NoSePuedeAgregar()
    {
        Assert.Throws<ReglaDeNegocioException>(() => new Carrito().Agregar(Caramelos));
        Assert.Throws<ReglaDeNegocioException>(() => new Carrito().Agregar(Caramelos, precio: 0));
    }

    [Fact]
    public void CadaVezVaEnUnRenglonAparte_ConSuPrecio()
    {
        var carrito = new Carrito();

        carrito.Agregar(Caramelos, precio: 900);
        carrito.Agregar(Caramelos, precio: 1800);
        carrito.Agregar(Coca);

        Assert.Equal(3, carrito.Items.Count);
        Assert.Equal([900m, 1800m, 1500m], carrito.Items.Select(i => i.PrecioUnitario));
        Assert.Equal(4200, carrito.Total);
    }

    [Fact]
    public void ElCosto_SeEstimaConLaProporcionDeReferencia()
    {
        var item = new Carrito().Agregar(Caramelos, precio: 900);   // la mitad de 100 g

        Assert.Equal(600, item.CostoUnitario);
    }

    [Fact]
    public void SinPrecioDeReferencia_ElCostoEsCero()
    {
        var sinReferencia = Producto.Restaurar(3, "302", "Varios", 0, 0, 0, 0, precioAlVender: true);
        Assert.Equal(0, new Carrito().Agregar(sinReferencia, precio: 500).CostoUnitario);
    }

    [Fact]
    public void NoControlaStock()
    {
        Assert.False(Caramelos.ControlaStock);
        Assert.False(Caramelos.StockBajo);
        Assert.False(new Carrito().Agregar(Caramelos, 5, 100).SuperaElStock);
    }

    [Fact]
    public void UnProductoDePrecioFijo_NoAceptaPrecio()
    {
        Assert.Throws<ArgumentException>(() => new Carrito().Agregar(Coca, precio: 100));
    }

    [Fact]
    public void SeCambiaYQuitaPorRenglon()
    {
        var carrito = new Carrito();
        var chica = carrito.Agregar(Caramelos, precio: 900);
        var grande = carrito.Agregar(Caramelos, precio: 1800);

        carrito.CambiarCantidad(chica, 2);
        carrito.Quitar(grande);

        Assert.Equal([chica], carrito.Items);
        Assert.Equal(1800, carrito.Total);
    }

    [Fact]
    public void NoPuedeSerUnidadSueltaDeUnPaquete()
    {
        var suelto = Producto.Restaurar(4, "101", "Cigarrillo suelto", 200, 300, 0, 0, paqueteId: 9, unidadesPorPaquete: 20);
        Assert.Throws<ReglaDeNegocioException>(() => suelto.DefinirPrecioAlVender(true));
    }
}

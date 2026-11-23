using KioscoRockAndRoll.Dominio;
using KioscoRockAndRoll.Dominio.Entidades;

namespace KioscoRockAndRoll.Tests.Dominio;

public class AjustePreciosTests
{
    [Fact]
    public void Aumento_SobreCostoYVenta()
    {
        var ajuste = new AjustePrecios(15, PreciosAAjustar.CostoYVenta);

        Assert.Equal(575m, ajuste.NuevoCosto(500));
        Assert.Equal(919.99m, ajuste.NuevaVenta(799.99m)); // 919.9885 redondeado a 2 decimales
    }

    [Fact]
    public void Baja_Porcentaje_Negativo()
    {
        var ajuste = new AjustePrecios(-10, PreciosAAjustar.CostoYVenta);
        Assert.Equal(900m, ajuste.NuevaVenta(1000));
    }

    [Fact]
    public void SoloVenta_NoTocaElCosto()
    {
        var ajuste = new AjustePrecios(20, PreciosAAjustar.SoloVenta);

        Assert.Equal(500m, ajuste.NuevoCosto(500));
        Assert.Equal(1200m, ajuste.NuevaVenta(1000));
    }

    [Fact]
    public void SoloCosto_NoTocaLaVenta()
    {
        var ajuste = new AjustePrecios(20, PreciosAAjustar.SoloCosto, redondearVentaA: 100);

        Assert.Equal(600m, ajuste.NuevoCosto(500));
        Assert.Equal(1000m, ajuste.NuevaVenta(1000));
    }

    // 1073.52 * 1.15 = 1234.548
    [Theory]
    [InlineData(1, 1235)]
    [InlineData(10, 1240)]
    [InlineData(50, 1250)]
    [InlineData(100, 1300)]
    public void Redondeo_HaciaArriba_SoloEnLaVenta(decimal multiplo, decimal esperado)
    {
        var ajuste = new AjustePrecios(15, PreciosAAjustar.CostoYVenta, multiplo);

        Assert.Equal(esperado, ajuste.NuevaVenta(1073.52m));
        Assert.Equal(1234.55m, ajuste.NuevoCosto(1073.52m)); // el costo no se redondea a múltiplos
    }

    [Fact]
    public void Redondeo_NoCambiaUnPrecioQueYaEsMultiplo()
    {
        var ajuste = new AjustePrecios(25, PreciosAAjustar.SoloVenta, 10);
        Assert.Equal(1000m, ajuste.NuevaVenta(800));
    }

    [Fact]
    public void PrecioCero_SigueEnCero()
    {
        var ajuste = new AjustePrecios(30, PreciosAAjustar.CostoYVenta, 10);

        Assert.Equal(0m, ajuste.NuevoCosto(0));
        Assert.Equal(0m, ajuste.NuevaVenta(0));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-100)]
    [InlineData(-150)]
    [InlineData(1001)]
    public void PorcentajeInvalido_Falla(decimal porcentaje)
    {
        Assert.Throws<ReglaDeNegocioException>(() => new AjustePrecios(porcentaje, PreciosAAjustar.CostoYVenta));
    }

    [Fact]
    public void Producto_AjustarPrecios()
    {
        var producto = new Producto("779", "Coca", 500, 800);

        producto.AjustarPrecios(new AjustePrecios(10, PreciosAAjustar.CostoYVenta, 10));

        Assert.Equal(550m, producto.PrecioCosto);
        Assert.Equal(880m, producto.PrecioVenta);
    }

    // ---------- Precio fijo ----------

    [Fact]
    public void PrecioFijo_DeVenta_IgualParaTodos_YNoTocaElCosto()
    {
        var ajuste = AjustePrecios.APrecioFijo(1200, PreciosAAjustar.SoloVenta);

        Assert.True(ajuste.EsPrecioFijo);
        Assert.Equal(1200m, ajuste.NuevaVenta(1100));
        Assert.Equal(1200m, ajuste.NuevaVenta(1350));
        Assert.Equal(800m, ajuste.NuevoCosto(800));
    }

    [Fact]
    public void PrecioFijo_DeCosto_NoTocaLaVenta()
    {
        var ajuste = AjustePrecios.APrecioFijo(750.5m, PreciosAAjustar.SoloCosto);

        Assert.Equal(750.5m, ajuste.NuevoCosto(700));
        Assert.Equal(1100m, ajuste.NuevaVenta(1100));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-5)]
    public void PrecioFijo_TieneQueSerMayorACero(decimal precio)
    {
        Assert.Throws<ReglaDeNegocioException>(() => AjustePrecios.APrecioFijo(precio, PreciosAAjustar.SoloVenta));
    }

    [Fact]
    public void PrecioFijo_NoPuedeSerCostoYVenta()
    {
        Assert.Throws<ReglaDeNegocioException>(() => AjustePrecios.APrecioFijo(1000, PreciosAAjustar.CostoYVenta));
    }

    [Fact]
    public void Descripcion_ParaConfirmar()
    {
        Assert.Equal("+15% (costo y venta)", new AjustePrecios(15, PreciosAAjustar.CostoYVenta).Descripcion);
        Assert.Equal("-10% (solo precio de venta)", new AjustePrecios(-10, PreciosAAjustar.SoloVenta).Descripcion);
        Assert.Equal($"precio de venta {1200m:C2}", AjustePrecios.APrecioFijo(1200, PreciosAAjustar.SoloVenta).Descripcion);
    }
}

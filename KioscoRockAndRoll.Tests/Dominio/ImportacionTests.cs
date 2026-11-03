using KioscoRockAndRoll.Dominio.Entidades;

namespace KioscoRockAndRoll.Tests.Dominio;

public class ImportacionTests
{
    [Theory]
    [InlineData("1500", 1500)]
    [InlineData("1.500", 1500)]          // así se escribe mil quinientos acá
    [InlineData("1.234.567", 1234567)]
    [InlineData("1.234,50", 1234.50)]
    [InlineData("1,234.50", 1234.50)]
    [InlineData("1500,5", 1500.5)]
    [InlineData("12.75", 12.75)]
    [InlineData("$ 1.650,00", 1650)]
    [InlineData(" 880 ", 880)]
    [InlineData("-3", -3)]
    public void NumeroImportado_LeeLosFormatosDeAca(string texto, decimal esperado)
    {
        Assert.True(NumeroImportado.TryLeer(texto, out var numero));
        Assert.Equal(esperado, numero);
    }

    [Theory]
    [InlineData("")]
    [InlineData("abc")]
    [InlineData("$")]
    public void NumeroImportado_SinNumero_NoLee(string texto)
    {
        Assert.False(NumeroImportado.TryLeer(texto, out _));
    }

    [Fact]
    public void Detectar_ReconoceTitulosHabituales()
    {
        var mapeo = MapeoDeColumnas.Detectar(["Cód. Barras", "Descripción", "Precio Costo", "Precio", "Existencia", "Rubro", "Proveedor"]);

        Assert.Equal(0, mapeo.Columna(CampoImportable.Codigo));
        Assert.Equal(1, mapeo.Columna(CampoImportable.Nombre));
        Assert.Equal(2, mapeo.Columna(CampoImportable.Costo));
        Assert.Equal(3, mapeo.Columna(CampoImportable.Venta));
        Assert.Equal(4, mapeo.Columna(CampoImportable.Stock));
        Assert.Equal(5, mapeo.Columna(CampoImportable.Categoria));
        Assert.Equal(6, mapeo.Columna(CampoImportable.Proveedor));
        Assert.Empty(mapeo.FaltanObligatorios());
    }

    [Fact]
    public void Detectar_TitulosQueContienenLaPalabra()
    {
        var mapeo = MapeoDeColumnas.Detectar(["Artículo", "Código interno", "Precio de venta final", "Stock actual"]);

        Assert.Equal(0, mapeo.Columna(CampoImportable.Nombre));
        Assert.Equal(1, mapeo.Columna(CampoImportable.Codigo));
        Assert.Equal(2, mapeo.Columna(CampoImportable.Venta));
        Assert.Equal(3, mapeo.Columna(CampoImportable.Stock));
        Assert.Null(mapeo.Columna(CampoImportable.Costo));
    }

    [Fact]
    public void Detectar_TitulosDesconocidos_PideLosObligatorios()
    {
        var mapeo = MapeoDeColumnas.Detectar(["Columna1", "Columna2"]);

        Assert.Equal([CampoImportable.Codigo, CampoImportable.Nombre, CampoImportable.Venta], mapeo.FaltanObligatorios());
    }

    [Fact]
    public void Asignar_UnaColumnaNoPuedeSerDosDatos()
    {
        var mapeo = new MapeoDeColumnas();
        mapeo.Asignar(CampoImportable.Costo, 2);

        mapeo.Asignar(CampoImportable.Venta, 2);

        Assert.Null(mapeo.Columna(CampoImportable.Costo));
        Assert.Equal(2, mapeo.Columna(CampoImportable.Venta));
    }

    [Theory]
    [InlineData(4, 1, 3, 4, "4 productos nuevos y 1 actualizado. Se crearon 3 rubros y 4 proveedores.")]
    [InlineData(1, 0, 1, 0, "1 producto nuevo. Se creó 1 rubro.")]
    [InlineData(0, 5, 0, 0, "0 productos nuevos y 5 actualizados.")]
    public void ResultadoImportacion_Descripcion(int nuevos, int actualizados, int rubros, int proveedores, string esperado)
    {
        Assert.Equal(esperado, new ResultadoImportacion(nuevos, actualizados, rubros, proveedores).Descripcion);
    }

    [Theory]
    [InlineData(0, "A")]
    [InlineData(25, "Z")]
    [InlineData(26, "AA")]
    [InlineData(27, "AB")]
    public void LetraDeColumna_ComoEnExcel(int columna, string letra)
    {
        Assert.Equal(letra, TablaImportada.LetraDeColumna(columna));
    }
}

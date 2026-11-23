namespace KioscoRockAndRoll.Dominio.Entidades;

public enum PreciosAAjustar
{
    CostoYVenta = 1,
    SoloVenta = 2,
    SoloCosto = 3
}

public static class PreciosAAjustarExtensiones
{
    public static string Descripcion(this PreciosAAjustar precios) => precios switch
    {
        PreciosAAjustar.CostoYVenta => "Costo y venta",
        PreciosAAjustar.SoloVenta => "Solo precio de venta",
        PreciosAAjustar.SoloCosto => "Solo precio de costo",
        _ => precios.ToString()
    };
}

/// <summary>
/// Un cambio de precios para varios productos a la vez. Puede ser:
/// <list type="bullet">
/// <item>Porcentual: por ejemplo "+15% a costo y venta, redondeando la venta hacia arriba a múltiplos de $10".</item>
/// <item>A un precio fijo (<see cref="APrecioFijo"/>): por ejemplo "venta a $1.200" para variedades que valen lo mismo.</item>
/// </list>
/// </summary>
public sealed class AjustePrecios
{
    public const decimal PorcentajeMaximo = 1000;

    /// <summary>Todos los productos quedan con el mismo precio de venta (o de costo).</summary>
    /// <param name="precios">Solo venta o solo costo: un mismo número no puede ser los dos.</param>
    public static AjustePrecios APrecioFijo(decimal precio, PreciosAAjustar precios)
    {
        if (precio <= 0)
            throw new ReglaDeNegocioException("Ingresá el precio nuevo.");
        if (precios == PreciosAAjustar.CostoYVenta)
            throw new ReglaDeNegocioException("Con un precio fijo, elegí si es el de venta o el de costo.");
        if (!Enum.IsDefined(precios))
            throw new ArgumentOutOfRangeException(nameof(precios));
        return new AjustePrecios(0, precios, 0, decimal.Round(precio, 2, MidpointRounding.AwayFromZero));
    }

    private AjustePrecios(decimal porcentaje, PreciosAAjustar precios, decimal redondearVentaA, decimal precioFijo)
    {
        Porcentaje = porcentaje;
        Precios = precios;
        RedondearVentaA = redondearVentaA;
        PrecioFijo = precioFijo;
    }

    /// <param name="porcentaje">Positivo para aumentar, negativo para bajar. 15 = +15%.</param>
    /// <param name="redondearVentaA">
    /// Si es mayor a cero, el precio de venta se redondea hacia arriba a múltiplos de este valor
    /// (por ejemplo 10: $1.234 pasa a $1.240). El costo nunca se redondea.
    /// </param>
    public AjustePrecios(decimal porcentaje, PreciosAAjustar precios, decimal redondearVentaA = 0)
    {
        if (porcentaje == 0)
            throw new ReglaDeNegocioException("Ingresá un porcentaje distinto de cero.");
        if (porcentaje <= -100)
            throw new ReglaDeNegocioException("No se puede bajar un 100% o más.");
        if (porcentaje > PorcentajeMaximo)
            throw new ReglaDeNegocioException($"El aumento no puede superar el {PorcentajeMaximo}%.");
        if (!Enum.IsDefined(precios))
            throw new ArgumentOutOfRangeException(nameof(precios));
        if (redondearVentaA < 0)
            throw new ArgumentOutOfRangeException(nameof(redondearVentaA));

        Porcentaje = porcentaje;
        Precios = precios;
        RedondearVentaA = redondearVentaA;
    }

    public decimal Porcentaje { get; }
    public PreciosAAjustar Precios { get; }
    public decimal RedondearVentaA { get; }

    /// <summary>El precio que quedan todos, o 0 si el ajuste es porcentual.</summary>
    public decimal PrecioFijo { get; }

    public bool EsPrecioFijo => PrecioFijo > 0;

    public bool AjustaCosto => Precios != PreciosAAjustar.SoloVenta;
    public bool AjustaVenta => Precios != PreciosAAjustar.SoloCosto;

    /// <summary>"+15% (costo y venta)" o "precio de venta $ 1.200,00", para confirmar antes de aplicar.</summary>
    public string Descripcion => EsPrecioFijo
        ? $"{(AjustaVenta ? "precio de venta" : "precio de costo")} {PrecioFijo:C2}"
        : $"{(Porcentaje > 0 ? "+" : "")}{Porcentaje:0.##}% ({Precios.Descripcion().ToLower()})";

    public decimal NuevoCosto(decimal costo)
    {
        if (!AjustaCosto)
            return costo;
        return EsPrecioFijo ? PrecioFijo : decimal.Round(Aumentar(costo), 2, MidpointRounding.AwayFromZero);
    }

    public decimal NuevaVenta(decimal venta)
    {
        if (!AjustaVenta)
            return venta;
        if (EsPrecioFijo)
            return PrecioFijo;

        var nueva = Aumentar(venta);
        return RedondearVentaA > 0
            ? Math.Ceiling(nueva / RedondearVentaA) * RedondearVentaA
            : decimal.Round(nueva, 2, MidpointRounding.AwayFromZero);
    }

    private decimal Aumentar(decimal precio) => precio * (1 + Porcentaje / 100);
}

/// <summary>Cómo quedaría un producto después de un ajuste (para mostrar antes de aplicarlo).</summary>
public sealed record CambioDePrecio(
    long ProductoId, string Nombre,
    decimal CostoAntes, decimal CostoDespues,
    decimal VentaAntes, decimal VentaDespues);

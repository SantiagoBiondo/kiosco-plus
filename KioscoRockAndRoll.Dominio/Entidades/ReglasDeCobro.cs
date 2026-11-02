namespace KioscoRockAndRoll.Dominio.Entidades;

/// <summary>
/// Reglas del kiosco para cobrar por transferencia:
/// <list type="bullet">
/// <item>Cada paquete de un rubro (por ejemplo, Cigarrillos) se cobra más caro si se paga por transferencia,
/// aunque sea solo una parte. Los sueltos y los de precio al vender no llevan recargo.</item>
/// <item>Lo que se paga por transferencia no puede ser menos que un mínimo.</item>
/// </list>
/// </summary>
/// <param name="CategoriaConRecargo">El rubro cuyos paquetes llevan recargo (null = ninguno).</param>
/// <param name="RecargoPorPaquete">Cuánto se suma a cada paquete.</param>
/// <param name="MinimoTransferencia">Lo mínimo que se puede pagar por transferencia (0 = sin mínimo).</param>
public sealed record ReglasDeCobro(long? CategoriaConRecargo, decimal RecargoPorPaquete, decimal MinimoTransferencia)
{
    public static ReglasDeCobro Ninguna { get; } = new(null, 0, 0);

    public bool LlevaRecargo(Producto producto) =>
        CategoriaConRecargo is { } categoria
        && RecargoPorPaquete > 0
        && producto.CategoriaId == categoria
        && !producto.SeVendeSuelto
        && !producto.PrecioAlVender;

    public int PaquetesConRecargo(Carrito carrito) =>
        carrito.Items.Where(i => LlevaRecargo(i.Producto)).Sum(i => i.Cantidad);

    /// <summary>Cuánto más se cobra si se paga (todo o una parte) por transferencia.</summary>
    public decimal RecargoPorTransferencia(Carrito carrito) => PaquetesConRecargo(carrito) * RecargoPorPaquete;

    /// <summary>El total a cobrar, según se pague algo por transferencia o no.</summary>
    public decimal TotalACobrar(Carrito carrito, bool conTransferencia) =>
        carrito.Total + (conTransferencia ? RecargoPorTransferencia(carrito) : 0);

    /// <summary>El precio de cada unidad de un renglón, con el recargo si corresponde.</summary>
    public decimal PrecioUnitario(ItemCarrito item, bool conTransferencia) =>
        item.PrecioUnitario + (conTransferencia && LlevaRecargo(item.Producto) ? RecargoPorPaquete : 0);

    public static decimal Transferido(IEnumerable<Pago> pagos) =>
        pagos.Where(p => p.Medio == MedioDePago.TransferenciaQr).Sum(p => p.Monto);

    public static bool UsaTransferencia(IEnumerable<Pago> pagos) => Transferido(pagos) > 0;

    /// <summary>Controla el mínimo para transferir.</summary>
    /// <exception cref="ReglaDeNegocioException">Si se transfiere menos que el mínimo.</exception>
    public void ValidarTransferencia(Cobro cobro)
    {
        var transferido = Transferido(cobro.Pagos);
        if (transferido > 0 && transferido < MinimoTransferencia)
            throw new ReglaDeNegocioException(
                $"Por transferencia se puede pagar desde {MinimoTransferencia:C0} (ahora son {transferido:C2}).");
    }
}

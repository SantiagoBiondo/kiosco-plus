namespace KioscoRockAndRoll.Dominio.Entidades;

/// <summary>Un rubro (por ejemplo, Cigarrillos) cuyos paquetes se cobran más caros al pagar por transferencia.</summary>
/// <param name="CategoriaId">El rubro.</param>
/// <param name="PorPaquete">Cuánto se suma a cada paquete.</param>
public sealed record RecargoDeRubro(long CategoriaId, decimal PorPaquete);

/// <summary>
/// Reglas del kiosco para cobrar por transferencia:
/// <list type="bullet">
/// <item>Cada paquete de los rubros con recargo (por ejemplo, Cigarrillos $200 y Tabacos $300) se cobra más caro
/// si se paga por transferencia, aunque sea solo una parte. Los sueltos y los de precio al vender no llevan recargo.</item>
/// <item>Lo que se paga por transferencia no puede ser menos que un mínimo.</item>
/// </list>
/// </summary>
public sealed class ReglasDeCobro
{
    /// <param name="recargos">Los rubros con recargo (cada rubro una sola vez).</param>
    /// <param name="minimoTransferencia">Lo mínimo que se puede pagar por transferencia (0 = sin mínimo).</param>
    public ReglasDeCobro(IEnumerable<RecargoDeRubro> recargos, decimal minimoTransferencia)
    {
        var lista = recargos.ToList();
        if (lista.Any(r => r.PorPaquete < 0) || minimoTransferencia < 0)
            throw new ReglaDeNegocioException("Los montos no pueden ser negativos.");
        if (lista.GroupBy(r => r.CategoriaId).Any(g => g.Count() > 1))
            throw new ReglaDeNegocioException("Ese rubro ya tiene recargo.");

        Recargos = lista.Where(r => r.PorPaquete > 0).ToList();
        MinimoTransferencia = minimoTransferencia;
    }

    public static ReglasDeCobro Ninguna { get; } = new([], 0);

    public IReadOnlyList<RecargoDeRubro> Recargos { get; }
    public decimal MinimoTransferencia { get; }

    /// <summary>Cuánto se suma a cada unidad del producto si se paga por transferencia (0 si no lleva recargo).</summary>
    public decimal RecargoDe(Producto producto) =>
        producto.SeVendeSuelto || producto.PrecioAlVender || producto.CategoriaId is not { } categoria
            ? 0
            : Recargos.FirstOrDefault(r => r.CategoriaId == categoria)?.PorPaquete ?? 0;

    public bool LlevaRecargo(Producto producto) => RecargoDe(producto) > 0;

    public int PaquetesConRecargo(Carrito carrito) =>
        carrito.Items.Where(i => LlevaRecargo(i.Producto)).Sum(i => i.Cantidad);

    /// <summary>Cuánto más se cobra si se paga (todo o una parte) por transferencia.</summary>
    public decimal RecargoPorTransferencia(Carrito carrito) =>
        carrito.Items.Sum(i => i.Cantidad * RecargoDe(i.Producto));

    /// <summary>El total a cobrar, según se pague algo por transferencia o no.</summary>
    public decimal TotalACobrar(Carrito carrito, bool conTransferencia) =>
        carrito.Total + (conTransferencia ? RecargoPorTransferencia(carrito) : 0);

    /// <summary>El precio de cada unidad de un renglón, con el recargo si corresponde.</summary>
    public decimal PrecioUnitario(ItemCarrito item, bool conTransferencia) =>
        item.PrecioUnitario + (conTransferencia ? RecargoDe(item.Producto) : 0);

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

    /// <summary>Las mismas reglas (mismos rubros y montos, sin importar el orden, y mismo mínimo).</summary>
    public bool Iguales(ReglasDeCobro otras) =>
        MinimoTransferencia == otras.MinimoTransferencia
        && Recargos.OrderBy(r => r.CategoriaId).SequenceEqual(otras.Recargos.OrderBy(r => r.CategoriaId));
}

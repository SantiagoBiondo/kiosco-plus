namespace KioscoRockAndRoll.Dominio.Entidades;

/// <summary>
/// Un renglón de una venta ya cobrada. Guarda el precio y el costo al momento de vender,
/// así los reportes de ganancia no cambian cuando después se actualizan los precios.
/// </summary>
public sealed record LineaVenta(long ProductoId, string NombreProducto, int Cantidad, decimal PrecioUnitario, decimal CostoUnitario = 0)
{
    public decimal Subtotal => Cantidad * PrecioUnitario;
    public decimal CostoTotal => Cantidad * CostoUnitario;
}

/// <summary>Una venta ya cobrada. Es inmutable: una vez hecha, no se modifica.</summary>
public sealed class Venta
{
    private Venta(long id, long turnoId, DateTime fecha, IReadOnlyList<Pago> pagos, IReadOnlyList<LineaVenta> lineas)
    {
        Id = id;
        TurnoId = turnoId;
        Fecha = fecha;
        Pagos = pagos;
        Lineas = lineas;
    }

    public long Id { get; private set; }

    /// <summary>El turno de caja en el que se cobró.</summary>
    public long TurnoId { get; }

    public DateTime Fecha { get; }

    /// <summary>Cuánto se pagó con cada medio (suma el total de la venta).</summary>
    public IReadOnlyList<Pago> Pagos { get; }

    /// <summary>El medio con el que se pagó la mayor parte.</summary>
    public MedioDePago MedioPrincipal => Pagos.MaxBy(p => p.Monto)!.Medio;

    public IReadOnlyList<LineaVenta> Lineas { get; }
    public decimal Total => Lineas.Sum(l => l.Subtotal);
    public int Unidades => Lineas.Sum(l => l.Cantidad);

    /// <param name="reglas">
    /// Recargo y mínimo por transferencia. Si se paga algo por transferencia, los paquetes con recargo
    /// quedan registrados con el precio más alto (así sale en el ticket y en los reportes).
    /// </param>
    public static Venta DesdeCarrito(Carrito carrito, Cobro cobro, Turno turno, DateTime fecha, ReglasDeCobro? reglas = null)
    {
        ArgumentNullException.ThrowIfNull(carrito);
        ArgumentNullException.ThrowIfNull(cobro);
        ArgumentNullException.ThrowIfNull(turno);
        reglas ??= ReglasDeCobro.Ninguna;
        if (!turno.EstaAbierto)
            throw new ReglaDeNegocioException("La caja está cerrada. Abrila para poder cobrar.");
        if (carrito.EstaVacio)
            throw new ReglaDeNegocioException("No hay productos para cobrar.");
        reglas.ValidarTransferencia(cobro);
        var conTransferencia = ReglasDeCobro.UsaTransferencia(cobro.Pagos);
        if (cobro.Total != reglas.TotalACobrar(carrito, conTransferencia))
            throw new InvalidOperationException("El cobro no corresponde al total del carrito.");

        var lineas = carrito.Items
            .Select(i => new LineaVenta(i.Producto.Id, i.Producto.Nombre, i.Cantidad,
                reglas.PrecioUnitario(i, conTransferencia), i.CostoUnitario))
            .ToList();
        return new Venta(0, turno.Id, fecha, cobro.Pagos, lineas);
    }

    public void AsignarId(long id)
    {
        if (Id != 0)
            throw new InvalidOperationException("La venta ya tiene un Id asignado.");
        Id = id;
    }
}

/// <summary>Datos resumidos de una venta para listados (no trae el detalle).</summary>
/// <param name="MedioDePago">El medio principal (con el que se pagó la mayor parte).</param>
/// <param name="TurnoId">El turno de caja en que se cobró (null en ventas de antes de que existieran los turnos).</param>
/// <param name="AnuladaEn">Cuándo se anuló, o null si sigue vigente.</param>
/// <param name="Pagos">Cuánto se pagó con cada medio. Si no se indica, se toma todo con el medio principal.</param>
public sealed record ResumenVenta(
    long Id, DateTime Fecha, MedioDePago MedioDePago, int Unidades, decimal Total,
    long? TurnoId = null, DateTime? AnuladaEn = null, string? MotivoAnulacion = null,
    IReadOnlyList<Pago>? Pagos = null)
{
    public const int LargoMaximoMotivo = 100;

    public IReadOnlyList<Pago> PagosRealizados => Pagos is { Count: > 0 } pagos ? pagos : [new Pago(MedioDePago, Total)];

    /// <summary>"Efectivo" o, si se pagó con varios medios, "Débito $ 1.000,00 + Efectivo $ 500,00".</summary>
    public string MedioDePagoDescripcion => Cobro.DescribirPagos(PagosRealizados);
    public bool Anulada => AnuladaEn is not null;
    public string Estado => Anulada ? "Anulada" : "";

    /// <summary>
    /// Una venta se puede anular solo mientras su turno de caja siga abierto: si ya se cerró,
    /// la plata de esa venta forma parte de un arqueo terminado.
    /// </summary>
    /// <returns>El motivo, ya validado.</returns>
    public string VerificarAnulacion(Turno? turnoAbierto, string motivo)
    {
        if (PorQueNoSePuedeAnular(turnoAbierto) is { } impedimento)
            throw new ReglaDeNegocioException(impedimento);

        motivo = motivo?.Trim() ?? "";
        if (motivo == "")
            throw new ReglaDeNegocioException("Escribí el motivo de la anulación (por ejemplo: \"cobré de más\").");
        if (motivo.Length > LargoMaximoMotivo)
            throw new ReglaDeNegocioException($"El motivo no puede tener más de {LargoMaximoMotivo} letras.");
        return motivo;
    }

    /// <summary>Por qué no se puede anular esta venta, o null si se puede.</summary>
    public string? PorQueNoSePuedeAnular(Turno? turnoAbierto)
    {
        if (Anulada)
            return "Esta venta ya está anulada.";
        if (turnoAbierto is null || TurnoId != turnoAbierto.Id)
            return "Solo se pueden anular ventas del turno de caja abierto. Esta venta ya quedó en un cierre de caja.";
        return null;
    }
}

/// <summary>Lo vendido de un producto en un período (sin contar ventas anuladas).</summary>
public sealed record ProductoVendido(string Nombre, int Cantidad, decimal Total);

/// <summary>Una venta con sus renglones, para ver el detalle.</summary>
public sealed record DetalleVenta(ResumenVenta Venta, IReadOnlyList<LineaVenta> Lineas);

using KioscoRockAndRoll.Dominio.Entidades;
using KioscoRockAndRoll.Dominio.Repositorios;

namespace KioscoRockAndRoll.Dominio.Servicios;

/// <summary>Totales de un día, separados por medio de pago. Las ventas anuladas se listan pero no suman.</summary>
public sealed record ResumenDelDia(
    DateOnly Dia,
    IReadOnlyList<ResumenVenta> Ventas,
    IReadOnlyDictionary<MedioDePago, decimal> TotalPorMedio)
{
    public decimal Total => Vigentes.Sum(v => v.Total);
    public int CantidadVigentes => Vigentes.Count();
    public int CantidadAnuladas => Ventas.Count(v => v.Anulada);

    private IEnumerable<ResumenVenta> Vigentes => Ventas.Where(v => !v.Anulada);
}

/// <summary>Casos de uso de la venta: cobrar, anular y consultar lo vendido.</summary>
public class VentaServicio(IVentaRepositorio ventas, CajaServicio caja, TimeProvider reloj)
{
    /// <summary>
    /// Registra la venta en el turno de caja abierto, descuenta el stock y vacía el carrito.
    /// Si la caja está cerrada, no se puede cobrar.
    /// </summary>
    /// <param name="reglas">Recargo y mínimo por transferencia (null = ninguno).</param>
    public Venta Cobrar(Carrito carrito, Cobro cobro, ReglasDeCobro? reglas = null)
    {
        var turno = caja.TurnoAbierto
                    ?? throw new ReglaDeNegocioException("La caja está cerrada. Abrila para poder cobrar.");
        var venta = Venta.DesdeCarrito(carrito, cobro, turno, reloj.GetLocalNow().DateTime, reglas);
        ventas.Registrar(venta);

        // El repositorio ya descontó el stock en la base; reflejamos lo mismo en memoria.
        foreach (var item in carrito.Items.Where(i => i.Producto.ControlaStock))
            item.Producto.DescontarStock(item.Cantidad);

        carrito.Vaciar();
        return venta;
    }

    public DetalleVenta? Detalle(long ventaId) => ventas.ObtenerDetalle(ventaId);

    /// <summary>Por qué no se puede anular la venta, o null si se puede.</summary>
    public string? PorQueNoSePuedeAnular(ResumenVenta venta) => venta.PorQueNoSePuedeAnular(caja.TurnoAbierto);

    /// <summary>
    /// Anula una venta del turno abierto: deja de contar en la caja y en los totales,
    /// y el stock de sus productos vuelve. La venta no se borra: queda marcada con el motivo.
    /// </summary>
    public void Anular(long ventaId, string motivo)
    {
        var detalle = ventas.ObtenerDetalle(ventaId)
                      ?? throw new ReglaDeNegocioException("La venta no existe.");
        var motivoValidado = detalle.Venta.VerificarAnulacion(caja.TurnoAbierto, motivo);
        ventas.Anular(ventaId, motivoValidado, reloj.GetLocalNow().DateTime);
    }

    /// <summary>Qué productos se vendieron en el día, de mayor a menor recaudación (sin anuladas).</summary>
    public IReadOnlyList<ProductoVendido> ProductosVendidos(DateOnly dia) => ventas.ProductosVendidosDelDia(dia);

    public ResumenDelDia ResumenDelDia(DateOnly dia)
    {
        var delDia = ventas.ListarDelDia(dia);
        // Una venta pagada con varios medios suma cada parte en su medio.
        var porMedio = delDia
            .Where(v => !v.Anulada)
            .SelectMany(v => v.PagosRealizados)
            .GroupBy(p => p.Medio)
            .ToDictionary(g => g.Key, g => g.Sum(p => p.Monto));
        return new ResumenDelDia(dia, delDia, porMedio);
    }
}

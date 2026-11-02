using KioscoRockAndRoll.Dominio.Entidades;
using KioscoRockAndRoll.Dominio.Repositorios;

namespace KioscoRockAndRoll.Dominio.Servicios;

/// <summary>Reportes de ventas y ganancia por período.</summary>
public class ReporteDeVentasServicio(IVentaRepositorio ventas, IProductoRepositorio productos)
{
    public ReporteDeVentas Generar(PeriodoReporte periodo, Agrupacion agrupacion) =>
        ReporteDeVentas.Armar(periodo, agrupacion, ventas.TotalesPorDia(periodo), ventas.RendimientoPorProducto(periodo));

    /// <summary>
    /// Lo vendido en el período de un rubro y/o proveedor (por ejemplo, "Cigarrillos" o "Arcor"),
    /// producto por producto, de mayor a menor recaudación. No cuenta las ventas anuladas.
    /// Se usa la clasificación actual de cada producto; los que ya no existen solo entran sin filtros.
    /// </summary>
    public IReadOnlyList<RendimientoProducto> VendidoPor(PeriodoReporte periodo,
        FiltroClasificacion categoria, FiltroClasificacion proveedor)
    {
        var clasificacion = productos.Buscar("").ToDictionary(p => p.Id);
        var sinFiltro = categoria == FiltroClasificacion.Todos && proveedor == FiltroClasificacion.Todos;

        return ventas.RendimientoPorProducto(periodo)
            .Where(r => clasificacion.TryGetValue(r.ProductoId, out var producto)
                ? categoria.Acepta(producto.CategoriaId) && proveedor.Acepta(producto.ProveedorId)
                : sinFiltro)
            .OrderByDescending(r => r.Vendido)
            .ToList();
    }
}

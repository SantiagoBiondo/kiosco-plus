namespace KioscoRockAndRoll.Dominio.Entidades;

/// <summary>
/// Un producto del kiosco. Sus datos solo se pueden cambiar a través de
/// métodos que validan las reglas del negocio.
/// </summary>
public class Producto
{
    public long Id { get; private set; }
    public string CodigoBarras { get; private set; } = "";
    public string Nombre { get; private set; } = "";
    public decimal PrecioCosto { get; private set; }
    public decimal PrecioVenta { get; private set; }

    /// <summary>
    /// Puede quedar negativo: en un kiosco a veces se vende algo antes de registrar su ingreso,
    /// y es preferible no frenar la venta. El inventario lo muestra como stock bajo.
    /// </summary>
    public int Stock { get; private set; }

    public int StockMinimo { get; private set; }

    /// <summary>Categoría del producto, o null si no tiene.</summary>
    public long? CategoriaId { get; private set; }

    /// <summary>Proveedor habitual del producto, o null si no tiene.</summary>
    public long? ProveedorId { get; private set; }

    /// <summary>
    /// Si se vende suelto: el producto (paquete, caja) del que salen las unidades, o null si no.
    /// Ejemplo: "Cigarrillo suelto" sale de "Marlboro x20".
    /// </summary>
    public long? PaqueteId { get; private set; }

    /// <summary>Cuántas unidades trae el paquete (0 si no se vende suelto).</summary>
    public int UnidadesPorPaquete { get; private set; }

    /// <summary>Paquetes cerrados que quedan, leídos junto con el producto. Solo informativo.</summary>
    public int PaquetesCerrados { get; private set; }

    public bool SeVendeSuelto => PaqueteId != null;

    /// <summary>
    /// Si el precio se pone al momento de vender (por ejemplo, caramelos que se venden por peso).
    /// En ese caso el precio de costo y de venta son de referencia y el stock no se controla.
    /// </summary>
    public bool PrecioAlVender { get; private set; }

    public Producto(string codigoBarras, string nombre, decimal precioCosto, decimal precioVenta,
        int stock = 0, int stockMinimo = 0)
    {
        ActualizarDatos(codigoBarras, nombre, precioCosto, precioVenta, stockMinimo);
        if (stock < 0)
            throw new ReglaDeNegocioException("El stock inicial no puede ser negativo.");
        Stock = stock;
    }

    /// <summary>Reconstruye un producto ya guardado. Lo usa la capa de datos.</summary>
    public static Producto Restaurar(long id, string codigoBarras, string nombre, decimal precioCosto,
        decimal precioVenta, int stock, int stockMinimo, long? categoriaId = null, long? proveedorId = null,
        long? paqueteId = null, int unidadesPorPaquete = 0, int paquetesCerrados = 0, bool precioAlVender = false)
    {
        var producto = new Producto(codigoBarras, nombre, precioCosto, precioVenta, 0, stockMinimo) { Id = id };
        producto.Stock = stock;
        producto.PrecioAlVender = precioAlVender;
        producto.Clasificar(categoriaId, proveedorId);
        if (paqueteId != null)
        {
            producto.PaqueteId = paqueteId;
            producto.UnidadesPorPaquete = unidadesPorPaquete;
            producto.PaquetesCerrados = paquetesCerrados;
        }
        return producto;
    }

    public bool EsNuevo => Id == 0;
    public bool VendeAPerdida => PrecioVenta < PrecioCosto;

    /// <summary>
    /// Lo que se puede vender: el stock, más las unidades de los paquetes cerrados si se vende suelto.
    /// </summary>
    public int StockDisponible => Stock + (SeVendeSuelto ? Math.Max(PaquetesCerrados, 0) * UnidadesPorPaquete : 0);

    /// <summary>Si se lleva la cuenta del stock (no en los productos con precio al vender).</summary>
    public bool ControlaStock => !PrecioAlVender;

    public bool StockBajo => ControlaStock && StockDisponible <= StockMinimo;

    public bool TieneStockPara(int cantidad) => !ControlaStock || StockDisponible >= cantidad;

    /// <summary>Indica si el precio se pone al vender (true) o es el precio de venta cargado (false).</summary>
    public void DefinirPrecioAlVender(bool alVender)
    {
        if (alVender && SeVendeSuelto)
            throw new ReglaDeNegocioException("Un producto que sale de un paquete no puede tener el precio al vender.");
        PrecioAlVender = alVender;
    }

    /// <summary>
    /// El costo estimado de una venta a un precio puesto en el momento: se mantiene la misma proporción
    /// entre costo y venta que en los precios de referencia (si no hay precio de referencia, es 0).
    /// </summary>
    public decimal CostoEstimadoPara(decimal precio) =>
        PrecioVenta > 0 ? Math.Round(precio * PrecioCosto / PrecioVenta, 2) : 0;

    public void AsignarId(long id)
    {
        if (!EsNuevo)
            throw new InvalidOperationException("El producto ya tiene un Id asignado.");
        if (id <= 0)
            throw new ArgumentOutOfRangeException(nameof(id));
        Id = id;
    }

    public void ActualizarDatos(string codigoBarras, string nombre, decimal precioCosto, decimal precioVenta, int stockMinimo)
    {
        codigoBarras = codigoBarras?.Trim() ?? "";
        nombre = nombre?.Trim() ?? "";

        if (codigoBarras == "")
            throw new ReglaDeNegocioException("El código de barras es obligatorio.");
        if (nombre == "")
            throw new ReglaDeNegocioException("El nombre es obligatorio.");
        if (precioCosto < 0 || precioVenta < 0)
            throw new ReglaDeNegocioException("Los precios no pueden ser negativos.");
        if (stockMinimo < 0)
            throw new ReglaDeNegocioException("El stock mínimo no puede ser negativo.");

        CodigoBarras = codigoBarras;
        Nombre = nombre;
        PrecioCosto = precioCosto;
        PrecioVenta = precioVenta;
        StockMinimo = stockMinimo;
    }

    /// <summary>Asigna categoría y proveedor (null = ninguno).</summary>
    public void Clasificar(long? categoriaId, long? proveedorId)
    {
        if (categoriaId <= 0 || proveedorId <= 0)
            throw new ArgumentOutOfRangeException(categoriaId <= 0 ? nameof(categoriaId) : nameof(proveedorId));
        CategoriaId = categoriaId;
        ProveedorId = proveedorId;
    }

    /// <summary>
    /// Indica de qué paquete salen las unidades sueltas de este producto (null = no se vende suelto).
    /// </summary>
    public void VenderSueltoDe(Producto? paquete, int unidadesPorPaquete)
    {
        if (paquete == null)
        {
            PaqueteId = null;
            UnidadesPorPaquete = 0;
            PaquetesCerrados = 0;
            return;
        }

        if (paquete.EsNuevo)
            throw new InvalidOperationException("El paquete tiene que estar guardado.");
        if (paquete.Id == Id || paquete.CodigoBarras == CodigoBarras)
            throw new ReglaDeNegocioException("Un producto no puede salir de sí mismo: el paquete tiene que ser otro producto.");
        if (PrecioAlVender)
            throw new ReglaDeNegocioException("Un producto con el precio al vender no puede salir de un paquete.");
        if (paquete.SeVendeSuelto)
            throw new ReglaDeNegocioException($"\"{paquete.Nombre}\" ya se vende suelto de otro paquete: elegí el paquete cerrado.");
        if (unidadesPorPaquete < 2)
            throw new ReglaDeNegocioException("Indicá cuántas unidades trae el paquete (2 o más).");

        PaqueteId = paquete.Id;
        UnidadesPorPaquete = unidadesPorPaquete;
        PaquetesCerrados = paquete.Stock;
    }

    /// <summary>
    /// Cuántos paquetes hay que abrir para vender <paramref name="cantidad"/> unidades sueltas:
    /// los justos para cubrir lo que falta, sin pasarse de los paquetes cerrados que quedan.
    /// Si no alcanza, el resto queda como stock negativo (igual que cualquier otro producto).
    /// </summary>
    public static int PaquetesQueHayQueAbrir(int sueltas, int cantidad, int unidadesPorPaquete, int paquetesCerrados)
    {
        var faltan = cantidad - sueltas;
        if (faltan <= 0 || unidadesPorPaquete <= 0 || paquetesCerrados <= 0)
            return 0;
        var necesarios = (faltan + unidadesPorPaquete - 1) / unidadesPorPaquete;
        return Math.Min(necesarios, paquetesCerrados);
    }

    public void AjustarPrecios(AjustePrecios ajuste)
    {
        ArgumentNullException.ThrowIfNull(ajuste);
        PrecioCosto = ajuste.NuevoCosto(PrecioCosto);
        PrecioVenta = ajuste.NuevaVenta(PrecioVenta);
    }

    public void IngresarStock(int cantidad)
    {
        if (cantidad <= 0)
            throw new ReglaDeNegocioException("La cantidad a ingresar tiene que ser mayor a cero.");
        Stock += cantidad;
    }

    public void DescontarStock(int cantidad)
    {
        if (cantidad <= 0)
            throw new ReglaDeNegocioException("La cantidad a descontar tiene que ser mayor a cero.");
        Stock -= cantidad;
    }

    /// <summary>Corrige el stock a mano (por ejemplo, después de contar la mercadería).</summary>
    public void AjustarStock(int stockReal)
    {
        if (stockReal < 0)
            throw new ReglaDeNegocioException("El stock no puede ser negativo.");
        Stock = stockReal;
    }
}

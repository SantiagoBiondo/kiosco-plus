namespace KioscoRockAndRoll.Dominio.Entidades;

/// <summary>Un renglón del carrito: un producto y cuántas unidades se llevan.</summary>
public class ItemCarrito
{
    internal ItemCarrito(Producto producto, int cantidad, decimal? precioElegido)
    {
        Producto = producto;
        Cantidad = cantidad;
        PrecioElegido = precioElegido;
    }

    public Producto Producto { get; internal set; }
    public int Cantidad { get; internal set; }

    /// <summary>El precio puesto al vender (productos con precio al vender), o null si se usa el de venta.</summary>
    public decimal? PrecioElegido { get; }

    public decimal PrecioUnitario => PrecioElegido ?? Producto.PrecioVenta;
    public decimal Subtotal => Cantidad * PrecioUnitario;

    /// <summary>El costo de cada unidad: el del producto, o estimado si el precio se puso al vender.</summary>
    public decimal CostoUnitario => PrecioElegido is { } precio ? Producto.CostoEstimadoPara(precio) : Producto.PrecioCosto;

    /// <summary>True si se están llevando más unidades de las que figuran en stock.</summary>
    public bool SuperaElStock => !Producto.TieneStockPara(Cantidad);
}

/// <summary>
/// La venta que se está armando en el mostrador, antes de cobrarla.
/// </summary>
public class Carrito
{
    private readonly List<ItemCarrito> _items = [];

    public IReadOnlyList<ItemCarrito> Items => _items;
    public decimal Total => _items.Sum(i => i.Subtotal);
    public int Unidades => _items.Sum(i => i.Cantidad);
    public bool EstaVacio => _items.Count == 0;

    /// <summary>Se dispara cada vez que cambia el contenido del carrito.</summary>
    public event EventHandler? Cambio;

    /// <summary>
    /// Agrega unidades de un producto. Si el producto ya estaba, suma en el mismo renglón.
    /// Los productos con precio al vender necesitan el <paramref name="precio"/> y van siempre en un renglón aparte
    /// (por ejemplo, dos bolsitas de caramelos de distinto peso).
    /// </summary>
    public ItemCarrito Agregar(Producto producto, int cantidad = 1, decimal? precio = null)
    {
        ArgumentNullException.ThrowIfNull(producto);
        if (producto.EsNuevo)
            throw new InvalidOperationException("Solo se pueden vender productos guardados.");
        if (cantidad <= 0)
            throw new ReglaDeNegocioException("La cantidad tiene que ser mayor a cero.");

        ItemCarrito? item;
        if (producto.PrecioAlVender)
        {
            if (precio is not > 0)
                throw new ReglaDeNegocioException($"Poné el precio de \"{producto.Nombre}\" (tiene que ser mayor a cero).");
            item = new ItemCarrito(producto, cantidad, precio);
            _items.Add(item);
        }
        else
        {
            if (precio != null)
                throw new ArgumentException("Este producto tiene precio fijo.", nameof(precio));
            item = Buscar(producto.Id);
            if (item == null)
            {
                item = new ItemCarrito(producto, cantidad, null);
                _items.Add(item);
            }
            else
            {
                item.Cantidad += cantidad;
            }
        }

        AvisarCambio();
        return item;
    }

    /// <summary>Cambia la cantidad de un renglón. Con cantidad 0 o menos, lo quita.</summary>
    public void CambiarCantidad(ItemCarrito item, int cantidad)
    {
        if (!_items.Contains(item))
            throw new InvalidOperationException("El renglón no está en el carrito.");
        if (cantidad <= 0)
            _items.Remove(item);
        else
            item.Cantidad = cantidad;
        AvisarCambio();
    }

    /// <summary>Cambia la cantidad del renglón de un producto. Con cantidad 0 o menos, lo quita.</summary>
    public void CambiarCantidad(long productoId, int cantidad) =>
        CambiarCantidad(Buscar(productoId) ?? throw new InvalidOperationException("El producto no está en el carrito."), cantidad);

    public void Quitar(ItemCarrito item)
    {
        if (_items.Remove(item))
            AvisarCambio();
    }

    public void Quitar(long productoId)
    {
        if (Buscar(productoId) is { } item)
            Quitar(item);
    }

    public void Vaciar()
    {
        if (EstaVacio)
            return;
        _items.Clear();
        AvisarCambio();
    }

    /// <summary>
    /// Reemplaza los datos de los productos (precio, stock) por versiones más nuevas.
    /// Los productos que ya no existen se quitan. Los precios puestos al vender se conservan.
    /// </summary>
    public void Refrescar(Func<long, Producto?> obtenerProducto)
    {
        for (var i = _items.Count - 1; i >= 0; i--)
        {
            var actualizado = obtenerProducto(_items[i].Producto.Id);
            if (actualizado == null)
                _items.RemoveAt(i);
            else
                _items[i].Producto = actualizado;
        }
        AvisarCambio();
    }

    /// <summary>El primer renglón del producto (los de precio al vender pueden tener varios).</summary>
    public ItemCarrito? Buscar(long productoId) => _items.FirstOrDefault(i => i.Producto.Id == productoId);

    private void AvisarCambio() => Cambio?.Invoke(this, EventArgs.Empty);
}

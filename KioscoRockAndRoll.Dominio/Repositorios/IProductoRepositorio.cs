using KioscoRockAndRoll.Dominio.Entidades;

namespace KioscoRockAndRoll.Dominio.Repositorios;

/// <summary>
/// Dónde se guardan los productos. El dominio no sabe si es SQLite, un archivo u otra cosa.
/// </summary>
public interface IProductoRepositorio
{
    Producto? ObtenerPorId(long id);
    Producto? ObtenerPorCodigo(string codigoBarras);

    /// <summary>Busca por parte del nombre o del código. Con texto vacío devuelve todos.</summary>
    IReadOnlyList<Producto> Buscar(string texto);

    void Agregar(Producto producto);
    void Actualizar(Producto producto);

    /// <summary>Guarda varios productos en una sola operación atómica.</summary>
    void ActualizarVarios(IReadOnlyCollection<Producto> productos);

    /// <summary>Agrega los nuevos y guarda los modificados en una sola operación atómica (todo o nada).</summary>
    void GuardarImportacion(IReadOnlyCollection<Producto> nuevos, IReadOnlyCollection<Producto> modificados);

    void Eliminar(long id);
    bool TieneVentas(long id);
}

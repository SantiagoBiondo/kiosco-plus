using KioscoRockAndRoll.Dominio.Entidades;
using KioscoRockAndRoll.Dominio.Repositorios;
using Microsoft.Extensions.DependencyInjection;

namespace KioscoRockAndRoll.Datos;

public static class RegistroDeServicios
{
    /// <summary>Registra la implementación SQLite de los repositorios.</summary>
    public static IServiceCollection AddDatosSqlite(this IServiceCollection servicios, string rutaArchivo)
    {
        servicios.AddSingleton(new ConexionSqlite(rutaArchivo));
        servicios.AddSingleton<InicializadorBaseDeDatos>();
        servicios.AddSingleton<Ajustes>();
        servicios.AddSingleton<AjustesDeTicket>();
        servicios.AddSingleton<AjustesDeApariencia>();
        servicios.AddSingleton<AjustesDeEtiquetas>();
        servicios.AddSingleton<AjustesDeCobro>();
        servicios.AddSingleton<GestorDeBackups>();
        servicios.AddSingleton<IProductoRepositorio, SqliteProductoRepositorio>();
        servicios.AddSingleton<IVentaRepositorio, SqliteVentaRepositorio>();
        servicios.AddSingleton<ICajaRepositorio, SqliteCajaRepositorio>();
        servicios.AddSingleton<ICatalogoRepositorio<Categoria>, SqliteCategoriaRepositorio>();
        servicios.AddSingleton<ICatalogoRepositorio<Proveedor>, SqliteProveedorRepositorio>();
        return servicios;
    }
}

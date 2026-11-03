using KioscoRockAndRoll.Datos;
using KioscoRockAndRoll.Dominio;
using KioscoRockAndRoll.Dominio.Entidades;
using KioscoRockAndRoll.Dominio.Servicios;
using KioscoRockAndRoll.Reportes;
using Microsoft.Extensions.DependencyInjection;
using Proyecto_Kiosco_Rock_Roll.Interfaz;
using Proyecto_Kiosco_Rock_Roll.Interfaz.Controles;
using Proyecto_Kiosco_Rock_Roll.Interfaz.Impresion;
using Proyecto_Kiosco_Rock_Roll.Interfaz.Vistas;

namespace Proyecto_Kiosco_Rock_Roll
{
    internal static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            // To customize application configuration such as set high DPI settings or default font,
            // see https://aka.ms/applicationconfiguration.
            ApplicationConfiguration.Initialize();
            ConfigurarManejoDeErrores();

            using var proveedor = ConfigurarServicios(RutaBase(args)).BuildServiceProvider(validateScopes: true);
            proveedor.GetRequiredService<InicializadorBaseDeDatos>().Inicializar();

            // El tema se elige antes de crear la primera ventana.
            Estilos.AplicarTema(proveedor.GetRequiredService<AjustesDeApariencia>().TemaOscuro);

            Application.Run(proveedor.GetRequiredService<FormPrincipal>());
        }

        /// <summary>
        /// Raíz de composición: el único lugar donde se decide qué implementación usa cada interfaz.
        /// </summary>
        private static ServiceCollection ConfigurarServicios(string rutaBase)
        {
            var servicios = new ServiceCollection();
            servicios.AddDatosSqlite(rutaBase);
            servicios.AddSingleton(TimeProvider.System);
            servicios.AddSingleton<InventarioServicio>();
            servicios.AddSingleton<VentaServicio>();
            servicios.AddSingleton<CajaServicio>();
            servicios.AddSingleton<ReporteDelDiaServicio>();
            servicios.AddSingleton<ReporteDeVentasServicio>();
            servicios.AddSingleton<ImportadorDeProductos>();
            servicios.AddSingleton<ExportadorDeReportes>();
            servicios.AddSingleton<ImpresoraDeTickets>();
            servicios.AddSingleton<CatalogoServicio<Categoria>>();
            servicios.AddSingleton<CatalogoServicio<Proveedor>>();

            // Controles que usan varias vistas: cada vista recibe su propia instancia.
            servicios.AddTransient<EditorProducto>();
            servicios.AddTransient<BuscadorProducto>();
            servicios.AddTransient<PanelAjustePrecios>();
            servicios.AddTransient<PanelCategorias>();
            servicios.AddTransient<PanelProveedores>();
            servicios.AddTransient<PanelPedido>();
            servicios.AddTransient<PanelImportacion>();
            servicios.AddTransient<PanelAbrirCaja>();
            servicios.AddTransient<PanelMovimientoCaja>();
            servicios.AddTransient<PanelCerrarCaja>();
            servicios.AddTransient<ListadoVentasPorDia>();
            servicios.AddTransient<PanelDetalleVenta>();

            // Una sola ventana con una instancia de cada vista durante toda la ejecución.
            servicios.AddSingleton<FormPrincipal>();
            servicios.AddSingleton<VistaVenta>();
            servicios.AddSingleton<VistaInventario>();
            servicios.AddSingleton<VistaIngresoMercaderia>();
            servicios.AddSingleton<VistaEtiquetas>();
            servicios.AddSingleton<VistaCaja>();
            servicios.AddSingleton<VistaReportes>();
            servicios.AddSingleton<VistaConfiguracion>();
            servicios.AddSingleton<VistaBackups>();
            return servicios;
        }

        /// <summary>
        /// Por defecto la base se guarda en %LOCALAPPDATA%\KioscoRockAndRoll\kiosco.db
        /// (para hacer un backup alcanza con copiar ese archivo).
        /// Con "--base otra.db" se puede usar otra, por ejemplo para practicar sin tocar la real.
        /// </summary>
        private static string RutaBase(string[] args)
        {
            var indice = Array.IndexOf(args, "--base");
            if (indice >= 0 && indice + 1 < args.Length)
                return Path.GetFullPath(args[indice + 1]);

            // La carpeta conserva el nombre de antes de que el programa se llamara Kiosco+,
            // así la base del kiosco se sigue encontrando después de actualizar.
            return Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "KioscoRockAndRoll", "kiosco.db");
        }

        // Red de seguridad: si algo falla y nadie lo atrapó, se muestra un mensaje en vez de cerrar el programa.
        private static void ConfigurarManejoDeErrores()
        {
            Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
            Application.ThreadException += (_, e) =>
            {
                if (e.Exception is ReglaDeNegocioException regla)
                    Mensajes.Advertencia(null, regla.Message);
                else
                    Mensajes.Error(null, $"Ocurrió un error inesperado:\n{e.Exception.Message}");
            };
        }
    }
}

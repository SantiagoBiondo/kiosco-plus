using System.Diagnostics;
using KioscoRockAndRoll.Datos;

namespace Proyecto_Kiosco_Rock_Roll.Interfaz.Vistas
{
    /// <summary>
    /// Copias de seguridad: dónde se guardan, hacer una ahora y restaurar una anterior.
    /// El backup automático se hace al cerrar el programa (ver <see cref="FormPrincipal"/>).
    /// Los controles están en VistaBackups.Designer.cs (se editan con el diseñador de Visual Studio).
    /// </summary>
    public partial class VistaBackups : Vista
    {
        private readonly GestorDeBackups _backups;

        public VistaBackups(GestorDeBackups backups)
        {
            _backups = backups;
            InitializeComponent();

            // Lo que el diseñador no puede hacer: textos que dependen de datos, colores del tema y la grilla.
            lblExplicacion.Text =
                "Cada vez que cerrás el programa se guarda una copia de seguridad del día. " +
                $"Se conservan las últimas {GestorDeBackups.CantidadAConservar}.\n" +
                "Para no depender de esta computadora, elegí una carpeta extra: un pendrive o una carpeta de Google Drive / OneDrive.";
            Estilos.ConfigurarGrilla(grilla);
            Estilos.Destacar(btnHacerBackup, Estilos.FondoBotonPrincipal);
        }

        // ---------- Lo que pasa al tocar cada cosa (conectado en el diseñador) ----------

        private void BtnAbrirCarpeta_Click(object? sender, EventArgs e) => AbrirEnExplorador(_backups.CarpetaPrincipal);

        private void BtnElegirCarpeta_Click(object? sender, EventArgs e) => ElegirCarpetaExtra();

        private void BtnQuitarExtra_Click(object? sender, EventArgs e) => QuitarCarpetaExtra();

        private void BtnHacerBackup_Click(object? sender, EventArgs e) => HacerBackup();

        private void BtnRestaurar_Click(object? sender, EventArgs e) => Restaurar();

        public override string Titulo => "Backups";

        public override void AlMostrar()
        {
            Cargar();
            grilla.Focus();
        }

        private void Cargar()
        {
            lblCarpetaPrincipal.Text = _backups.CarpetaPrincipal;
            var extra = _backups.CarpetaExtra;
            lblCarpetaExtra.Text = extra ?? "(ninguna)";
            lblCarpetaExtra.ForeColor = extra is null ? SystemColors.GrayText : SystemColors.ControlText;
            btnQuitarExtra.Enabled = extra is not null;
            grilla.DataSource = _backups.Listar().ToList();
        }

        private void HacerBackup()
        {
            var resultado = _backups.HacerBackup();
            Cargar();
            if (resultado.TodoBien)
                MostrarEstado($"Backup hecho en {resultado.Destinos.Count} carpeta(s).", Estilos.Exito);
            else
                MostrarEstado(DescribirErrores(resultado), Estilos.Error);
        }

        private void ElegirCarpetaExtra()
        {
            using var dialogo = new FolderBrowserDialog
            {
                Description = "Elegí dónde guardar una segunda copia de los backups (pendrive, Google Drive, etc.)",
                UseDescriptionForTitle = true,
                SelectedPath = _backups.CarpetaExtra ?? ""
            };
            if (dialogo.ShowDialog(this) != DialogResult.OK)
                return;

            _backups.CarpetaExtra = dialogo.SelectedPath;
            Cargar();
            // Se hace un backup enseguida para comprobar que la carpeta sirve.
            HacerBackup();
        }

        private void QuitarCarpetaExtra()
        {
            _backups.CarpetaExtra = null;
            Cargar();
            MostrarEstado("Ya no se copian backups a la carpeta extra (los que ya están ahí no se borran).", SystemColors.ControlText);
        }

        private void Restaurar()
        {
            if (grilla.CurrentRow?.DataBoundItem is not InfoBackup backup)
            {
                MostrarEstado("Elegí de la lista el backup a restaurar.", Estilos.Advertencia);
                return;
            }

            var pregunta =
                $"¿Restaurar \"{backup.Nombre}\" (del {backup.Fecha:dd/MM/yyyy HH:mm})?\n\n" +
                "Se van a perder los cambios hechos después de esa fecha (ventas, productos, precios).\n" +
                "Por las dudas, antes se guarda una copia del estado actual.\n\n" +
                "Después el programa se reinicia.";
            if (!Mensajes.Confirmar(this, pregunta))
                return;

            try
            {
                _backups.Restaurar(backup.Ruta);
            }
            catch (Exception ex) when (ex is IOException or InvalidDataException or Microsoft.Data.Sqlite.SqliteException)
            {
                MostrarEstado($"No se pudo restaurar: {ex.Message}", Estilos.Error);
                return;
            }

            Mensajes.Informacion(this, "Backup restaurado. El programa se va a reiniciar.");
            Application.Restart();
        }

        internal static string DescribirErrores(ResultadoBackup resultado) =>
            string.Join("\n", resultado.Destinos
                .Where(d => !d.Exito)
                .Select(d => $"No se pudo hacer el backup en \"{d.Carpeta}\": {d.Error}"));

        private static void AbrirEnExplorador(string carpeta)
        {
            Directory.CreateDirectory(carpeta);
            Process.Start(new ProcessStartInfo { FileName = carpeta, UseShellExecute = true });
        }

        private void MostrarEstado(string texto, Color color)
        {
            lblEstado.Text = texto;
            lblEstado.ForeColor = color;
        }
    }
}

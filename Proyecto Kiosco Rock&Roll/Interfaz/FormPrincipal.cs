using KioscoRockAndRoll.Datos;
using Proyecto_Kiosco_Rock_Roll.Interfaz.Vistas;

namespace Proyecto_Kiosco_Rock_Roll.Interfaz
{
    /// <summary>
    /// La única ventana del programa: una barra de secciones arriba y la vista activa abajo.
    /// Al cerrarse hace el backup automático del día.
    /// </summary>
    public class FormPrincipal : Form
    {
        private readonly GestorDeBackups _backups;
        private readonly Panel contenedor = new() { Dock = DockStyle.Fill };
        private readonly Dictionary<Vista, Button> _botones = [];
        private Vista? _actual;

        public FormPrincipal(GestorDeBackups backups, VistaVenta venta, VistaInventario inventario,
            VistaIngresoMercaderia ingreso, VistaEtiquetas etiquetas, VistaCaja caja, VistaReportes reportes, VistaConfiguracion configuracion, VistaBackups vistaBackups)
        {
            _backups = backups;

            Text = "Kiosco+";
            // El mismo símbolo del .exe, para la barra de título y la barra de tareas.
            using (var icono = typeof(FormPrincipal).Assembly.GetManifestResourceStream("KioscoPlus.ico"))
                if (icono != null)
                    Icon = new Icon(icono);
            Font = Estilos.Normal;
            Size = new Size(1200, 750);
            MinimumSize = new Size(1000, 600);
            StartPosition = FormStartPosition.CenterScreen;
            WindowState = FormWindowState.Maximized;

            var barra = new FlowLayoutPanel { Dock = DockStyle.Top, AutoSize = true, BackColor = Estilos.FondoBarra, Padding = new Padding(6, 6, 6, 0) };
            foreach (var vista in new Vista[] { venta, inventario, ingreso, etiquetas, caja, reportes, configuracion, vistaBackups })
            {
                var boton = new Button
                {
                    Text = vista.Atajo == Keys.None ? vista.Titulo : $"{vista.Titulo}  ({vista.Atajo})",
                    AutoSize = true,
                    Height = 46,
                    Padding = new Padding(14, 0, 14, 0),
                    Margin = new Padding(0, 0, 4, 0),
                    FlatStyle = FlatStyle.Flat,
                    ForeColor = Color.White,
                    Font = Estilos.Grande,
                    TabStop = false
                };
                boton.FlatAppearance.BorderSize = 0;
                boton.Click += (_, _) => Navegar(vista);
                barra.Controls.Add(boton);
                _botones.Add(vista, boton);
            }

            Controls.Add(contenedor);
            Controls.Add(barra);

            Load += (_, _) => Navegar(venta);
            Shown += (_, _) => _actual?.AlMostrar(); // recién acá se puede poner el foco
            FormClosing += (_, _) => HacerBackupAlCerrar();
        }

        private void HacerBackupAlCerrar()
        {
            Cursor = Cursors.WaitCursor;
            var resultado = _backups.HacerBackup();
            Cursor = Cursors.Default;

            if (!resultado.TodoBien)
                Mensajes.Advertencia(this, VistaBackups.DescribirErrores(resultado) +
                                           "\n\nPodés revisarlo en la sección Backups la próxima vez que abras el programa.");
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            // Primero dejamos que la vista activa use la tecla; si no la usa, vemos si es un atajo de sección.
            if (base.ProcessCmdKey(ref msg, keyData))
                return true;

            var destino = _botones.Keys.FirstOrDefault(v => v.Atajo != Keys.None && v.Atajo == keyData);
            if (destino == null)
                return false;
            Navegar(destino);
            return true;
        }

        private void Navegar(Vista vista)
        {
            if (vista != _actual)
            {
                SuspendLayout();
                contenedor.Controls.Clear(); // la vista anterior no se destruye: conserva su estado
                contenedor.Controls.Add(vista);
                ResumeLayout();
                _actual = vista;

                foreach (var (v, boton) in _botones)
                    boton.BackColor = v == vista ? Estilos.SeccionActiva : Estilos.FondoBarra;
            }
            vista.AlMostrar();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                // Las vistas que no están en pantalla no son hijas del formulario: se liberan a mano.
                foreach (var vista in _botones.Keys)
                    vista.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}

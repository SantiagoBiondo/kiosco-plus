namespace Proyecto_Kiosco_Rock_Roll.Interfaz.Controles
{
    /// <summary>
    /// Panel que aparece a la derecha de una vista para mostrar un formulario
    /// (alta de producto, búsqueda, cobro) sin abrir otra ventana.
    /// </summary>
    public class PanelLateral : Panel
    {
        private readonly int _anchoNormal;

        public PanelLateral(int ancho = 460)
        {
            _anchoNormal = ancho;
            Dock = DockStyle.Right;
            Width = ancho;
            Padding = new Padding(16);
            BackColor = Estilos.FondoPanelLateral;
            Visible = false;
        }

        public bool Abierto => Visible;
        public Control? Contenido => Controls.Count > 0 ? Controls[0] : null;

        /// <param name="ancho">Para un contenido que necesita más lugar (por ejemplo, una vista previa); si no, el ancho normal.</param>
        public void Mostrar(Control contenido, int? ancho = null)
        {
            SuspendLayout();
            Controls.Clear(); // no destruye el control anterior: se reutiliza
            Width = ancho ?? _anchoNormal;
            contenido.Dock = DockStyle.Fill;
            Controls.Add(contenido);
            Visible = true;
            ResumeLayout();
        }

        public void Ocultar()
        {
            Visible = false;
            Controls.Clear();
        }
    }
}

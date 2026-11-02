using KioscoRockAndRoll.Dominio.Entidades;

namespace Proyecto_Kiosco_Rock_Roll.Interfaz.Controles
{
    /// <summary>
    /// Pide el precio de un producto que se cobra según lo que se lleva (por ejemplo, caramelos por peso).
    /// Enter agrega el producto al carrito con ese precio, Esc cancela.
    /// </summary>
    public class PanelPrecioAlVender : UserControl
    {
        private Producto? _producto;
        private int _cantidad = 1;

        private readonly Label lblNombre = new() { AutoSize = true, Font = Estilos.Titulo, MaximumSize = new Size(400, 0), Margin = new Padding(3, 8, 3, 8) };
        private readonly NumericUpDown numPrecio = Estilos.CampoNumero(2);
        private readonly Label lblReferencia = new() { AutoSize = true, ForeColor = SystemColors.GrayText, MaximumSize = new Size(400, 0), Margin = new Padding(3, 8, 3, 3) };
        private readonly Label lblError = new() { AutoSize = true, ForeColor = Estilos.Error, MaximumSize = new Size(400, 0) };

        /// <summary>Se confirmó el precio: el producto, la cantidad y el precio de cada uno.</summary>
        public event EventHandler<(Producto Producto, int Cantidad, decimal Precio)>? Confirmado;

        public event EventHandler? Cancelado;

        public PanelPrecioAlVender()
        {
            Font = Estilos.Normal;
            numPrecio.Font = Estilos.Total;
            numPrecio.Width = 260;

            var tabla = new TableLayoutPanel { ColumnCount = 1, AutoSize = true, Dock = DockStyle.Top };
            tabla.Controls.Add(Estilos.Etiqueta("¿Cuánto se cobra?", Estilos.Titulo));
            tabla.Controls.Add(lblNombre);
            tabla.Controls.Add(Estilos.Etiqueta("Precio"));
            tabla.Controls.Add(numPrecio);
            tabla.Controls.Add(lblReferencia);

            var botones = new FlowLayoutPanel { AutoSize = true, Margin = new Padding(0, 12, 0, 0) };
            var btnAgregar = Estilos.Boton("Agregar (Enter)", (_, _) => Confirmar(), 180);
            Estilos.Destacar(btnAgregar, Estilos.FondoBotonPrincipal);
            botones.Controls.Add(btnAgregar);
            botones.Controls.Add(Estilos.Boton("Cancelar (Esc)", (_, _) => Cancelado?.Invoke(this, EventArgs.Empty), 160));
            tabla.Controls.Add(botones);
            tabla.Controls.Add(lblError);

            Controls.Add(tabla);
        }

        public void Iniciar(Producto producto, int cantidad)
        {
            _producto = producto;
            _cantidad = cantidad;
            lblNombre.Text = cantidad == 1 ? producto.Nombre : $"{producto.Nombre} (x{cantidad}, precio de cada uno)";
            lblReferencia.Text = producto.PrecioVenta > 0
                ? $"De referencia: {producto.PrecioVenta:C2}. Escribí el precio de lo que se lleva y apretá Enter."
                : "Escribí el precio de lo que se lleva y apretá Enter.";
            lblError.Text = "";
            numPrecio.Value = 0;
            numPrecio.Text = "";
            numPrecio.Focus();
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            switch (keyData)
            {
                case Keys.Enter: Confirmar(); return true;
                case Keys.Escape: Cancelado?.Invoke(this, EventArgs.Empty); return true;
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }

        private void Confirmar()
        {
            if (_producto == null)
                return;
            var precio = numPrecio.ValorTipeado();
            if (precio <= 0)
            {
                lblError.Text = "Escribí un precio mayor a cero.";
                numPrecio.Focus();
                return;
            }
            Confirmado?.Invoke(this, (_producto, _cantidad, precio));
        }
    }
}

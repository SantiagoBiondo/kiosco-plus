namespace Proyecto_Kiosco_Rock_Roll.Interfaz.Vistas
{
    partial class VistaIngresoMercaderia
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                components?.Dispose();
                // El editor puede no estar en pantalla: se libera a mano.
                editor.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            panelPrincipal = new Panel();
            lstIngresos = new ListBox();
            lblEstado = new Label();
            lblAyuda = new Label();
            panelArriba = new FlowLayoutPanel();
            lblCantidad = new Label();
            numCantidad = new NumericUpDown();
            lblCodigo = new Label();
            txtCodigo = new TextBox();
            panelPrincipal.SuspendLayout();
            panelArriba.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)numCantidad).BeginInit();
            SuspendLayout();
            //
            // panelPrincipal
            //
            panelPrincipal.Controls.Add(lstIngresos);
            panelPrincipal.Controls.Add(lblEstado);
            panelPrincipal.Controls.Add(lblAyuda);
            panelPrincipal.Controls.Add(panelArriba);
            panelPrincipal.Dock = DockStyle.Fill;
            panelPrincipal.Name = "panelPrincipal";
            panelPrincipal.TabIndex = 0;
            //
            // lstIngresos
            //
            lstIngresos.Dock = DockStyle.Fill;
            lstIngresos.IntegralHeight = false;
            lstIngresos.Name = "lstIngresos";
            lstIngresos.TabIndex = 3;
            //
            // lblEstado
            //
            lblEstado.Dock = DockStyle.Bottom;
            lblEstado.Name = "lblEstado";
            lblEstado.Padding = new Padding(6);
            lblEstado.Size = new Size(1400, 36);
            lblEstado.TabIndex = 2;
            //
            // lblAyuda
            //
            lblAyuda.Dock = DockStyle.Top;
            lblAyuda.ForeColor = SystemColors.GrayText;
            lblAyuda.Name = "lblAyuda";
            lblAyuda.Padding = new Padding(8, 4, 0, 0);
            lblAyuda.Size = new Size(1400, 28);
            lblAyuda.TabIndex = 1;
            lblAyuda.Text = "Primero poné la cantidad (si es más de 1) y después escaneá. Abajo queda el registro de lo ingresado.";
            //
            // panelArriba
            //
            panelArriba.AutoSize = true;
            panelArriba.Controls.Add(lblCantidad);
            panelArriba.Controls.Add(numCantidad);
            panelArriba.Controls.Add(lblCodigo);
            panelArriba.Controls.Add(txtCodigo);
            panelArriba.Dock = DockStyle.Top;
            panelArriba.Name = "panelArriba";
            panelArriba.Padding = new Padding(6);
            panelArriba.TabIndex = 0;
            //
            // lblCantidad
            //
            lblCantidad.AutoSize = true;
            lblCantidad.Font = new Font("Segoe UI", 16F);
            lblCantidad.Margin = new Padding(3, 8, 3, 3);
            lblCantidad.Name = "lblCantidad";
            lblCantidad.TabIndex = 0;
            lblCantidad.Text = "Cantidad:";
            //
            // numCantidad
            //
            numCantidad.Font = new Font("Segoe UI", 16F);
            numCantidad.Maximum = new decimal(new int[] { 100000, 0, 0, 0 });
            numCantidad.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            numCantidad.Name = "numCantidad";
            numCantidad.Size = new Size(110, 36);
            numCantidad.TabIndex = 1;
            numCantidad.TextAlign = HorizontalAlignment.Right;
            numCantidad.ThousandsSeparator = true;
            numCantidad.Value = new decimal(new int[] { 1, 0, 0, 0 });
            //
            // lblCodigo
            //
            lblCodigo.AutoSize = true;
            lblCodigo.Font = new Font("Segoe UI", 16F);
            lblCodigo.Margin = new Padding(12, 8, 3, 3);
            lblCodigo.Name = "lblCodigo";
            lblCodigo.TabIndex = 2;
            lblCodigo.Text = "Código:";
            //
            // txtCodigo
            //
            txtCodigo.Font = new Font("Segoe UI", 16F);
            txtCodigo.Name = "txtCodigo";
            txtCodigo.PlaceholderText = "Escaneá el producto";
            txtCodigo.Size = new Size(380, 36);
            txtCodigo.TabIndex = 3;
            txtCodigo.KeyDown += TxtCodigo_KeyDown;
            //
            // VistaIngresoMercaderia
            //
            Controls.Add(panelPrincipal);
            Font = new Font("Segoe UI", 11F);
            Name = "VistaIngresoMercaderia";
            Size = new Size(1400, 700);
            panelPrincipal.ResumeLayout(false);
            panelPrincipal.PerformLayout();
            panelArriba.ResumeLayout(false);
            panelArriba.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)numCantidad).EndInit();
            ResumeLayout(false);
        }

        #endregion

        private Panel panelPrincipal;
        private ListBox lstIngresos;
        private Label lblEstado;
        private Label lblAyuda;
        private FlowLayoutPanel panelArriba;
        private Label lblCantidad;
        private NumericUpDown numCantidad;
        private Label lblCodigo;
        private TextBox txtCodigo;
    }
}

namespace Proyecto_Kiosco_Rock_Roll.Interfaz.Vistas
{
    partial class VistaVenta
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
                // Los controles del panel lateral pueden no estar en pantalla: se liberan a mano.
                editor.Dispose();
                panelCobro.Dispose();
                panelPrecio.Dispose();
                panelAbrirCaja.Dispose();
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
            DataGridViewCellStyle estiloCantidad = new DataGridViewCellStyle();
            DataGridViewCellStyle estiloPrecio = new DataGridViewCellStyle();
            DataGridViewCellStyle estiloSubtotal = new DataGridViewCellStyle();
            panelPrincipal = new Panel();
            grilla = new DataGridView();
            colCodigo = new DataGridViewTextBoxColumn();
            colNombre = new DataGridViewTextBoxColumn();
            colCantidad = new DataGridViewTextBoxColumn();
            colPrecio = new DataGridViewTextBoxColumn();
            colSubtotal = new DataGridViewTextBoxColumn();
            panelAbajo = new TableLayoutPanel();
            panelAcciones = new FlowLayoutPanel();
            btnSumar = new Button();
            btnRestar = new Button();
            btnQuitar = new Button();
            btnCancelarVenta = new Button();
            lblEstado = new Label();
            panelTotal = new FlowLayoutPanel();
            lblTituloTotal = new Label();
            lblTotal = new Label();
            btnCobrar = new Button();
            panelEscaneo = new FlowLayoutPanel();
            lblCodigo = new Label();
            txtCodigo = new TextBox();
            lblAyuda = new Label();
            panelPrincipal.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)grilla).BeginInit();
            panelAbajo.SuspendLayout();
            panelAcciones.SuspendLayout();
            panelTotal.SuspendLayout();
            panelEscaneo.SuspendLayout();
            SuspendLayout();
            //
            // panelPrincipal
            //
            // El orden importa: lo que tiene Dock = Fill (la grilla) va primero.
            panelPrincipal.Controls.Add(grilla);
            panelPrincipal.Controls.Add(panelAbajo);
            panelPrincipal.Controls.Add(panelEscaneo);
            panelPrincipal.Dock = DockStyle.Fill;
            panelPrincipal.Name = "panelPrincipal";
            panelPrincipal.TabIndex = 0;
            //
            // grilla
            //
            grilla.AllowUserToAddRows = false;
            grilla.AllowUserToDeleteRows = false;
            grilla.AllowUserToResizeRows = false;
            grilla.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            grilla.Columns.AddRange(new DataGridViewColumn[] { colCodigo, colNombre, colCantidad, colPrecio, colSubtotal });
            grilla.Dock = DockStyle.Fill;
            grilla.MultiSelect = false;
            grilla.Name = "grilla";
            grilla.RowHeadersVisible = false;
            grilla.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            grilla.TabIndex = 1;
            grilla.CellEndEdit += Grilla_CellEndEdit;
            grilla.CellValidating += Grilla_CellValidating;
            grilla.KeyDown += Grilla_KeyDown;
            //
            // colCodigo
            //
            colCodigo.HeaderText = "Código";
            colCodigo.Name = "colCodigo";
            colCodigo.ReadOnly = true;
            colCodigo.Width = 160;
            //
            // colNombre
            //
            colNombre.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            colNombre.HeaderText = "Producto";
            colNombre.Name = "colNombre";
            colNombre.ReadOnly = true;
            //
            // colCantidad
            //
            estiloCantidad.Alignment = DataGridViewContentAlignment.MiddleRight;
            estiloCantidad.Format = "N0";
            colCantidad.DefaultCellStyle = estiloCantidad;
            colCantidad.HeaderText = "Cant.";
            colCantidad.Name = "colCantidad";
            colCantidad.Width = 90;
            //
            // colPrecio
            //
            estiloPrecio.Alignment = DataGridViewContentAlignment.MiddleRight;
            estiloPrecio.Format = "C2";
            colPrecio.DefaultCellStyle = estiloPrecio;
            colPrecio.HeaderText = "Precio";
            colPrecio.Name = "colPrecio";
            colPrecio.ReadOnly = true;
            colPrecio.Width = 140;
            //
            // colSubtotal
            //
            estiloSubtotal.Alignment = DataGridViewContentAlignment.MiddleRight;
            estiloSubtotal.Format = "C2";
            colSubtotal.DefaultCellStyle = estiloSubtotal;
            colSubtotal.HeaderText = "Subtotal";
            colSubtotal.Name = "colSubtotal";
            colSubtotal.ReadOnly = true;
            colSubtotal.Width = 160;
            //
            // panelAbajo
            //
            panelAbajo.AutoSize = true;
            panelAbajo.ColumnCount = 3;
            panelAbajo.ColumnStyles.Add(new ColumnStyle());
            panelAbajo.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            panelAbajo.ColumnStyles.Add(new ColumnStyle());
            panelAbajo.Controls.Add(panelAcciones, 0, 0);
            panelAbajo.Controls.Add(lblEstado, 1, 0);
            panelAbajo.Controls.Add(panelTotal, 2, 0);
            panelAbajo.Dock = DockStyle.Bottom;
            panelAbajo.Name = "panelAbajo";
            panelAbajo.Padding = new Padding(6);
            panelAbajo.RowCount = 1;
            panelAbajo.RowStyles.Add(new RowStyle());
            panelAbajo.TabIndex = 2;
            //
            // panelAcciones
            //
            panelAcciones.AutoSize = true;
            panelAcciones.Controls.Add(btnSumar);
            panelAcciones.Controls.Add(btnRestar);
            panelAcciones.Controls.Add(btnQuitar);
            panelAcciones.Controls.Add(btnCancelarVenta);
            panelAcciones.Name = "panelAcciones";
            panelAcciones.TabIndex = 0;
            panelAcciones.WrapContents = false;
            //
            // btnSumar
            //
            btnSumar.Margin = new Padding(4);
            btnSumar.Name = "btnSumar";
            btnSumar.Size = new Size(60, 42);
            btnSumar.TabIndex = 0;
            btnSumar.Text = "+1";
            btnSumar.UseVisualStyleBackColor = true;
            btnSumar.Click += BtnSumar_Click;
            //
            // btnRestar
            //
            btnRestar.Margin = new Padding(4);
            btnRestar.Name = "btnRestar";
            btnRestar.Size = new Size(60, 42);
            btnRestar.TabIndex = 1;
            btnRestar.Text = "-1";
            btnRestar.UseVisualStyleBackColor = true;
            btnRestar.Click += BtnRestar_Click;
            //
            // btnQuitar
            //
            btnQuitar.Margin = new Padding(4);
            btnQuitar.Name = "btnQuitar";
            btnQuitar.Size = new Size(150, 42);
            btnQuitar.TabIndex = 2;
            btnQuitar.Text = "Quitar (Supr)";
            btnQuitar.UseVisualStyleBackColor = true;
            btnQuitar.Click += BtnQuitar_Click;
            //
            // btnCancelarVenta
            //
            btnCancelarVenta.Margin = new Padding(4);
            btnCancelarVenta.Name = "btnCancelarVenta";
            btnCancelarVenta.Size = new Size(170, 42);
            btnCancelarVenta.TabIndex = 3;
            btnCancelarVenta.Text = "Cancelar venta";
            btnCancelarVenta.UseVisualStyleBackColor = true;
            btnCancelarVenta.Click += BtnCancelarVenta_Click;
            //
            // lblEstado
            //
            lblEstado.Anchor = AnchorStyles.Left;
            lblEstado.AutoSize = true;
            lblEstado.Name = "lblEstado";
            lblEstado.TabIndex = 1;
            //
            // panelTotal
            //
            panelTotal.AutoSize = true;
            panelTotal.Controls.Add(lblTituloTotal);
            panelTotal.Controls.Add(lblTotal);
            panelTotal.Controls.Add(btnCobrar);
            panelTotal.Name = "panelTotal";
            panelTotal.TabIndex = 2;
            panelTotal.WrapContents = false;
            //
            // lblTituloTotal
            //
            lblTituloTotal.AutoSize = true;
            lblTituloTotal.Font = new Font("Segoe UI", 16F);
            lblTituloTotal.Margin = new Padding(3, 16, 3, 3);
            lblTituloTotal.Name = "lblTituloTotal";
            lblTituloTotal.TabIndex = 0;
            lblTituloTotal.Text = "TOTAL:";
            //
            // lblTotal
            //
            lblTotal.AutoSize = true;
            lblTotal.Font = new Font("Segoe UI", 28F, FontStyle.Bold);
            lblTotal.Name = "lblTotal";
            lblTotal.TabIndex = 1;
            lblTotal.Text = "$ 0,00";
            //
            // btnCobrar
            //
            btnCobrar.Font = new Font("Segoe UI", 16F);
            btnCobrar.Margin = new Padding(4);
            btnCobrar.Name = "btnCobrar";
            btnCobrar.Size = new Size(200, 60);
            btnCobrar.TabIndex = 2;
            btnCobrar.Text = "COBRAR (F12)";
            btnCobrar.UseVisualStyleBackColor = true;
            btnCobrar.Click += BtnCobrar_Click;
            //
            // panelEscaneo
            //
            panelEscaneo.AutoSize = true;
            panelEscaneo.Controls.Add(lblCodigo);
            panelEscaneo.Controls.Add(txtCodigo);
            panelEscaneo.Controls.Add(lblAyuda);
            panelEscaneo.Dock = DockStyle.Top;
            panelEscaneo.Name = "panelEscaneo";
            panelEscaneo.Padding = new Padding(6);
            panelEscaneo.TabIndex = 0;
            //
            // lblCodigo
            //
            lblCodigo.AutoSize = true;
            lblCodigo.Font = new Font("Segoe UI", 16F);
            lblCodigo.Margin = new Padding(3, 8, 3, 3);
            lblCodigo.Name = "lblCodigo";
            lblCodigo.TabIndex = 0;
            lblCodigo.Text = "Código o nombre:";
            //
            // txtCodigo
            //
            txtCodigo.Font = new Font("Segoe UI", 16F);
            txtCodigo.Name = "txtCodigo";
            txtCodigo.PlaceholderText = "Escaneá el código o escribí el nombre";
            txtCodigo.Size = new Size(520, 36);
            txtCodigo.TabIndex = 1;
            //
            // lblAyuda
            //
            lblAyuda.AutoSize = true;
            lblAyuda.ForeColor = SystemColors.GrayText;
            lblAyuda.Margin = new Padding(12, 12, 3, 3);
            lblAyuda.Name = "lblAyuda";
            lblAyuda.TabIndex = 2;
            lblAyuda.Text = "\"3*código\" agrega 3  ·  Supr quita el elegido  ·  + y − cambian la cantidad  ·  ↑ ↓ eligen otro";
            //
            // VistaVenta
            //
            Controls.Add(panelPrincipal);
            Font = new Font("Segoe UI", 11F);
            Name = "VistaVenta";
            Size = new Size(1400, 700);
            panelPrincipal.ResumeLayout(false);
            panelPrincipal.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)grilla).EndInit();
            panelAbajo.ResumeLayout(false);
            panelAbajo.PerformLayout();
            panelAcciones.ResumeLayout(false);
            panelAcciones.PerformLayout();
            panelTotal.ResumeLayout(false);
            panelTotal.PerformLayout();
            panelEscaneo.ResumeLayout(false);
            panelEscaneo.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        private Panel panelPrincipal;
        private DataGridView grilla;
        private DataGridViewTextBoxColumn colCodigo;
        private DataGridViewTextBoxColumn colNombre;
        private DataGridViewTextBoxColumn colCantidad;
        private DataGridViewTextBoxColumn colPrecio;
        private DataGridViewTextBoxColumn colSubtotal;
        private TableLayoutPanel panelAbajo;
        private FlowLayoutPanel panelAcciones;
        private Button btnSumar;
        private Button btnRestar;
        private Button btnQuitar;
        private Button btnCancelarVenta;
        private Label lblEstado;
        private FlowLayoutPanel panelTotal;
        private Label lblTituloTotal;
        private Label lblTotal;
        private Button btnCobrar;
        private FlowLayoutPanel panelEscaneo;
        private Label lblCodigo;
        private TextBox txtCodigo;
        private Label lblAyuda;
    }
}

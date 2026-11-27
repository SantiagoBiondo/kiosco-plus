namespace Proyecto_Kiosco_Rock_Roll.Interfaz.Vistas
{
    partial class VistaEtiquetas
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
                // La hoja de la vista previa la arma el programa: se libera a mano.
                vistaPrevia.Document?.Dispose();
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
            DataGridViewCellStyle estiloPrecio = new DataGridViewCellStyle();
            DataGridViewCellStyle estiloCantidad = new DataGridViewCellStyle();
            panelPrincipal = new Panel();
            grilla = new DataGridView();
            colNombre = new DataGridViewTextBoxColumn();
            colPrecio = new DataGridViewTextBoxColumn();
            colCantidad = new DataGridViewTextBoxColumn();
            panelAcciones = new FlowLayoutPanel();
            btnSumar = new Button();
            btnRestar = new Button();
            btnQuitar = new Button();
            btnVaciar = new Button();
            lblEstado = new Label();
            panelImprimir = new FlowLayoutPanel();
            lblTotal = new Label();
            lblImpresora = new Label();
            cmbImpresora = new ComboBox();
            btnImprimir = new Button();
            panelEscaneo = new FlowLayoutPanel();
            lblCodigo = new Label();
            txtCodigo = new TextBox();
            lblAyuda = new Label();
            panelPrevia = new Panel();
            vistaPrevia = new PrintPreviewControl();
            panelNavegacion = new FlowLayoutPanel();
            lblTituloPrevia = new Label();
            btnHojaAnterior = new Button();
            lblHoja = new Label();
            btnHojaSiguiente = new Button();
            panelPrincipal.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)grilla).BeginInit();
            panelAcciones.SuspendLayout();
            panelImprimir.SuspendLayout();
            panelEscaneo.SuspendLayout();
            panelPrevia.SuspendLayout();
            panelNavegacion.SuspendLayout();
            SuspendLayout();
            //
            // panelPrincipal
            //
            // El orden importa: lo que tiene Dock = Fill (la grilla) va primero.
            panelPrincipal.Controls.Add(grilla);
            panelPrincipal.Controls.Add(panelAcciones);
            panelPrincipal.Controls.Add(panelImprimir);
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
            grilla.Columns.AddRange(new DataGridViewColumn[] { colNombre, colPrecio, colCantidad });
            grilla.Dock = DockStyle.Fill;
            grilla.MultiSelect = false;
            grilla.Name = "grilla";
            grilla.ReadOnly = true;
            grilla.RowHeadersVisible = false;
            grilla.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            grilla.TabIndex = 1;
            grilla.KeyDown += Grilla_KeyDown;
            //
            // colNombre
            //
            colNombre.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            colNombre.HeaderText = "Producto";
            colNombre.Name = "colNombre";
            colNombre.ReadOnly = true;
            //
            // colPrecio
            //
            estiloPrecio.Alignment = DataGridViewContentAlignment.MiddleRight;
            estiloPrecio.Format = "C2";
            colPrecio.DefaultCellStyle = estiloPrecio;
            colPrecio.HeaderText = "Precio";
            colPrecio.Name = "colPrecio";
            colPrecio.ReadOnly = true;
            colPrecio.Width = 130;
            //
            // colCantidad
            //
            estiloCantidad.Alignment = DataGridViewContentAlignment.MiddleRight;
            estiloCantidad.Format = "N0";
            colCantidad.DefaultCellStyle = estiloCantidad;
            colCantidad.HeaderText = "Etiquetas";
            colCantidad.Name = "colCantidad";
            colCantidad.ReadOnly = true;
            colCantidad.Width = 100;
            //
            // panelAcciones
            //
            panelAcciones.AutoSize = true;
            panelAcciones.Controls.Add(btnSumar);
            panelAcciones.Controls.Add(btnRestar);
            panelAcciones.Controls.Add(btnQuitar);
            panelAcciones.Controls.Add(btnVaciar);
            panelAcciones.Controls.Add(lblEstado);
            panelAcciones.Dock = DockStyle.Bottom;
            panelAcciones.Name = "panelAcciones";
            panelAcciones.Padding = new Padding(6);
            panelAcciones.TabIndex = 2;
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
            // btnVaciar
            //
            btnVaciar.Margin = new Padding(4);
            btnVaciar.Name = "btnVaciar";
            btnVaciar.Size = new Size(150, 42);
            btnVaciar.TabIndex = 3;
            btnVaciar.Text = "Vaciar lista";
            btnVaciar.UseVisualStyleBackColor = true;
            btnVaciar.Click += BtnVaciar_Click;
            //
            // lblEstado
            //
            lblEstado.AutoSize = true;
            lblEstado.Margin = new Padding(12, 14, 3, 3);
            lblEstado.MaximumSize = new Size(600, 0);
            lblEstado.Name = "lblEstado";
            lblEstado.TabIndex = 4;
            //
            // panelImprimir
            //
            panelImprimir.AutoSize = true;
            panelImprimir.Controls.Add(lblTotal);
            panelImprimir.Controls.Add(lblImpresora);
            panelImprimir.Controls.Add(cmbImpresora);
            panelImprimir.Controls.Add(btnImprimir);
            panelImprimir.Dock = DockStyle.Bottom;
            panelImprimir.Name = "panelImprimir";
            panelImprimir.Padding = new Padding(6);
            panelImprimir.TabIndex = 3;
            //
            // lblTotal
            //
            lblTotal.AutoSize = true;
            lblTotal.Font = new Font("Segoe UI", 16F);
            lblTotal.Margin = new Padding(3, 12, 12, 3);
            lblTotal.Name = "lblTotal";
            lblTotal.TabIndex = 0;
            lblTotal.Text = "Sin etiquetas";
            //
            // lblImpresora
            //
            lblImpresora.AutoSize = true;
            lblImpresora.Margin = new Padding(12, 14, 3, 3);
            lblImpresora.Name = "lblImpresora";
            lblImpresora.TabIndex = 1;
            lblImpresora.Text = "Impresora:";
            //
            // cmbImpresora
            //
            cmbImpresora.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbImpresora.Margin = new Padding(3, 10, 3, 3);
            cmbImpresora.Name = "cmbImpresora";
            cmbImpresora.Size = new Size(300, 28);
            cmbImpresora.TabIndex = 2;
            cmbImpresora.SelectedIndexChanged += CmbImpresora_SelectedIndexChanged;
            //
            // btnImprimir
            //
            btnImprimir.Font = new Font("Segoe UI", 16F);
            btnImprimir.Margin = new Padding(4);
            btnImprimir.Name = "btnImprimir";
            btnImprimir.Size = new Size(200, 60);
            btnImprimir.TabIndex = 3;
            btnImprimir.Text = "IMPRIMIR (F12)";
            btnImprimir.UseVisualStyleBackColor = true;
            btnImprimir.Click += BtnImprimir_Click;
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
            lblAyuda.Text = "5*código agrega 5  ·  Supr quita el elegido  ·  + y − cambian la cantidad  ·  ↑ ↓ eligen otro";
            //
            // panelPrevia
            //
            panelPrevia.Controls.Add(vistaPrevia);
            panelPrevia.Controls.Add(panelNavegacion);
            panelPrevia.Dock = DockStyle.Right;
            panelPrevia.Name = "panelPrevia";
            panelPrevia.Padding = new Padding(8);
            panelPrevia.Size = new Size(560, 700);
            panelPrevia.TabIndex = 1;
            //
            // vistaPrevia
            //
            vistaPrevia.AutoZoom = true;
            vistaPrevia.Dock = DockStyle.Fill;
            vistaPrevia.Name = "vistaPrevia";
            vistaPrevia.TabIndex = 1;
            //
            // panelNavegacion
            //
            panelNavegacion.AutoSize = true;
            panelNavegacion.Controls.Add(lblTituloPrevia);
            panelNavegacion.Controls.Add(btnHojaAnterior);
            panelNavegacion.Controls.Add(lblHoja);
            panelNavegacion.Controls.Add(btnHojaSiguiente);
            panelNavegacion.Dock = DockStyle.Top;
            panelNavegacion.Name = "panelNavegacion";
            panelNavegacion.TabIndex = 0;
            //
            // lblTituloPrevia
            //
            lblTituloPrevia.Anchor = AnchorStyles.Left;
            lblTituloPrevia.AutoSize = true;
            lblTituloPrevia.Font = new Font("Segoe UI", 15F, FontStyle.Bold);
            lblTituloPrevia.Margin = new Padding(3, 8, 12, 8);
            lblTituloPrevia.Name = "lblTituloPrevia";
            lblTituloPrevia.TabIndex = 0;
            lblTituloPrevia.Text = "Así sale la hoja";
            //
            // btnHojaAnterior
            //
            btnHojaAnterior.Margin = new Padding(4);
            btnHojaAnterior.Name = "btnHojaAnterior";
            btnHojaAnterior.Size = new Size(50, 42);
            btnHojaAnterior.TabIndex = 1;
            btnHojaAnterior.Text = "◀";
            btnHojaAnterior.UseVisualStyleBackColor = true;
            btnHojaAnterior.Click += BtnHojaAnterior_Click;
            //
            // lblHoja
            //
            lblHoja.AutoSize = true;
            lblHoja.Margin = new Padding(8, 12, 8, 3);
            lblHoja.Name = "lblHoja";
            lblHoja.TabIndex = 2;
            //
            // btnHojaSiguiente
            //
            btnHojaSiguiente.Margin = new Padding(4);
            btnHojaSiguiente.Name = "btnHojaSiguiente";
            btnHojaSiguiente.Size = new Size(50, 42);
            btnHojaSiguiente.TabIndex = 3;
            btnHojaSiguiente.Text = "▶";
            btnHojaSiguiente.UseVisualStyleBackColor = true;
            btnHojaSiguiente.Click += BtnHojaSiguiente_Click;
            //
            // VistaEtiquetas
            //
            Controls.Add(panelPrincipal);
            Controls.Add(panelPrevia);
            Font = new Font("Segoe UI", 11F);
            Name = "VistaEtiquetas";
            Size = new Size(1400, 700);
            panelPrincipal.ResumeLayout(false);
            panelPrincipal.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)grilla).EndInit();
            panelAcciones.ResumeLayout(false);
            panelAcciones.PerformLayout();
            panelImprimir.ResumeLayout(false);
            panelImprimir.PerformLayout();
            panelEscaneo.ResumeLayout(false);
            panelEscaneo.PerformLayout();
            panelPrevia.ResumeLayout(false);
            panelPrevia.PerformLayout();
            panelNavegacion.ResumeLayout(false);
            panelNavegacion.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        private Panel panelPrincipal;
        private DataGridView grilla;
        private DataGridViewTextBoxColumn colNombre;
        private DataGridViewTextBoxColumn colPrecio;
        private DataGridViewTextBoxColumn colCantidad;
        private FlowLayoutPanel panelAcciones;
        private Button btnSumar;
        private Button btnRestar;
        private Button btnQuitar;
        private Button btnVaciar;
        private Label lblEstado;
        private FlowLayoutPanel panelImprimir;
        private Label lblTotal;
        private Label lblImpresora;
        private ComboBox cmbImpresora;
        private Button btnImprimir;
        private FlowLayoutPanel panelEscaneo;
        private Label lblCodigo;
        private TextBox txtCodigo;
        private Label lblAyuda;
        private Panel panelPrevia;
        private PrintPreviewControl vistaPrevia;
        private FlowLayoutPanel panelNavegacion;
        private Label lblTituloPrevia;
        private Button btnHojaAnterior;
        private Label lblHoja;
        private Button btnHojaSiguiente;
    }
}

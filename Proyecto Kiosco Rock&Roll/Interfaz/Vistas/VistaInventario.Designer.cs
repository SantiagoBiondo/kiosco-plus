namespace Proyecto_Kiosco_Rock_Roll.Interfaz.Vistas
{
    partial class VistaInventario
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
                // Los paneles que no están en pantalla no son hijos de la vista: se liberan a mano.
                editor.Dispose();
                ajustePrecios.Dispose();
                panelCategorias.Dispose();
                panelProveedores.Dispose();
                panelPedido.Dispose();
                panelImportacion.Dispose();
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
            DataGridViewCellStyle estiloCosto = new DataGridViewCellStyle();
            DataGridViewCellStyle estiloVenta = new DataGridViewCellStyle();
            DataGridViewCellStyle estiloStock = new DataGridViewCellStyle();
            DataGridViewCellStyle estiloMinimo = new DataGridViewCellStyle();
            panelPrincipal = new Panel();
            grilla = new DataGridView();
            colMarca = new DataGridViewCheckBoxColumn();
            colCodigo = new DataGridViewTextBoxColumn();
            colNombre = new DataGridViewTextBoxColumn();
            colCategoria = new DataGridViewTextBoxColumn();
            colProveedor = new DataGridViewTextBoxColumn();
            colCosto = new DataGridViewTextBoxColumn();
            colVenta = new DataGridViewTextBoxColumn();
            colStock = new DataGridViewTextBoxColumn();
            colMinimo = new DataGridViewTextBoxColumn();
            panelAbajo = new FlowLayoutPanel();
            btnNuevo = new Button();
            btnEditar = new Button();
            btnEliminar = new Button();
            btnActualizarPrecios = new Button();
            btnArmarPedido = new Button();
            btnCategorias = new Button();
            btnProveedores = new Button();
            btnImportar = new Button();
            lblEstado = new Label();
            panelArriba = new FlowLayoutPanel();
            txtBuscar = new TextBox();
            lblCategoria = new Label();
            cmbCategoria = new ComboBox();
            lblProveedor = new Label();
            cmbProveedor = new ComboBox();
            chkStockBajo = new CheckBox();
            lblResumen = new Label();
            lblMarcados = new Label();
            btnDesmarcar = new Button();
            panelPrincipal.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)grilla).BeginInit();
            panelAbajo.SuspendLayout();
            panelArriba.SuspendLayout();
            SuspendLayout();
            //
            // panelPrincipal
            //
            // El orden importa: lo que tiene Dock = Fill (la grilla) va primero.
            panelPrincipal.Controls.Add(grilla);
            panelPrincipal.Controls.Add(panelAbajo);
            panelPrincipal.Controls.Add(panelArriba);
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
            grilla.Columns.AddRange(new DataGridViewColumn[] { colMarca, colCodigo, colNombre, colCategoria, colProveedor, colCosto, colVenta, colStock, colMinimo });
            grilla.Dock = DockStyle.Fill;
            grilla.MultiSelect = false;
            grilla.Name = "grilla";
            grilla.ReadOnly = true;
            grilla.RowHeadersVisible = false;
            grilla.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            grilla.TabIndex = 1;
            grilla.CellDoubleClick += Grilla_CellDoubleClick;
            grilla.CellFormatting += Grilla_CellFormatting;
            grilla.CellMouseClick += Grilla_CellMouseClick;
            grilla.PreviewKeyDown += Grilla_PreviewKeyDown;
            //
            // colMarca
            //
            colMarca.HeaderText = "✓";
            colMarca.Name = "colMarca";
            colMarca.ReadOnly = true;
            colMarca.Resizable = DataGridViewTriState.False;
            colMarca.ToolTipText = "Marcá varios productos (clic o barra espaciadora) para cambiarles el precio juntos con F6";
            colMarca.Width = 44;
            //
            // colCodigo
            //
            colCodigo.DataPropertyName = "CodigoBarras";
            colCodigo.HeaderText = "Código";
            colCodigo.Name = "colCodigo";
            colCodigo.ReadOnly = true;
            colCodigo.Width = 150;
            //
            // colNombre
            //
            colNombre.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            colNombre.DataPropertyName = "Nombre";
            colNombre.HeaderText = "Producto";
            colNombre.Name = "colNombre";
            colNombre.ReadOnly = true;
            //
            // colCategoria
            //
            colCategoria.DataPropertyName = "CategoriaId";
            colCategoria.HeaderText = "Categoría";
            colCategoria.Name = "colCategoria";
            colCategoria.ReadOnly = true;
            colCategoria.Width = 150;
            //
            // colProveedor
            //
            colProveedor.DataPropertyName = "ProveedorId";
            colProveedor.HeaderText = "Proveedor";
            colProveedor.Name = "colProveedor";
            colProveedor.ReadOnly = true;
            colProveedor.Width = 150;
            //
            // colCosto
            //
            colCosto.DataPropertyName = "PrecioCosto";
            estiloCosto.Alignment = DataGridViewContentAlignment.MiddleRight;
            estiloCosto.Format = "C2";
            colCosto.DefaultCellStyle = estiloCosto;
            colCosto.HeaderText = "Costo";
            colCosto.Name = "colCosto";
            colCosto.ReadOnly = true;
            colCosto.Width = 110;
            //
            // colVenta
            //
            colVenta.DataPropertyName = "PrecioVenta";
            estiloVenta.Alignment = DataGridViewContentAlignment.MiddleRight;
            estiloVenta.Format = "C2";
            colVenta.DefaultCellStyle = estiloVenta;
            colVenta.HeaderText = "Venta";
            colVenta.Name = "colVenta";
            colVenta.ReadOnly = true;
            colVenta.Width = 110;
            //
            // colStock
            //
            colStock.DataPropertyName = "Stock";
            estiloStock.Alignment = DataGridViewContentAlignment.MiddleRight;
            estiloStock.Format = "N0";
            colStock.DefaultCellStyle = estiloStock;
            colStock.HeaderText = "Stock";
            colStock.Name = "colStock";
            colStock.ReadOnly = true;
            colStock.Width = 75;
            //
            // colMinimo
            //
            colMinimo.DataPropertyName = "StockMinimo";
            estiloMinimo.Alignment = DataGridViewContentAlignment.MiddleRight;
            estiloMinimo.Format = "N0";
            colMinimo.DefaultCellStyle = estiloMinimo;
            colMinimo.HeaderText = "Mínimo";
            colMinimo.Name = "colMinimo";
            colMinimo.ReadOnly = true;
            colMinimo.Width = 75;
            //
            // panelAbajo
            //
            panelAbajo.AutoSize = true;
            panelAbajo.Controls.Add(btnNuevo);
            panelAbajo.Controls.Add(btnEditar);
            panelAbajo.Controls.Add(btnEliminar);
            panelAbajo.Controls.Add(btnActualizarPrecios);
            panelAbajo.Controls.Add(btnArmarPedido);
            panelAbajo.Controls.Add(btnCategorias);
            panelAbajo.Controls.Add(btnProveedores);
            panelAbajo.Controls.Add(btnImportar);
            panelAbajo.Controls.Add(lblEstado);
            panelAbajo.Dock = DockStyle.Bottom;
            panelAbajo.Name = "panelAbajo";
            panelAbajo.Padding = new Padding(6);
            panelAbajo.TabIndex = 2;
            //
            // btnNuevo
            //
            btnNuevo.Margin = new Padding(4);
            btnNuevo.Name = "btnNuevo";
            btnNuevo.Size = new Size(150, 42);
            btnNuevo.TabIndex = 0;
            btnNuevo.Text = "Nuevo (Insert)";
            btnNuevo.UseVisualStyleBackColor = true;
            btnNuevo.Click += BtnNuevo_Click;
            //
            // btnEditar
            //
            btnEditar.Margin = new Padding(4);
            btnEditar.Name = "btnEditar";
            btnEditar.Size = new Size(150, 42);
            btnEditar.TabIndex = 1;
            btnEditar.Text = "Editar (Enter)";
            btnEditar.UseVisualStyleBackColor = true;
            btnEditar.Click += BtnEditar_Click;
            //
            // btnEliminar
            //
            btnEliminar.Margin = new Padding(4);
            btnEliminar.Name = "btnEliminar";
            btnEliminar.Size = new Size(150, 42);
            btnEliminar.TabIndex = 2;
            btnEliminar.Text = "Eliminar (Supr)";
            btnEliminar.UseVisualStyleBackColor = true;
            btnEliminar.Click += BtnEliminar_Click;
            //
            // btnActualizarPrecios
            //
            btnActualizarPrecios.Margin = new Padding(4);
            btnActualizarPrecios.Name = "btnActualizarPrecios";
            btnActualizarPrecios.Size = new Size(210, 42);
            btnActualizarPrecios.TabIndex = 3;
            btnActualizarPrecios.Text = "Actualizar precios (F6)";
            btnActualizarPrecios.UseVisualStyleBackColor = true;
            btnActualizarPrecios.Click += BtnActualizarPrecios_Click;
            //
            // btnArmarPedido
            //
            btnArmarPedido.Margin = new Padding(4);
            btnArmarPedido.Name = "btnArmarPedido";
            btnArmarPedido.Size = new Size(180, 42);
            btnArmarPedido.TabIndex = 4;
            btnArmarPedido.Text = "Armar pedido (F7)";
            btnArmarPedido.UseVisualStyleBackColor = true;
            btnArmarPedido.Click += BtnArmarPedido_Click;
            //
            // btnCategorias
            //
            btnCategorias.Margin = new Padding(4);
            btnCategorias.Name = "btnCategorias";
            btnCategorias.Size = new Size(160, 42);
            btnCategorias.TabIndex = 5;
            btnCategorias.Text = "Categorías (F8)";
            btnCategorias.UseVisualStyleBackColor = true;
            btnCategorias.Click += BtnCategorias_Click;
            //
            // btnProveedores
            //
            btnProveedores.Margin = new Padding(4);
            btnProveedores.Name = "btnProveedores";
            btnProveedores.Size = new Size(170, 42);
            btnProveedores.TabIndex = 6;
            btnProveedores.Text = "Proveedores (F9)";
            btnProveedores.UseVisualStyleBackColor = true;
            btnProveedores.Click += BtnProveedores_Click;
            //
            // btnImportar
            //
            btnImportar.Margin = new Padding(4);
            btnImportar.Name = "btnImportar";
            btnImportar.Size = new Size(190, 42);
            btnImportar.TabIndex = 7;
            btnImportar.Text = "Importar de Excel…";
            btnImportar.UseVisualStyleBackColor = true;
            btnImportar.Click += BtnImportar_Click;
            //
            // lblEstado
            //
            lblEstado.AutoSize = true;
            lblEstado.Margin = new Padding(12, 14, 3, 3);
            lblEstado.Name = "lblEstado";
            lblEstado.TabIndex = 8;
            //
            // panelArriba
            //
            panelArriba.AutoSize = true;
            panelArriba.Controls.Add(txtBuscar);
            panelArriba.Controls.Add(lblCategoria);
            panelArriba.Controls.Add(cmbCategoria);
            panelArriba.Controls.Add(lblProveedor);
            panelArriba.Controls.Add(cmbProveedor);
            panelArriba.Controls.Add(chkStockBajo);
            panelArriba.Controls.Add(lblResumen);
            panelArriba.Controls.Add(lblMarcados);
            panelArriba.Controls.Add(btnDesmarcar);
            panelArriba.Dock = DockStyle.Top;
            panelArriba.Name = "panelArriba";
            panelArriba.Padding = new Padding(6);
            panelArriba.TabIndex = 0;
            //
            // txtBuscar
            //
            txtBuscar.Font = new Font("Segoe UI", 16F);
            txtBuscar.Name = "txtBuscar";
            txtBuscar.PlaceholderText = "Buscar por nombre o código";
            txtBuscar.Size = new Size(320, 36);
            txtBuscar.TabIndex = 0;
            txtBuscar.TextChanged += TxtBuscar_TextChanged;
            //
            // lblCategoria
            //
            lblCategoria.AutoSize = true;
            lblCategoria.Margin = new Padding(12, 10, 3, 3);
            lblCategoria.Name = "lblCategoria";
            lblCategoria.TabIndex = 1;
            lblCategoria.Text = "Categoría:";
            //
            // cmbCategoria
            //
            cmbCategoria.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbCategoria.Margin = new Padding(3, 6, 3, 3);
            cmbCategoria.Name = "cmbCategoria";
            cmbCategoria.Size = new Size(200, 28);
            cmbCategoria.TabIndex = 2;
            cmbCategoria.SelectedIndexChanged += CmbFiltro_SelectedIndexChanged;
            //
            // lblProveedor
            //
            lblProveedor.AutoSize = true;
            lblProveedor.Margin = new Padding(12, 10, 3, 3);
            lblProveedor.Name = "lblProveedor";
            lblProveedor.TabIndex = 3;
            lblProveedor.Text = "Proveedor:";
            //
            // cmbProveedor
            //
            cmbProveedor.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbProveedor.Margin = new Padding(3, 6, 3, 3);
            cmbProveedor.Name = "cmbProveedor";
            cmbProveedor.Size = new Size(200, 28);
            cmbProveedor.TabIndex = 4;
            cmbProveedor.SelectedIndexChanged += CmbFiltro_SelectedIndexChanged;
            //
            // chkStockBajo
            //
            chkStockBajo.AutoSize = true;
            chkStockBajo.Margin = new Padding(12, 10, 3, 3);
            chkStockBajo.Name = "chkStockBajo";
            chkStockBajo.TabIndex = 5;
            chkStockBajo.Text = "Solo stock bajo";
            chkStockBajo.UseVisualStyleBackColor = true;
            chkStockBajo.CheckedChanged += ChkStockBajo_CheckedChanged;
            //
            // lblResumen
            //
            lblResumen.AutoSize = true;
            lblResumen.Margin = new Padding(12, 14, 3, 3);
            lblResumen.Name = "lblResumen";
            lblResumen.TabIndex = 6;
            //
            // lblMarcados
            //
            lblMarcados.AutoSize = true;
            lblMarcados.Margin = new Padding(12, 10, 3, 3);
            lblMarcados.Name = "lblMarcados";
            lblMarcados.TabIndex = 7;
            //
            // btnDesmarcar
            //
            btnDesmarcar.Margin = new Padding(6, 2, 3, 3);
            btnDesmarcar.Name = "btnDesmarcar";
            btnDesmarcar.Size = new Size(170, 34);
            btnDesmarcar.TabIndex = 8;
            btnDesmarcar.Text = "Desmarcar todos";
            btnDesmarcar.UseVisualStyleBackColor = true;
            btnDesmarcar.Click += BtnDesmarcar_Click;
            //
            // VistaInventario
            //
            Controls.Add(panelPrincipal);
            Font = new Font("Segoe UI", 11F);
            Name = "VistaInventario";
            Size = new Size(1400, 700);
            panelPrincipal.ResumeLayout(false);
            panelPrincipal.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)grilla).EndInit();
            panelAbajo.ResumeLayout(false);
            panelAbajo.PerformLayout();
            panelArriba.ResumeLayout(false);
            panelArriba.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        private Panel panelPrincipal;
        private DataGridView grilla;
        private DataGridViewCheckBoxColumn colMarca;
        private DataGridViewTextBoxColumn colCodigo;
        private DataGridViewTextBoxColumn colNombre;
        private DataGridViewTextBoxColumn colCategoria;
        private DataGridViewTextBoxColumn colProveedor;
        private DataGridViewTextBoxColumn colCosto;
        private DataGridViewTextBoxColumn colVenta;
        private DataGridViewTextBoxColumn colStock;
        private DataGridViewTextBoxColumn colMinimo;
        private FlowLayoutPanel panelAbajo;
        private Button btnNuevo;
        private Button btnEditar;
        private Button btnEliminar;
        private Button btnActualizarPrecios;
        private Button btnArmarPedido;
        private Button btnCategorias;
        private Button btnProveedores;
        private Button btnImportar;
        private Label lblEstado;
        private FlowLayoutPanel panelArriba;
        private TextBox txtBuscar;
        private Label lblCategoria;
        private ComboBox cmbCategoria;
        private Label lblProveedor;
        private ComboBox cmbProveedor;
        private CheckBox chkStockBajo;
        private Label lblResumen;
        private Label lblMarcados;
        private Button btnDesmarcar;
    }
}

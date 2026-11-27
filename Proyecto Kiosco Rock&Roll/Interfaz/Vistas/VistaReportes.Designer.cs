namespace Proyecto_Kiosco_Rock_Roll.Interfaz.Vistas
{
    partial class VistaReportes
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
            if (disposing && (components != null))
            {
                components.Dispose();
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
            DataGridViewCellStyle estiloPeriodosVentas = new DataGridViewCellStyle();
            DataGridViewCellStyle estiloPeriodosVendido = new DataGridViewCellStyle();
            DataGridViewCellStyle estiloPeriodosCosto = new DataGridViewCellStyle();
            DataGridViewCellStyle estiloPeriodosGanancia = new DataGridViewCellStyle();
            DataGridViewCellStyle estiloPeriodosPorcentaje = new DataGridViewCellStyle();
            DataGridViewCellStyle estiloProductosUnidades = new DataGridViewCellStyle();
            DataGridViewCellStyle estiloProductosProductoVendido = new DataGridViewCellStyle();
            DataGridViewCellStyle estiloProductosProductoGanancia = new DataGridViewCellStyle();
            DataGridViewCellStyle estiloProductosProductoPorcentaje = new DataGridViewCellStyle();
            panelColumnas = new TableLayoutPanel();
            panelIzquierda = new Panel();
            panelDerecha = new Panel();
            panelTituloProductos = new FlowLayoutPanel();
            lblTituloProductos = new Label();
            cmbOrden = new ComboBox();
            panelResumen = new FlowLayoutPanel();
            panelFiltros = new FlowLayoutPanel();
            grillaPeriodos = new DataGridView();
            colPeriodo = new DataGridViewTextBoxColumn();
            colVentas = new DataGridViewTextBoxColumn();
            colVendido = new DataGridViewTextBoxColumn();
            colCosto = new DataGridViewTextBoxColumn();
            colGanancia = new DataGridViewTextBoxColumn();
            colPorcentaje = new DataGridViewTextBoxColumn();
            grillaProductos = new DataGridView();
            colProducto = new DataGridViewTextBoxColumn();
            colUnidades = new DataGridViewTextBoxColumn();
            colProductoVendido = new DataGridViewTextBoxColumn();
            colProductoGanancia = new DataGridViewTextBoxColumn();
            colProductoPorcentaje = new DataGridViewTextBoxColumn();
            bloqueVendido = new FlowLayoutPanel();
            lblTituloVendido = new Label();
            lblVendido = new Label();
            bloqueCosto = new FlowLayoutPanel();
            lblTituloCosto = new Label();
            lblCosto = new Label();
            bloqueGanancia = new FlowLayoutPanel();
            lblTituloGanancia = new Label();
            lblGanancia = new Label();
            bloquePorcentaje = new FlowLayoutPanel();
            lblTituloPorcentaje = new Label();
            lblPorcentaje = new Label();
            bloqueVentas = new FlowLayoutPanel();
            lblTituloVentas = new Label();
            lblVentas = new Label();
            lblPeriodo = new Label();
            cmbPeriodo = new ComboBox();
            lblDesde = new Label();
            dtpDesde = new DateTimePicker();
            lblHasta = new Label();
            dtpHasta = new DateTimePicker();
            lblAgrupar = new Label();
            cmbAgrupar = new ComboBox();
            btnExportar = new Button();
            lblEstado = new Label();
            panelColumnas.SuspendLayout();
            panelIzquierda.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)grillaPeriodos).BeginInit();
            panelDerecha.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)grillaProductos).BeginInit();
            panelTituloProductos.SuspendLayout();
            panelResumen.SuspendLayout();
            bloqueVendido.SuspendLayout();
            bloqueCosto.SuspendLayout();
            bloqueGanancia.SuspendLayout();
            bloquePorcentaje.SuspendLayout();
            bloqueVentas.SuspendLayout();
            panelFiltros.SuspendLayout();
            SuspendLayout();
            // 
            // panelColumnas
            // 
            // Izquierda: por día o por mes. Derecha: productos.
            panelColumnas.ColumnCount = 2;
            panelColumnas.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            panelColumnas.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            panelColumnas.Controls.Add(panelIzquierda, 0, 0);
            panelColumnas.Controls.Add(panelDerecha, 1, 0);
            panelColumnas.Dock = DockStyle.Fill;
            panelColumnas.Name = "panelColumnas";
            panelColumnas.RowCount = 1;
            panelColumnas.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            panelColumnas.TabIndex = 2;
            // 
            // panelIzquierda
            // 
            panelIzquierda.Controls.Add(grillaPeriodos);
            panelIzquierda.Dock = DockStyle.Fill;
            panelIzquierda.Name = "panelIzquierda";
            panelIzquierda.Padding = new Padding(6);
            panelIzquierda.TabIndex = 0;
            // 
            // grillaPeriodos
            // 
            grillaPeriodos.AllowUserToAddRows = false;
            grillaPeriodos.AllowUserToDeleteRows = false;
            grillaPeriodos.AllowUserToResizeRows = false;
            grillaPeriodos.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            grillaPeriodos.Columns.AddRange(new DataGridViewColumn[] { colPeriodo, colVentas, colVendido, colCosto, colGanancia, colPorcentaje });
            grillaPeriodos.Dock = DockStyle.Fill;
            grillaPeriodos.MultiSelect = false;
            grillaPeriodos.Name = "grillaPeriodos";
            grillaPeriodos.ReadOnly = true;
            grillaPeriodos.RowHeadersVisible = false;
            grillaPeriodos.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            grillaPeriodos.TabIndex = 0;
            grillaPeriodos.CellFormatting += GrillaPeriodos_CellFormatting;
            // 
            // colPeriodo
            // 
            colPeriodo.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            colPeriodo.DataPropertyName = "Etiqueta";
            colPeriodo.HeaderText = "Período";
            colPeriodo.Name = "colPeriodo";
            colPeriodo.ReadOnly = true;
            // 
            // colVentas
            // 
            colVentas.DataPropertyName = "CantidadVentas";
            estiloPeriodosVentas.Alignment = DataGridViewContentAlignment.MiddleRight;
            estiloPeriodosVentas.Format = "N0";
            colVentas.DefaultCellStyle = estiloPeriodosVentas;
            colVentas.HeaderText = "Ventas";
            colVentas.Name = "colVentas";
            colVentas.ReadOnly = true;
            colVentas.Width = 80;
            // 
            // colVendido
            // 
            colVendido.DataPropertyName = "Vendido";
            estiloPeriodosVendido.Alignment = DataGridViewContentAlignment.MiddleRight;
            estiloPeriodosVendido.Format = "C2";
            colVendido.DefaultCellStyle = estiloPeriodosVendido;
            colVendido.HeaderText = "Vendido";
            colVendido.Name = "colVendido";
            colVendido.ReadOnly = true;
            colVendido.Width = 130;
            // 
            // colCosto
            // 
            colCosto.DataPropertyName = "Costo";
            estiloPeriodosCosto.Alignment = DataGridViewContentAlignment.MiddleRight;
            estiloPeriodosCosto.Format = "C2";
            colCosto.DefaultCellStyle = estiloPeriodosCosto;
            colCosto.HeaderText = "Costo";
            colCosto.Name = "colCosto";
            colCosto.ReadOnly = true;
            colCosto.Width = 130;
            // 
            // colGanancia
            // 
            colGanancia.DataPropertyName = "Ganancia";
            estiloPeriodosGanancia.Alignment = DataGridViewContentAlignment.MiddleRight;
            estiloPeriodosGanancia.Format = "C2";
            colGanancia.DefaultCellStyle = estiloPeriodosGanancia;
            colGanancia.HeaderText = "Ganancia";
            colGanancia.Name = "colGanancia";
            colGanancia.ReadOnly = true;
            colGanancia.Width = 130;
            // 
            // colPorcentaje
            // 
            colPorcentaje.DataPropertyName = "PorcentajeSobreCosto";
            estiloPeriodosPorcentaje.Alignment = DataGridViewContentAlignment.MiddleRight;
            estiloPeriodosPorcentaje.Format = "P0";
            colPorcentaje.DefaultCellStyle = estiloPeriodosPorcentaje;
            colPorcentaje.HeaderText = "%";
            colPorcentaje.Name = "colPorcentaje";
            colPorcentaje.ReadOnly = true;
            colPorcentaje.Width = 70;
            // 
            // panelDerecha
            // 
            panelDerecha.Controls.Add(grillaProductos);
            panelDerecha.Controls.Add(panelTituloProductos);
            panelDerecha.Dock = DockStyle.Fill;
            panelDerecha.Name = "panelDerecha";
            panelDerecha.Padding = new Padding(6);
            panelDerecha.TabIndex = 1;
            // 
            // grillaProductos
            // 
            grillaProductos.AllowUserToAddRows = false;
            grillaProductos.AllowUserToDeleteRows = false;
            grillaProductos.AllowUserToResizeRows = false;
            grillaProductos.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            grillaProductos.Columns.AddRange(new DataGridViewColumn[] { colProducto, colUnidades, colProductoVendido, colProductoGanancia, colProductoPorcentaje });
            grillaProductos.Dock = DockStyle.Fill;
            grillaProductos.MultiSelect = false;
            grillaProductos.Name = "grillaProductos";
            grillaProductos.ReadOnly = true;
            grillaProductos.RowHeadersVisible = false;
            grillaProductos.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            grillaProductos.TabIndex = 1;
            grillaProductos.CellFormatting += GrillaProductos_CellFormatting;
            // 
            // colProducto
            // 
            colProducto.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            colProducto.DataPropertyName = "Nombre";
            colProducto.HeaderText = "Producto";
            colProducto.Name = "colProducto";
            colProducto.ReadOnly = true;
            // 
            // colUnidades
            // 
            colUnidades.DataPropertyName = "Unidades";
            estiloProductosUnidades.Alignment = DataGridViewContentAlignment.MiddleRight;
            estiloProductosUnidades.Format = "N0";
            colUnidades.DefaultCellStyle = estiloProductosUnidades;
            colUnidades.HeaderText = "Unidades";
            colUnidades.Name = "colUnidades";
            colUnidades.ReadOnly = true;
            colUnidades.Width = 90;
            // 
            // colProductoVendido
            // 
            colProductoVendido.DataPropertyName = "Vendido";
            estiloProductosProductoVendido.Alignment = DataGridViewContentAlignment.MiddleRight;
            estiloProductosProductoVendido.Format = "C2";
            colProductoVendido.DefaultCellStyle = estiloProductosProductoVendido;
            colProductoVendido.HeaderText = "Vendido";
            colProductoVendido.Name = "colProductoVendido";
            colProductoVendido.ReadOnly = true;
            colProductoVendido.Width = 120;
            // 
            // colProductoGanancia
            // 
            colProductoGanancia.DataPropertyName = "Ganancia";
            estiloProductosProductoGanancia.Alignment = DataGridViewContentAlignment.MiddleRight;
            estiloProductosProductoGanancia.Format = "C2";
            colProductoGanancia.DefaultCellStyle = estiloProductosProductoGanancia;
            colProductoGanancia.HeaderText = "Ganancia";
            colProductoGanancia.Name = "colProductoGanancia";
            colProductoGanancia.ReadOnly = true;
            colProductoGanancia.Width = 120;
            // 
            // colProductoPorcentaje
            // 
            colProductoPorcentaje.DataPropertyName = "PorcentajeSobreCosto";
            estiloProductosProductoPorcentaje.Alignment = DataGridViewContentAlignment.MiddleRight;
            estiloProductosProductoPorcentaje.Format = "P0";
            colProductoPorcentaje.DefaultCellStyle = estiloProductosProductoPorcentaje;
            colProductoPorcentaje.HeaderText = "%";
            colProductoPorcentaje.Name = "colProductoPorcentaje";
            colProductoPorcentaje.ReadOnly = true;
            colProductoPorcentaje.Width = 70;
            // 
            // panelTituloProductos
            // 
            panelTituloProductos.AutoSize = true;
            panelTituloProductos.Controls.Add(lblTituloProductos);
            panelTituloProductos.Controls.Add(cmbOrden);
            panelTituloProductos.Dock = DockStyle.Top;
            panelTituloProductos.Name = "panelTituloProductos";
            panelTituloProductos.TabIndex = 0;
            // 
            // lblTituloProductos
            // 
            lblTituloProductos.Anchor = AnchorStyles.Left;
            lblTituloProductos.AutoSize = true;
            lblTituloProductos.Margin = new Padding(3, 8, 12, 8);
            lblTituloProductos.Name = "lblTituloProductos";
            lblTituloProductos.TabIndex = 0;
            lblTituloProductos.Text = "Productos más vendidos, ordenados por";
            // 
            // cmbOrden
            // 
            cmbOrden.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbOrden.Margin = new Padding(3, 4, 3, 3);
            cmbOrden.Name = "cmbOrden";
            cmbOrden.Size = new Size(150, 28);
            cmbOrden.TabIndex = 1;
            cmbOrden.SelectedIndexChanged += CmbOrden_SelectedIndexChanged;
            // 
            // panelResumen
            // 
            // Resumen del período.
            panelResumen.AutoSize = true;
            panelResumen.Controls.Add(bloqueVendido);
            panelResumen.Controls.Add(bloqueCosto);
            panelResumen.Controls.Add(bloqueGanancia);
            panelResumen.Controls.Add(bloquePorcentaje);
            panelResumen.Controls.Add(bloqueVentas);
            panelResumen.Dock = DockStyle.Top;
            panelResumen.Name = "panelResumen";
            panelResumen.Padding = new Padding(6, 0, 6, 6);
            panelResumen.TabIndex = 1;
            // 
            // bloqueVendido
            // 
            bloqueVendido.AutoSize = true;
            bloqueVendido.Controls.Add(lblTituloVendido);
            bloqueVendido.Controls.Add(lblVendido);
            bloqueVendido.FlowDirection = FlowDirection.TopDown;
            bloqueVendido.Margin = new Padding(12, 3, 36, 3);
            bloqueVendido.Name = "bloqueVendido";
            bloqueVendido.TabIndex = 0;
            // 
            // lblTituloVendido
            // 
            lblTituloVendido.AutoSize = true;
            lblTituloVendido.ForeColor = SystemColors.GrayText;
            lblTituloVendido.Name = "lblTituloVendido";
            lblTituloVendido.TabIndex = 0;
            lblTituloVendido.Text = "Vendido";
            // 
            // lblVendido
            // 
            lblVendido.AutoSize = true;
            lblVendido.Font = new Font("Segoe UI", 16F);
            lblVendido.Name = "lblVendido";
            lblVendido.TabIndex = 1;
            lblVendido.Text = "-";
            // 
            // bloqueCosto
            // 
            bloqueCosto.AutoSize = true;
            bloqueCosto.Controls.Add(lblTituloCosto);
            bloqueCosto.Controls.Add(lblCosto);
            bloqueCosto.FlowDirection = FlowDirection.TopDown;
            bloqueCosto.Margin = new Padding(12, 3, 36, 3);
            bloqueCosto.Name = "bloqueCosto";
            bloqueCosto.TabIndex = 1;
            // 
            // lblTituloCosto
            // 
            lblTituloCosto.AutoSize = true;
            lblTituloCosto.ForeColor = SystemColors.GrayText;
            lblTituloCosto.Name = "lblTituloCosto";
            lblTituloCosto.TabIndex = 0;
            lblTituloCosto.Text = "Costo";
            // 
            // lblCosto
            // 
            lblCosto.AutoSize = true;
            lblCosto.Font = new Font("Segoe UI", 16F);
            lblCosto.Name = "lblCosto";
            lblCosto.TabIndex = 1;
            lblCosto.Text = "-";
            // 
            // bloqueGanancia
            // 
            bloqueGanancia.AutoSize = true;
            bloqueGanancia.Controls.Add(lblTituloGanancia);
            bloqueGanancia.Controls.Add(lblGanancia);
            bloqueGanancia.FlowDirection = FlowDirection.TopDown;
            bloqueGanancia.Margin = new Padding(12, 3, 36, 3);
            bloqueGanancia.Name = "bloqueGanancia";
            bloqueGanancia.TabIndex = 2;
            // 
            // lblTituloGanancia
            // 
            lblTituloGanancia.AutoSize = true;
            lblTituloGanancia.ForeColor = SystemColors.GrayText;
            lblTituloGanancia.Name = "lblTituloGanancia";
            lblTituloGanancia.TabIndex = 0;
            lblTituloGanancia.Text = "Ganancia";
            // 
            // lblGanancia
            // 
            lblGanancia.AutoSize = true;
            lblGanancia.Font = new Font("Segoe UI", 16F);
            lblGanancia.Name = "lblGanancia";
            lblGanancia.TabIndex = 1;
            lblGanancia.Text = "-";
            // 
            // bloquePorcentaje
            // 
            bloquePorcentaje.AutoSize = true;
            bloquePorcentaje.Controls.Add(lblTituloPorcentaje);
            bloquePorcentaje.Controls.Add(lblPorcentaje);
            bloquePorcentaje.FlowDirection = FlowDirection.TopDown;
            bloquePorcentaje.Margin = new Padding(12, 3, 36, 3);
            bloquePorcentaje.Name = "bloquePorcentaje";
            bloquePorcentaje.TabIndex = 3;
            // 
            // lblTituloPorcentaje
            // 
            lblTituloPorcentaje.AutoSize = true;
            lblTituloPorcentaje.ForeColor = SystemColors.GrayText;
            lblTituloPorcentaje.Name = "lblTituloPorcentaje";
            lblTituloPorcentaje.TabIndex = 0;
            lblTituloPorcentaje.Text = "% sobre costo";
            // 
            // lblPorcentaje
            // 
            lblPorcentaje.AutoSize = true;
            lblPorcentaje.Font = new Font("Segoe UI", 16F);
            lblPorcentaje.Name = "lblPorcentaje";
            lblPorcentaje.TabIndex = 1;
            lblPorcentaje.Text = "-";
            // 
            // bloqueVentas
            // 
            bloqueVentas.AutoSize = true;
            bloqueVentas.Controls.Add(lblTituloVentas);
            bloqueVentas.Controls.Add(lblVentas);
            bloqueVentas.FlowDirection = FlowDirection.TopDown;
            bloqueVentas.Margin = new Padding(12, 3, 36, 3);
            bloqueVentas.Name = "bloqueVentas";
            bloqueVentas.TabIndex = 4;
            // 
            // lblTituloVentas
            // 
            lblTituloVentas.AutoSize = true;
            lblTituloVentas.ForeColor = SystemColors.GrayText;
            lblTituloVentas.Name = "lblTituloVentas";
            lblTituloVentas.TabIndex = 0;
            lblTituloVentas.Text = "Ventas";
            // 
            // lblVentas
            // 
            lblVentas.AutoSize = true;
            lblVentas.Font = new Font("Segoe UI", 16F);
            lblVentas.Name = "lblVentas";
            lblVentas.TabIndex = 1;
            lblVentas.Text = "-";
            // 
            // panelFiltros
            // 
            panelFiltros.AutoSize = true;
            panelFiltros.Controls.Add(lblPeriodo);
            panelFiltros.Controls.Add(cmbPeriodo);
            panelFiltros.Controls.Add(lblDesde);
            panelFiltros.Controls.Add(dtpDesde);
            panelFiltros.Controls.Add(lblHasta);
            panelFiltros.Controls.Add(dtpHasta);
            panelFiltros.Controls.Add(lblAgrupar);
            panelFiltros.Controls.Add(cmbAgrupar);
            panelFiltros.Controls.Add(btnExportar);
            panelFiltros.Controls.Add(lblEstado);
            panelFiltros.Dock = DockStyle.Top;
            panelFiltros.Name = "panelFiltros";
            panelFiltros.Padding = new Padding(6);
            panelFiltros.TabIndex = 0;
            // 
            // lblPeriodo
            // 
            lblPeriodo.AutoSize = true;
            lblPeriodo.Margin = new Padding(12, 11, 3, 3);
            lblPeriodo.Name = "lblPeriodo";
            lblPeriodo.TabIndex = 0;
            lblPeriodo.Text = "Período:";
            // 
            // cmbPeriodo
            // 
            cmbPeriodo.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbPeriodo.Margin = new Padding(3, 8, 3, 3);
            cmbPeriodo.Name = "cmbPeriodo";
            cmbPeriodo.Size = new Size(170, 28);
            cmbPeriodo.TabIndex = 1;
            cmbPeriodo.SelectedIndexChanged += CmbPeriodo_SelectedIndexChanged;
            // 
            // lblDesde
            // 
            lblDesde.AutoSize = true;
            lblDesde.Margin = new Padding(12, 11, 3, 3);
            lblDesde.Name = "lblDesde";
            lblDesde.TabIndex = 2;
            lblDesde.Text = "Desde:";
            // 
            // dtpDesde
            // 
            dtpDesde.Format = DateTimePickerFormat.Short;
            dtpDesde.Margin = new Padding(3, 8, 3, 3);
            dtpDesde.Name = "dtpDesde";
            dtpDesde.Size = new Size(140, 27);
            dtpDesde.TabIndex = 3;
            dtpDesde.ValueChanged += Fecha_ValueChanged;
            // 
            // lblHasta
            // 
            lblHasta.AutoSize = true;
            lblHasta.Margin = new Padding(12, 11, 3, 3);
            lblHasta.Name = "lblHasta";
            lblHasta.TabIndex = 4;
            lblHasta.Text = "Hasta:";
            // 
            // dtpHasta
            // 
            dtpHasta.Format = DateTimePickerFormat.Short;
            dtpHasta.Margin = new Padding(3, 8, 3, 3);
            dtpHasta.Name = "dtpHasta";
            dtpHasta.Size = new Size(140, 27);
            dtpHasta.TabIndex = 5;
            dtpHasta.ValueChanged += Fecha_ValueChanged;
            // 
            // lblAgrupar
            // 
            lblAgrupar.AutoSize = true;
            lblAgrupar.Margin = new Padding(12, 11, 3, 3);
            lblAgrupar.Name = "lblAgrupar";
            lblAgrupar.TabIndex = 6;
            lblAgrupar.Text = "Agrupar:";
            // 
            // cmbAgrupar
            // 
            cmbAgrupar.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbAgrupar.Margin = new Padding(3, 8, 3, 3);
            cmbAgrupar.Name = "cmbAgrupar";
            cmbAgrupar.Size = new Size(130, 28);
            cmbAgrupar.TabIndex = 7;
            cmbAgrupar.SelectedIndexChanged += CmbAgrupar_SelectedIndexChanged;
            // 
            // btnExportar
            // 
            btnExportar.Margin = new Padding(4);
            btnExportar.Name = "btnExportar";
            btnExportar.Size = new Size(160, 42);
            btnExportar.TabIndex = 8;
            btnExportar.Text = "Exportar PDF…";
            btnExportar.UseVisualStyleBackColor = true;
            btnExportar.Click += BtnExportar_Click;
            // 
            // lblEstado
            // 
            lblEstado.AutoSize = true;
            lblEstado.Margin = new Padding(12, 14, 3, 3);
            lblEstado.MaximumSize = new Size(600, 0);
            lblEstado.Name = "lblEstado";
            lblEstado.TabIndex = 9;
            //
            // VistaReportes
            //
            // El orden importa: lo que tiene Dock = Fill (las dos columnas) va primero.
            Controls.Add(panelColumnas);
            Controls.Add(panelResumen);
            Controls.Add(panelFiltros);
            Font = new Font("Segoe UI", 11F);
            Name = "VistaReportes";
            Size = new Size(1400, 700);
            panelColumnas.ResumeLayout(false);
            panelColumnas.PerformLayout();
            panelIzquierda.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)grillaPeriodos).EndInit();
            panelDerecha.ResumeLayout(false);
            panelDerecha.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)grillaProductos).EndInit();
            panelTituloProductos.ResumeLayout(false);
            panelTituloProductos.PerformLayout();
            panelResumen.ResumeLayout(false);
            panelResumen.PerformLayout();
            bloqueVendido.ResumeLayout(false);
            bloqueVendido.PerformLayout();
            bloqueCosto.ResumeLayout(false);
            bloqueCosto.PerformLayout();
            bloqueGanancia.ResumeLayout(false);
            bloqueGanancia.PerformLayout();
            bloquePorcentaje.ResumeLayout(false);
            bloquePorcentaje.PerformLayout();
            bloqueVentas.ResumeLayout(false);
            bloqueVentas.PerformLayout();
            panelFiltros.ResumeLayout(false);
            panelFiltros.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private TableLayoutPanel panelColumnas;
        private Panel panelIzquierda;
        private Panel panelDerecha;
        private FlowLayoutPanel panelTituloProductos;
        private Label lblTituloProductos;
        private ComboBox cmbOrden;
        private FlowLayoutPanel panelResumen;
        private FlowLayoutPanel panelFiltros;
        private DataGridView grillaPeriodos;
        private DataGridViewTextBoxColumn colPeriodo;
        private DataGridViewTextBoxColumn colVentas;
        private DataGridViewTextBoxColumn colVendido;
        private DataGridViewTextBoxColumn colCosto;
        private DataGridViewTextBoxColumn colGanancia;
        private DataGridViewTextBoxColumn colPorcentaje;
        private DataGridView grillaProductos;
        private DataGridViewTextBoxColumn colProducto;
        private DataGridViewTextBoxColumn colUnidades;
        private DataGridViewTextBoxColumn colProductoVendido;
        private DataGridViewTextBoxColumn colProductoGanancia;
        private DataGridViewTextBoxColumn colProductoPorcentaje;
        private FlowLayoutPanel bloqueVendido;
        private Label lblTituloVendido;
        private Label lblVendido;
        private FlowLayoutPanel bloqueCosto;
        private Label lblTituloCosto;
        private Label lblCosto;
        private FlowLayoutPanel bloqueGanancia;
        private Label lblTituloGanancia;
        private Label lblGanancia;
        private FlowLayoutPanel bloquePorcentaje;
        private Label lblTituloPorcentaje;
        private Label lblPorcentaje;
        private FlowLayoutPanel bloqueVentas;
        private Label lblTituloVentas;
        private Label lblVentas;
        private Label lblPeriodo;
        private ComboBox cmbPeriodo;
        private Label lblDesde;
        private DateTimePicker dtpDesde;
        private Label lblHasta;
        private DateTimePicker dtpHasta;
        private Label lblAgrupar;
        private ComboBox cmbAgrupar;
        private Button btnExportar;
        private Label lblEstado;
    }
}

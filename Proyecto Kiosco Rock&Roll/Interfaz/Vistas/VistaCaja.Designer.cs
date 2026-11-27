namespace Proyecto_Kiosco_Rock_Roll.Interfaz.Vistas
{
    partial class VistaCaja
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
                panelAbrir.Dispose();
                panelMovimiento.Dispose();
                panelCerrar.Dispose();
                panelDetalle.Dispose();
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
            DataGridViewCellStyle estiloHora = new DataGridViewCellStyle();
            DataGridViewCellStyle estiloMonto = new DataGridViewCellStyle();
            DataGridViewCellStyle estiloApertura = new DataGridViewCellStyle();
            DataGridViewCellStyle estiloCierre = new DataGridViewCellStyle();
            DataGridViewCellStyle estiloVentas = new DataGridViewCellStyle();
            DataGridViewCellStyle estiloTotalVendido = new DataGridViewCellStyle();
            DataGridViewCellStyle estiloEsperado = new DataGridViewCellStyle();
            DataGridViewCellStyle estiloContado = new DataGridViewCellStyle();
            panelPrincipal = new Panel();
            pestanias = new TabControl();
            tabTurno = new TabPage();
            panelMovimientos = new Panel();
            grillaMovimientos = new DataGridView();
            colHora = new DataGridViewTextBoxColumn();
            colTipo = new DataGridViewTextBoxColumn();
            colMonto = new DataGridViewTextBoxColumn();
            colMotivo = new DataGridViewTextBoxColumn();
            lblTituloMovimientos = new Label();
            columnaCuentas = new Panel();
            cuentas = new TableLayoutPanel();
            lblTituloResumen = new Label();
            lblConceptoInicial = new Label();
            lblInicial = new Label();
            lblConceptoVentasEfectivo = new Label();
            lblVentasEfectivo = new Label();
            lblConceptoIngresos = new Label();
            lblIngresos = new Label();
            lblConceptoRetiros = new Label();
            lblRetiros = new Label();
            lblConceptoEsperado = new Label();
            lblEsperado = new Label();
            lblConceptoContado = new Label();
            lblContado = new Label();
            lblConceptoDiferencia = new Label();
            lblDiferencia = new Label();
            lblConceptoOtrosMedios = new Label();
            lblOtrosMedios = new Label();
            lblConceptoTotalVendido = new Label();
            lblTotalVendido = new Label();
            tabAnteriores = new TabPage();
            grillaTurnos = new DataGridView();
            colApertura = new DataGridViewTextBoxColumn();
            colCierre = new DataGridViewTextBoxColumn();
            colVentas = new DataGridViewTextBoxColumn();
            colTotalVendido = new DataGridViewTextBoxColumn();
            colEsperado = new DataGridViewTextBoxColumn();
            colContado = new DataGridViewTextBoxColumn();
            colDiferencia = new DataGridViewTextBoxColumn();
            tabVentas = new TabPage();
            panelArriba = new FlowLayoutPanel();
            lblEstado = new Label();
            btnAbrir = new Button();
            btnIngreso = new Button();
            btnRetiro = new Button();
            btnCerrar = new Button();
            lblAviso = new Label();
            panelPrincipal.SuspendLayout();
            pestanias.SuspendLayout();
            tabTurno.SuspendLayout();
            panelMovimientos.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)grillaMovimientos).BeginInit();
            columnaCuentas.SuspendLayout();
            cuentas.SuspendLayout();
            tabAnteriores.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)grillaTurnos).BeginInit();
            panelArriba.SuspendLayout();
            SuspendLayout();
            //
            // panelPrincipal
            //
            panelPrincipal.Controls.Add(pestanias);
            panelPrincipal.Controls.Add(panelArriba);
            panelPrincipal.Dock = DockStyle.Fill;
            panelPrincipal.Name = "panelPrincipal";
            panelPrincipal.TabIndex = 0;
            //
            // pestanias
            //
            pestanias.Controls.Add(tabTurno);
            pestanias.Controls.Add(tabAnteriores);
            pestanias.Controls.Add(tabVentas);
            pestanias.Dock = DockStyle.Fill;
            pestanias.Name = "pestanias";
            pestanias.Padding = new Point(16, 6);
            pestanias.SelectedIndex = 0;
            pestanias.TabIndex = 1;
            pestanias.SelectedIndexChanged += Pestanias_SelectedIndexChanged;
            //
            // tabTurno
            //
            // Las cuentas a la izquierda, los movimientos a la derecha.
            tabTurno.Controls.Add(panelMovimientos);
            tabTurno.Controls.Add(columnaCuentas);
            tabTurno.Name = "tabTurno";
            tabTurno.TabIndex = 0;
            tabTurno.Text = "Turno actual";
            tabTurno.UseVisualStyleBackColor = true;
            //
            // panelMovimientos
            //
            panelMovimientos.Controls.Add(grillaMovimientos);
            panelMovimientos.Controls.Add(lblTituloMovimientos);
            panelMovimientos.Dock = DockStyle.Fill;
            panelMovimientos.Name = "panelMovimientos";
            panelMovimientos.Padding = new Padding(12);
            panelMovimientos.TabIndex = 1;
            //
            // grillaMovimientos
            //
            grillaMovimientos.AllowUserToAddRows = false;
            grillaMovimientos.AllowUserToDeleteRows = false;
            grillaMovimientos.AllowUserToResizeRows = false;
            grillaMovimientos.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            grillaMovimientos.Columns.AddRange(new DataGridViewColumn[] { colHora, colTipo, colMonto, colMotivo });
            grillaMovimientos.Dock = DockStyle.Fill;
            grillaMovimientos.MultiSelect = false;
            grillaMovimientos.Name = "grillaMovimientos";
            grillaMovimientos.ReadOnly = true;
            grillaMovimientos.RowHeadersVisible = false;
            grillaMovimientos.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            grillaMovimientos.TabIndex = 1;
            // 
            // colHora
            // 
            estiloHora.Alignment = DataGridViewContentAlignment.MiddleRight;
            estiloHora.Format = "HH:mm";
            colHora.DataPropertyName = "Fecha";
            colHora.DefaultCellStyle = estiloHora;
            colHora.HeaderText = "Hora";
            colHora.Name = "colHora";
            colHora.ReadOnly = true;
            colHora.Width = 80;
            // 
            // colTipo
            // 
            colTipo.DataPropertyName = "TipoDescripcion";
            colTipo.HeaderText = "Tipo";
            colTipo.Name = "colTipo";
            colTipo.ReadOnly = true;
            colTipo.Width = 100;
            // 
            // colMonto
            // 
            estiloMonto.Alignment = DataGridViewContentAlignment.MiddleRight;
            estiloMonto.Format = "C2";
            colMonto.DataPropertyName = "Monto";
            colMonto.DefaultCellStyle = estiloMonto;
            colMonto.HeaderText = "Monto";
            colMonto.Name = "colMonto";
            colMonto.ReadOnly = true;
            colMonto.Width = 130;
            // 
            // colMotivo
            // 
            colMotivo.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            colMotivo.DataPropertyName = "Motivo";
            colMotivo.HeaderText = "Motivo";
            colMotivo.Name = "colMotivo";
            colMotivo.ReadOnly = true;
            //
            // lblTituloMovimientos
            //
            lblTituloMovimientos.Dock = DockStyle.Top;
            lblTituloMovimientos.Name = "lblTituloMovimientos";
            lblTituloMovimientos.Size = new Size(800, 30);
            lblTituloMovimientos.TabIndex = 0;
            lblTituloMovimientos.Text = "Ingresos y retiros de efectivo del turno";
            //
            // columnaCuentas
            //
            // La tabla va arriba dentro de una columna fija: si la tabla misma ocupara todo el alto,
            // la última fila se quedaría con el espacio sobrante y su texto aparecería muy abajo.
            columnaCuentas.Controls.Add(cuentas);
            columnaCuentas.Dock = DockStyle.Left;
            columnaCuentas.Name = "columnaCuentas";
            columnaCuentas.Size = new Size(460, 600);
            columnaCuentas.TabIndex = 0;
            //
            // cuentas
            //
            cuentas.AutoSize = true;
            cuentas.ColumnCount = 2;
            cuentas.ColumnStyles.Add(new ColumnStyle());
            cuentas.ColumnStyles.Add(new ColumnStyle());
            cuentas.Controls.Add(lblTituloResumen, 0, 0);
            cuentas.Controls.Add(lblConceptoInicial, 0, 1);
            cuentas.Controls.Add(lblInicial, 1, 1);
            cuentas.Controls.Add(lblConceptoVentasEfectivo, 0, 2);
            cuentas.Controls.Add(lblVentasEfectivo, 1, 2);
            cuentas.Controls.Add(lblConceptoIngresos, 0, 3);
            cuentas.Controls.Add(lblIngresos, 1, 3);
            cuentas.Controls.Add(lblConceptoRetiros, 0, 4);
            cuentas.Controls.Add(lblRetiros, 1, 4);
            cuentas.Controls.Add(lblConceptoEsperado, 0, 5);
            cuentas.Controls.Add(lblEsperado, 1, 5);
            cuentas.Controls.Add(lblConceptoContado, 0, 6);
            cuentas.Controls.Add(lblContado, 1, 6);
            cuentas.Controls.Add(lblConceptoDiferencia, 0, 7);
            cuentas.Controls.Add(lblDiferencia, 1, 7);
            cuentas.Controls.Add(lblConceptoOtrosMedios, 0, 8);
            cuentas.Controls.Add(lblOtrosMedios, 1, 8);
            cuentas.Controls.Add(lblConceptoTotalVendido, 0, 9);
            cuentas.Controls.Add(lblTotalVendido, 1, 9);
            cuentas.Dock = DockStyle.Top;
            cuentas.Name = "cuentas";
            cuentas.Padding = new Padding(12);
            cuentas.RowCount = 10;
            cuentas.RowStyles.Add(new RowStyle());
            cuentas.RowStyles.Add(new RowStyle());
            cuentas.RowStyles.Add(new RowStyle());
            cuentas.RowStyles.Add(new RowStyle());
            cuentas.RowStyles.Add(new RowStyle());
            cuentas.RowStyles.Add(new RowStyle());
            cuentas.RowStyles.Add(new RowStyle());
            cuentas.RowStyles.Add(new RowStyle());
            cuentas.RowStyles.Add(new RowStyle());
            cuentas.RowStyles.Add(new RowStyle());
            cuentas.TabIndex = 0;
            //
            // lblTituloResumen
            //
            lblTituloResumen.Anchor = AnchorStyles.Left;
            lblTituloResumen.AutoSize = true;
            cuentas.SetColumnSpan(lblTituloResumen, 2);
            lblTituloResumen.Font = new Font("Segoe UI", 15F, FontStyle.Bold);
            lblTituloResumen.Margin = new Padding(3, 8, 12, 8);
            lblTituloResumen.MaximumSize = new Size(420, 0);
            lblTituloResumen.Name = "lblTituloResumen";
            lblTituloResumen.TabIndex = 0;
            lblTituloResumen.Text = "Turno actual";
            // 
            // lblConceptoInicial
            // 
            lblConceptoInicial.Anchor = AnchorStyles.Left;
            lblConceptoInicial.AutoSize = true;
            lblConceptoInicial.Margin = new Padding(3, 8, 12, 8);
            lblConceptoInicial.Name = "lblConceptoInicial";
            lblConceptoInicial.TabIndex = 1;
            lblConceptoInicial.Text = "Efectivo inicial";
            // 
            // lblInicial
            // 
            lblInicial.Anchor = AnchorStyles.Right;
            lblInicial.AutoSize = true;
            lblInicial.Margin = new Padding(24, 6, 3, 6);
            lblInicial.Name = "lblInicial";
            lblInicial.TabIndex = 2;
            lblInicial.Text = "-";
            // 
            // lblConceptoVentasEfectivo
            // 
            lblConceptoVentasEfectivo.Anchor = AnchorStyles.Left;
            lblConceptoVentasEfectivo.AutoSize = true;
            lblConceptoVentasEfectivo.Margin = new Padding(3, 8, 12, 8);
            lblConceptoVentasEfectivo.Name = "lblConceptoVentasEfectivo";
            lblConceptoVentasEfectivo.TabIndex = 3;
            lblConceptoVentasEfectivo.Text = "+ Ventas en efectivo";
            // 
            // lblVentasEfectivo
            // 
            lblVentasEfectivo.Anchor = AnchorStyles.Right;
            lblVentasEfectivo.AutoSize = true;
            lblVentasEfectivo.Margin = new Padding(24, 6, 3, 6);
            lblVentasEfectivo.Name = "lblVentasEfectivo";
            lblVentasEfectivo.TabIndex = 4;
            lblVentasEfectivo.Text = "-";
            // 
            // lblConceptoIngresos
            // 
            lblConceptoIngresos.Anchor = AnchorStyles.Left;
            lblConceptoIngresos.AutoSize = true;
            lblConceptoIngresos.Margin = new Padding(3, 8, 12, 8);
            lblConceptoIngresos.Name = "lblConceptoIngresos";
            lblConceptoIngresos.TabIndex = 5;
            lblConceptoIngresos.Text = "+ Ingresos de efectivo";
            // 
            // lblIngresos
            // 
            lblIngresos.Anchor = AnchorStyles.Right;
            lblIngresos.AutoSize = true;
            lblIngresos.Margin = new Padding(24, 6, 3, 6);
            lblIngresos.Name = "lblIngresos";
            lblIngresos.TabIndex = 6;
            lblIngresos.Text = "-";
            // 
            // lblConceptoRetiros
            // 
            lblConceptoRetiros.Anchor = AnchorStyles.Left;
            lblConceptoRetiros.AutoSize = true;
            lblConceptoRetiros.Margin = new Padding(3, 8, 12, 8);
            lblConceptoRetiros.Name = "lblConceptoRetiros";
            lblConceptoRetiros.TabIndex = 7;
            lblConceptoRetiros.Text = "− Retiros de efectivo";
            // 
            // lblRetiros
            // 
            lblRetiros.Anchor = AnchorStyles.Right;
            lblRetiros.AutoSize = true;
            lblRetiros.Margin = new Padding(24, 6, 3, 6);
            lblRetiros.Name = "lblRetiros";
            lblRetiros.TabIndex = 8;
            lblRetiros.Text = "-";
            // 
            // lblConceptoEsperado
            // 
            lblConceptoEsperado.Anchor = AnchorStyles.Left;
            lblConceptoEsperado.AutoSize = true;
            lblConceptoEsperado.Font = new Font("Segoe UI", 16F);
            lblConceptoEsperado.Margin = new Padding(3, 8, 12, 8);
            lblConceptoEsperado.Name = "lblConceptoEsperado";
            lblConceptoEsperado.TabIndex = 9;
            lblConceptoEsperado.Text = "= Efectivo esperado";
            // 
            // lblEsperado
            // 
            lblEsperado.Anchor = AnchorStyles.Right;
            lblEsperado.AutoSize = true;
            lblEsperado.Font = new Font("Segoe UI", 16F);
            lblEsperado.Margin = new Padding(24, 6, 3, 6);
            lblEsperado.Name = "lblEsperado";
            lblEsperado.TabIndex = 10;
            lblEsperado.Text = "-";
            // 
            // lblConceptoContado
            // 
            lblConceptoContado.Anchor = AnchorStyles.Left;
            lblConceptoContado.AutoSize = true;
            lblConceptoContado.Font = new Font("Segoe UI", 16F);
            lblConceptoContado.Margin = new Padding(3, 8, 12, 8);
            lblConceptoContado.Name = "lblConceptoContado";
            lblConceptoContado.TabIndex = 11;
            lblConceptoContado.Text = "Efectivo contado";
            // 
            // lblContado
            // 
            lblContado.Anchor = AnchorStyles.Right;
            lblContado.AutoSize = true;
            lblContado.Font = new Font("Segoe UI", 16F);
            lblContado.Margin = new Padding(24, 6, 3, 6);
            lblContado.Name = "lblContado";
            lblContado.TabIndex = 12;
            lblContado.Text = "-";
            // 
            // lblConceptoDiferencia
            // 
            lblConceptoDiferencia.Anchor = AnchorStyles.Left;
            lblConceptoDiferencia.AutoSize = true;
            lblConceptoDiferencia.Font = new Font("Segoe UI", 16F);
            lblConceptoDiferencia.Margin = new Padding(3, 8, 12, 8);
            lblConceptoDiferencia.Name = "lblConceptoDiferencia";
            lblConceptoDiferencia.TabIndex = 13;
            lblConceptoDiferencia.Text = "Diferencia";
            // 
            // lblDiferencia
            // 
            lblDiferencia.Anchor = AnchorStyles.Right;
            lblDiferencia.AutoSize = true;
            lblDiferencia.Font = new Font("Segoe UI", 16F);
            lblDiferencia.Margin = new Padding(24, 6, 3, 6);
            lblDiferencia.Name = "lblDiferencia";
            lblDiferencia.TabIndex = 14;
            lblDiferencia.Text = "-";
            // 
            // lblConceptoOtrosMedios
            // 
            lblConceptoOtrosMedios.Anchor = AnchorStyles.Left;
            lblConceptoOtrosMedios.AutoSize = true;
            lblConceptoOtrosMedios.Margin = new Padding(3, 8, 12, 8);
            lblConceptoOtrosMedios.Name = "lblConceptoOtrosMedios";
            lblConceptoOtrosMedios.TabIndex = 15;
            lblConceptoOtrosMedios.Text = "Otros medios de pago";
            // 
            // lblOtrosMedios
            // 
            lblOtrosMedios.Anchor = AnchorStyles.Right;
            lblOtrosMedios.AutoSize = true;
            lblOtrosMedios.Margin = new Padding(24, 6, 3, 6);
            lblOtrosMedios.Name = "lblOtrosMedios";
            lblOtrosMedios.TabIndex = 16;
            lblOtrosMedios.Text = "-";
            // 
            // lblConceptoTotalVendido
            // 
            lblConceptoTotalVendido.Anchor = AnchorStyles.Left;
            lblConceptoTotalVendido.AutoSize = true;
            lblConceptoTotalVendido.Margin = new Padding(3, 8, 12, 8);
            lblConceptoTotalVendido.Name = "lblConceptoTotalVendido";
            lblConceptoTotalVendido.TabIndex = 17;
            lblConceptoTotalVendido.Text = "Total vendido";
            // 
            // lblTotalVendido
            // 
            lblTotalVendido.Anchor = AnchorStyles.Right;
            lblTotalVendido.AutoSize = true;
            lblTotalVendido.Margin = new Padding(24, 6, 3, 6);
            lblTotalVendido.Name = "lblTotalVendido";
            lblTotalVendido.TabIndex = 18;
            lblTotalVendido.Text = "-";
            //
            // tabAnteriores
            //
            tabAnteriores.Controls.Add(grillaTurnos);
            tabAnteriores.Name = "tabAnteriores";
            tabAnteriores.TabIndex = 1;
            tabAnteriores.Text = "Turnos anteriores";
            tabAnteriores.UseVisualStyleBackColor = true;
            //
            // grillaTurnos
            //
            grillaTurnos.AllowUserToAddRows = false;
            grillaTurnos.AllowUserToDeleteRows = false;
            grillaTurnos.AllowUserToResizeRows = false;
            grillaTurnos.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            grillaTurnos.Columns.AddRange(new DataGridViewColumn[] { colApertura, colCierre, colVentas, colTotalVendido, colEsperado, colContado, colDiferencia });
            grillaTurnos.Dock = DockStyle.Fill;
            grillaTurnos.MultiSelect = false;
            grillaTurnos.Name = "grillaTurnos";
            grillaTurnos.ReadOnly = true;
            grillaTurnos.RowHeadersVisible = false;
            grillaTurnos.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            grillaTurnos.TabIndex = 0;
            grillaTurnos.CellFormatting += FormatearDiferencia;
            // 
            // colApertura
            // 
            estiloApertura.Alignment = DataGridViewContentAlignment.MiddleRight;
            estiloApertura.Format = "dd/MM/yyyy HH:mm";
            colApertura.DataPropertyName = "Apertura";
            colApertura.DefaultCellStyle = estiloApertura;
            colApertura.HeaderText = "Apertura";
            colApertura.Name = "colApertura";
            colApertura.ReadOnly = true;
            colApertura.Width = 170;
            // 
            // colCierre
            // 
            estiloCierre.Alignment = DataGridViewContentAlignment.MiddleRight;
            estiloCierre.Format = "dd/MM HH:mm";
            colCierre.DataPropertyName = "Cierre";
            colCierre.DefaultCellStyle = estiloCierre;
            colCierre.HeaderText = "Cierre";
            colCierre.Name = "colCierre";
            colCierre.ReadOnly = true;
            colCierre.Width = 130;
            // 
            // colVentas
            // 
            estiloVentas.Alignment = DataGridViewContentAlignment.MiddleRight;
            estiloVentas.Format = "N0";
            colVentas.DataPropertyName = "CantidadVentas";
            colVentas.DefaultCellStyle = estiloVentas;
            colVentas.HeaderText = "Ventas";
            colVentas.Name = "colVentas";
            colVentas.ReadOnly = true;
            colVentas.Width = 80;
            // 
            // colTotalVendido
            // 
            estiloTotalVendido.Alignment = DataGridViewContentAlignment.MiddleRight;
            estiloTotalVendido.Format = "C2";
            colTotalVendido.DataPropertyName = "TotalVendido";
            colTotalVendido.DefaultCellStyle = estiloTotalVendido;
            colTotalVendido.HeaderText = "Total vendido";
            colTotalVendido.Name = "colTotalVendido";
            colTotalVendido.ReadOnly = true;
            colTotalVendido.Width = 150;
            // 
            // colEsperado
            // 
            estiloEsperado.Alignment = DataGridViewContentAlignment.MiddleRight;
            estiloEsperado.Format = "C2";
            colEsperado.DataPropertyName = "EfectivoEsperado";
            colEsperado.DefaultCellStyle = estiloEsperado;
            colEsperado.HeaderText = "Efectivo esperado";
            colEsperado.Name = "colEsperado";
            colEsperado.ReadOnly = true;
            colEsperado.Width = 170;
            // 
            // colContado
            // 
            estiloContado.Alignment = DataGridViewContentAlignment.MiddleRight;
            estiloContado.Format = "C2";
            colContado.DataPropertyName = "EfectivoContado";
            colContado.DefaultCellStyle = estiloContado;
            colContado.HeaderText = "Contado";
            colContado.Name = "colContado";
            colContado.ReadOnly = true;
            colContado.Width = 140;
            // 
            // colDiferencia
            // 
            colDiferencia.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            colDiferencia.DataPropertyName = "Diferencia";
            colDiferencia.HeaderText = "Diferencia";
            colDiferencia.Name = "colDiferencia";
            colDiferencia.ReadOnly = true;
            //
            // tabVentas
            //
            // El listado de ventas por día lo arma el programa y se agrega al abrir la vista.
            tabVentas.Name = "tabVentas";
            tabVentas.TabIndex = 2;
            tabVentas.Text = "Ventas por día";
            tabVentas.UseVisualStyleBackColor = true;
            //
            // panelArriba
            //
            panelArriba.AutoSize = true;
            panelArriba.Controls.Add(lblEstado);
            panelArriba.Controls.Add(btnAbrir);
            panelArriba.Controls.Add(btnIngreso);
            panelArriba.Controls.Add(btnRetiro);
            panelArriba.Controls.Add(btnCerrar);
            panelArriba.Controls.Add(lblAviso);
            panelArriba.Dock = DockStyle.Top;
            panelArriba.Name = "panelArriba";
            panelArriba.Padding = new Padding(6);
            panelArriba.TabIndex = 0;
            //
            // lblEstado
            //
            lblEstado.AutoSize = true;
            lblEstado.Font = new Font("Segoe UI", 15F, FontStyle.Bold);
            lblEstado.Margin = new Padding(6, 12, 24, 3);
            lblEstado.Name = "lblEstado";
            lblEstado.TabIndex = 0;
            lblEstado.Text = "Caja cerrada";
            //
            // btnAbrir
            //
            btnAbrir.Margin = new Padding(4);
            btnAbrir.Name = "btnAbrir";
            btnAbrir.Size = new Size(150, 42);
            btnAbrir.TabIndex = 1;
            btnAbrir.Text = "Abrir caja";
            btnAbrir.UseVisualStyleBackColor = true;
            btnAbrir.Click += BtnAbrir_Click;
            //
            // btnIngreso
            //
            btnIngreso.Margin = new Padding(4);
            btnIngreso.Name = "btnIngreso";
            btnIngreso.Size = new Size(190, 42);
            btnIngreso.TabIndex = 2;
            btnIngreso.Text = "Ingreso de efectivo";
            btnIngreso.UseVisualStyleBackColor = true;
            btnIngreso.Click += BtnIngreso_Click;
            //
            // btnRetiro
            //
            btnRetiro.Margin = new Padding(4);
            btnRetiro.Name = "btnRetiro";
            btnRetiro.Size = new Size(190, 42);
            btnRetiro.TabIndex = 3;
            btnRetiro.Text = "Retiro de efectivo";
            btnRetiro.UseVisualStyleBackColor = true;
            btnRetiro.Click += BtnRetiro_Click;
            //
            // btnCerrar
            //
            btnCerrar.Margin = new Padding(4);
            btnCerrar.Name = "btnCerrar";
            btnCerrar.Size = new Size(150, 42);
            btnCerrar.TabIndex = 4;
            btnCerrar.Text = "Cerrar caja";
            btnCerrar.UseVisualStyleBackColor = true;
            btnCerrar.Click += BtnCerrar_Click;
            //
            // lblAviso
            //
            lblAviso.AutoSize = true;
            lblAviso.Margin = new Padding(12, 16, 3, 3);
            lblAviso.MaximumSize = new Size(800, 0);
            lblAviso.Name = "lblAviso";
            lblAviso.TabIndex = 5;
            //
            // VistaCaja
            //
            Controls.Add(panelPrincipal);
            Font = new Font("Segoe UI", 11F);
            Name = "VistaCaja";
            Size = new Size(1400, 700);
            panelPrincipal.ResumeLayout(false);
            panelPrincipal.PerformLayout();
            pestanias.ResumeLayout(false);
            tabTurno.ResumeLayout(false);
            panelMovimientos.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)grillaMovimientos).EndInit();
            columnaCuentas.ResumeLayout(false);
            columnaCuentas.PerformLayout();
            cuentas.ResumeLayout(false);
            cuentas.PerformLayout();
            tabAnteriores.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)grillaTurnos).EndInit();
            panelArriba.ResumeLayout(false);
            panelArriba.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        private Panel panelPrincipal;
        private TabControl pestanias;
        private TabPage tabTurno;
        private Panel panelMovimientos;
        private DataGridView grillaMovimientos;
        private Label lblTituloMovimientos;
        private Panel columnaCuentas;
        private TableLayoutPanel cuentas;
        private Label lblTituloResumen;
        private TabPage tabAnteriores;
        private DataGridView grillaTurnos;
        private TabPage tabVentas;
        private FlowLayoutPanel panelArriba;
        private Label lblEstado;
        private Button btnAbrir;
        private Button btnIngreso;
        private Button btnRetiro;
        private Button btnCerrar;
        private Label lblAviso;
        private Label lblConceptoInicial;
        private Label lblInicial;
        private Label lblConceptoVentasEfectivo;
        private Label lblVentasEfectivo;
        private Label lblConceptoIngresos;
        private Label lblIngresos;
        private Label lblConceptoRetiros;
        private Label lblRetiros;
        private Label lblConceptoEsperado;
        private Label lblEsperado;
        private Label lblConceptoContado;
        private Label lblContado;
        private Label lblConceptoDiferencia;
        private Label lblDiferencia;
        private Label lblConceptoOtrosMedios;
        private Label lblOtrosMedios;
        private Label lblConceptoTotalVendido;
        private Label lblTotalVendido;
        private DataGridViewTextBoxColumn colHora;
        private DataGridViewTextBoxColumn colTipo;
        private DataGridViewTextBoxColumn colMonto;
        private DataGridViewTextBoxColumn colMotivo;
        private DataGridViewTextBoxColumn colApertura;
        private DataGridViewTextBoxColumn colCierre;
        private DataGridViewTextBoxColumn colVentas;
        private DataGridViewTextBoxColumn colTotalVendido;
        private DataGridViewTextBoxColumn colEsperado;
        private DataGridViewTextBoxColumn colContado;
        private DataGridViewTextBoxColumn colDiferencia;
    }
}

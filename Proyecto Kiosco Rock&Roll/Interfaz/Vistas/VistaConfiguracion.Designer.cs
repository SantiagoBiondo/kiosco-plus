namespace Proyecto_Kiosco_Rock_Roll.Interfaz.Vistas
{
    partial class VistaConfiguracion
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
            panelImpresora = new TableLayoutPanel();
            lblTituloImpresora = new Label();
            lblAyudaImpresora = new Label();
            lblImpresora = new Label();
            filaImpresora = new FlowLayoutPanel();
            cmbImpresora = new ComboBox();
            btnActualizarLista = new Button();
            lblPapel = new Label();
            cmbPapel = new ComboBox();
            chkImprimirSiempre = new CheckBox();
            btnImprimirPrueba = new Button();
            lblEstado = new Label();
            lblTituloApariencia = new Label();
            lblTemaTitulo = new Label();
            cmbTema = new ComboBox();
            lblAvisoTema = new Label();
            btnReiniciar = new Button();
            panelVistaPrevia = new TableLayoutPanel();
            lblTituloTicket = new Label();
            txtVistaPrevia = new TextBox();
            panelCobro = new TableLayoutPanel();
            lblTituloCobro = new Label();
            lblAyudaCobro = new Label();
            lblRubrosConRecargo = new Label();
            lstRecargos = new ListBox();
            lblAgregarRubro = new Label();
            filaAgregarRubro = new FlowLayoutPanel();
            cmbRubroRecargo = new ComboBox();
            lblMas = new Label();
            numRecargo = new NumericUpDown();
            lblPorPaquete = new Label();
            filaBotonesRecargo = new FlowLayoutPanel();
            btnAgregarRecargo = new Button();
            btnQuitarRecargo = new Button();
            lblMinimo = new Label();
            numMinimo = new NumericUpDown();
            lblCobro = new Label();
            panelImpresora.SuspendLayout();
            filaImpresora.SuspendLayout();
            panelVistaPrevia.SuspendLayout();
            panelCobro.SuspendLayout();
            filaAgregarRubro.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)numRecargo).BeginInit();
            filaBotonesRecargo.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)numMinimo).BeginInit();
            SuspendLayout();
            // 
            // panelImpresora
            // 
            panelImpresora.AutoSize = true;
            panelImpresora.ColumnCount = 1;
            panelImpresora.ColumnStyles.Add(new ColumnStyle());
            panelImpresora.Controls.Add(lblTituloImpresora, 0, 0);
            panelImpresora.Controls.Add(lblAyudaImpresora, 0, 1);
            panelImpresora.Controls.Add(lblImpresora, 0, 2);
            panelImpresora.Controls.Add(filaImpresora, 0, 3);
            panelImpresora.Controls.Add(lblPapel, 0, 4);
            panelImpresora.Controls.Add(cmbPapel, 0, 5);
            panelImpresora.Controls.Add(chkImprimirSiempre, 0, 6);
            panelImpresora.Controls.Add(btnImprimirPrueba, 0, 7);
            panelImpresora.Controls.Add(lblEstado, 0, 8);
            panelImpresora.Controls.Add(lblTituloApariencia, 0, 9);
            panelImpresora.Controls.Add(lblTemaTitulo, 0, 10);
            panelImpresora.Controls.Add(cmbTema, 0, 11);
            panelImpresora.Controls.Add(lblAvisoTema, 0, 12);
            panelImpresora.Controls.Add(btnReiniciar, 0, 13);
            panelImpresora.Dock = DockStyle.Left;
            panelImpresora.Location = new Point(0, 0);
            panelImpresora.Name = "panelImpresora";
            panelImpresora.Padding = new Padding(16);
            panelImpresora.RowCount = 14;
            panelImpresora.RowStyles.Add(new RowStyle());
            panelImpresora.RowStyles.Add(new RowStyle());
            panelImpresora.RowStyles.Add(new RowStyle());
            panelImpresora.RowStyles.Add(new RowStyle());
            panelImpresora.RowStyles.Add(new RowStyle());
            panelImpresora.RowStyles.Add(new RowStyle());
            panelImpresora.RowStyles.Add(new RowStyle());
            panelImpresora.RowStyles.Add(new RowStyle());
            panelImpresora.RowStyles.Add(new RowStyle());
            panelImpresora.RowStyles.Add(new RowStyle());
            panelImpresora.RowStyles.Add(new RowStyle());
            panelImpresora.RowStyles.Add(new RowStyle());
            panelImpresora.RowStyles.Add(new RowStyle());
            panelImpresora.RowStyles.Add(new RowStyle());
            panelImpresora.Size = new Size(645, 719);
            panelImpresora.TabIndex = 0;
            // 
            // lblTituloImpresora
            // 
            lblTituloImpresora.Anchor = AnchorStyles.Left;
            lblTituloImpresora.AutoSize = true;
            lblTituloImpresora.Font = new Font("Segoe UI", 15F, FontStyle.Bold);
            lblTituloImpresora.Location = new Point(19, 24);
            lblTituloImpresora.Margin = new Padding(3, 8, 12, 8);
            lblTituloImpresora.Name = "lblTituloImpresora";
            lblTituloImpresora.Size = new Size(205, 28);
            lblTituloImpresora.TabIndex = 0;
            lblTituloImpresora.Text = "Impresora de tickets";
            // 
            // lblAyudaImpresora
            // 
            lblAyudaImpresora.AutoSize = true;
            lblAyudaImpresora.Location = new Point(19, 60);
            lblAyudaImpresora.MaximumSize = new Size(620, 0);
            lblAyudaImpresora.Name = "lblAyudaImpresora";
            lblAyudaImpresora.Size = new Size(607, 40);
            lblAyudaImpresora.TabIndex = 1;
            lblAyudaImpresora.Text = "Elegí por qué impresora salen los tickets. Sirve cualquier impresora instalada en Windows: una de tickets (térmica) o una común. Los cambios se guardan solos.";
            // 
            // lblImpresora
            // 
            lblImpresora.Anchor = AnchorStyles.Left;
            lblImpresora.AutoSize = true;
            lblImpresora.Location = new Point(19, 108);
            lblImpresora.Margin = new Padding(3, 8, 12, 8);
            lblImpresora.Name = "lblImpresora";
            lblImpresora.Size = new Size(76, 20);
            lblImpresora.TabIndex = 2;
            lblImpresora.Text = "Impresora";
            // 
            // filaImpresora
            // 
            filaImpresora.AutoSize = true;
            filaImpresora.Controls.Add(cmbImpresora);
            filaImpresora.Controls.Add(btnActualizarLista);
            filaImpresora.Location = new Point(19, 139);
            filaImpresora.Name = "filaImpresora";
            filaImpresora.Size = new Size(594, 50);
            filaImpresora.TabIndex = 3;
            // 
            // cmbImpresora
            // 
            cmbImpresora.Anchor = AnchorStyles.Left;
            cmbImpresora.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbImpresora.FormattingEnabled = true;
            cmbImpresora.Location = new Point(3, 13);
            cmbImpresora.Name = "cmbImpresora";
            cmbImpresora.Size = new Size(420, 28);
            cmbImpresora.TabIndex = 0;
            cmbImpresora.SelectedIndexChanged += CmbImpresora_SelectedIndexChanged;
            // 
            // btnActualizarLista
            // 
            btnActualizarLista.Location = new Point(430, 4);
            btnActualizarLista.Margin = new Padding(4);
            btnActualizarLista.Name = "btnActualizarLista";
            btnActualizarLista.Size = new Size(160, 42);
            btnActualizarLista.TabIndex = 1;
            btnActualizarLista.Text = "Actualizar lista";
            btnActualizarLista.UseVisualStyleBackColor = true;
            btnActualizarLista.Click += BtnActualizarLista_Click;
            // 
            // lblPapel
            // 
            lblPapel.Anchor = AnchorStyles.Left;
            lblPapel.AutoSize = true;
            lblPapel.Location = new Point(19, 200);
            lblPapel.Margin = new Padding(3, 8, 12, 8);
            lblPapel.Name = "lblPapel";
            lblPapel.Size = new Size(118, 20);
            lblPapel.TabIndex = 4;
            lblPapel.Text = "Ancho del papel";
            // 
            // cmbPapel
            // 
            cmbPapel.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbPapel.FormattingEnabled = true;
            cmbPapel.Location = new Point(19, 231);
            cmbPapel.Name = "cmbPapel";
            cmbPapel.Size = new Size(200, 28);
            cmbPapel.TabIndex = 5;
            cmbPapel.SelectedIndexChanged += CmbPapel_SelectedIndexChanged;
            // 
            // chkImprimirSiempre
            // 
            chkImprimirSiempre.AutoSize = true;
            chkImprimirSiempre.Location = new Point(19, 269);
            chkImprimirSiempre.Margin = new Padding(3, 12, 3, 3);
            chkImprimirSiempre.Name = "chkImprimirSiempre";
            chkImprimirSiempre.Size = new Size(601, 24);
            chkImprimirSiempre.TabIndex = 6;
            chkImprimirSiempre.Text = "Imprimir el ticket siempre al cobrar (si no, se marca la casilla cuando el cliente lo pide)";
            chkImprimirSiempre.UseVisualStyleBackColor = true;
            chkImprimirSiempre.CheckedChanged += ChkImprimirSiempre_CheckedChanged;
            // 
            // btnImprimirPrueba
            // 
            btnImprimirPrueba.Location = new Point(19, 312);
            btnImprimirPrueba.Margin = new Padding(3, 16, 3, 3);
            btnImprimirPrueba.Name = "btnImprimirPrueba";
            btnImprimirPrueba.Size = new Size(240, 42);
            btnImprimirPrueba.TabIndex = 7;
            btnImprimirPrueba.Text = "Imprimir ticket de prueba";
            btnImprimirPrueba.UseVisualStyleBackColor = true;
            btnImprimirPrueba.Click += BtnImprimirPrueba_Click;
            // 
            // lblEstado
            // 
            lblEstado.AutoSize = true;
            lblEstado.Location = new Point(19, 369);
            lblEstado.Margin = new Padding(3, 12, 3, 3);
            lblEstado.MaximumSize = new Size(700, 0);
            lblEstado.Name = "lblEstado";
            lblEstado.Size = new Size(0, 20);
            lblEstado.TabIndex = 8;
            // 
            // lblTituloApariencia
            // 
            lblTituloApariencia.Anchor = AnchorStyles.Left;
            lblTituloApariencia.AutoSize = true;
            lblTituloApariencia.Font = new Font("Segoe UI", 15F, FontStyle.Bold);
            lblTituloApariencia.Location = new Point(19, 424);
            lblTituloApariencia.Margin = new Padding(3, 32, 3, 8);
            lblTituloApariencia.Name = "lblTituloApariencia";
            lblTituloApariencia.Size = new Size(113, 28);
            lblTituloApariencia.TabIndex = 9;
            lblTituloApariencia.Text = "Apariencia";
            // 
            // lblTemaTitulo
            // 
            lblTemaTitulo.Anchor = AnchorStyles.Left;
            lblTemaTitulo.AutoSize = true;
            lblTemaTitulo.Location = new Point(19, 468);
            lblTemaTitulo.Margin = new Padding(3, 8, 12, 8);
            lblTemaTitulo.Name = "lblTemaTitulo";
            lblTemaTitulo.Size = new Size(45, 20);
            lblTemaTitulo.TabIndex = 10;
            lblTemaTitulo.Text = "Tema";
            // 
            // cmbTema
            // 
            cmbTema.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbTema.FormattingEnabled = true;
            cmbTema.Location = new Point(19, 499);
            cmbTema.Name = "cmbTema";
            cmbTema.Size = new Size(200, 28);
            cmbTema.TabIndex = 11;
            cmbTema.SelectedIndexChanged += CmbTema_SelectedIndexChanged;
            // 
            // lblAvisoTema
            // 
            lblAvisoTema.AutoSize = true;
            lblAvisoTema.Location = new Point(19, 533);
            lblAvisoTema.Margin = new Padding(3, 8, 3, 3);
            lblAvisoTema.MaximumSize = new Size(620, 0);
            lblAvisoTema.Name = "lblAvisoTema";
            lblAvisoTema.Size = new Size(0, 20);
            lblAvisoTema.TabIndex = 12;
            // 
            // btnReiniciar
            // 
            btnReiniciar.Location = new Point(20, 560);
            btnReiniciar.Margin = new Padding(4);
            btnReiniciar.Name = "btnReiniciar";
            btnReiniciar.Size = new Size(180, 42);
            btnReiniciar.TabIndex = 13;
            btnReiniciar.Text = "Reiniciar ahora";
            btnReiniciar.UseVisualStyleBackColor = true;
            btnReiniciar.Click += BtnReiniciar_Click;
            // 
            // panelVistaPrevia
            // 
            panelVistaPrevia.AutoSize = true;
            panelVistaPrevia.ColumnCount = 1;
            panelVistaPrevia.ColumnStyles.Add(new ColumnStyle());
            panelVistaPrevia.Controls.Add(lblTituloTicket, 0, 0);
            panelVistaPrevia.Controls.Add(txtVistaPrevia, 0, 1);
            panelVistaPrevia.Dock = DockStyle.Left;
            panelVistaPrevia.Location = new Point(645, 0);
            panelVistaPrevia.Name = "panelVistaPrevia";
            panelVistaPrevia.Padding = new Padding(16);
            panelVistaPrevia.RowCount = 2;
            panelVistaPrevia.RowStyles.Add(new RowStyle());
            panelVistaPrevia.RowStyles.Add(new RowStyle());
            panelVistaPrevia.Size = new Size(558, 719);
            panelVistaPrevia.TabIndex = 1;
            // 
            // lblTituloTicket
            // 
            lblTituloTicket.Anchor = AnchorStyles.Left;
            lblTituloTicket.AutoSize = true;
            lblTituloTicket.Font = new Font("Segoe UI", 15F, FontStyle.Bold);
            lblTituloTicket.Location = new Point(19, 24);
            lblTituloTicket.Margin = new Padding(3, 8, 12, 8);
            lblTituloTicket.Name = "lblTituloTicket";
            lblTituloTicket.Size = new Size(167, 28);
            lblTituloTicket.TabIndex = 0;
            lblTituloTicket.Text = "Así sale el ticket";
            // 
            // txtVistaPrevia
            // 
            txtVistaPrevia.BackColor = Color.White;
            txtVistaPrevia.Font = new Font("Consolas", 11F);
            txtVistaPrevia.ForeColor = Color.Black;
            txtVistaPrevia.Location = new Point(19, 63);
            txtVistaPrevia.Multiline = true;
            txtVistaPrevia.Name = "txtVistaPrevia";
            txtVistaPrevia.ReadOnly = true;
            txtVistaPrevia.ScrollBars = ScrollBars.Vertical;
            txtVistaPrevia.Size = new Size(520, 480);
            txtVistaPrevia.TabIndex = 1;
            txtVistaPrevia.TabStop = false;
            // 
            // panelCobro
            // 
            panelCobro.AutoSize = true;
            panelCobro.ColumnCount = 1;
            panelCobro.ColumnStyles.Add(new ColumnStyle());
            panelCobro.Controls.Add(lblTituloCobro, 0, 0);
            panelCobro.Controls.Add(lblAyudaCobro, 0, 1);
            panelCobro.Controls.Add(lblRubrosConRecargo, 0, 2);
            panelCobro.Controls.Add(lstRecargos, 0, 3);
            panelCobro.Controls.Add(lblAgregarRubro, 0, 4);
            panelCobro.Controls.Add(filaAgregarRubro, 0, 5);
            panelCobro.Controls.Add(filaBotonesRecargo, 0, 6);
            panelCobro.Controls.Add(lblMinimo, 0, 7);
            panelCobro.Controls.Add(numMinimo, 0, 8);
            panelCobro.Controls.Add(lblCobro, 0, 9);
            panelCobro.Dock = DockStyle.Left;
            panelCobro.Location = new Point(1203, 0);
            panelCobro.Name = "panelCobro";
            panelCobro.Padding = new Padding(16);
            panelCobro.RowCount = 10;
            panelCobro.RowStyles.Add(new RowStyle());
            panelCobro.RowStyles.Add(new RowStyle());
            panelCobro.RowStyles.Add(new RowStyle());
            panelCobro.RowStyles.Add(new RowStyle());
            panelCobro.RowStyles.Add(new RowStyle());
            panelCobro.RowStyles.Add(new RowStyle());
            panelCobro.RowStyles.Add(new RowStyle());
            panelCobro.RowStyles.Add(new RowStyle());
            panelCobro.RowStyles.Add(new RowStyle());
            panelCobro.RowStyles.Add(new RowStyle());
            panelCobro.Size = new Size(498, 719);
            panelCobro.TabIndex = 2;
            // 
            // lblTituloCobro
            // 
            lblTituloCobro.Anchor = AnchorStyles.Left;
            lblTituloCobro.AutoSize = true;
            lblTituloCobro.Font = new Font("Segoe UI", 15F, FontStyle.Bold);
            lblTituloCobro.Location = new Point(19, 24);
            lblTituloCobro.Margin = new Padding(3, 8, 12, 8);
            lblTituloCobro.Name = "lblTituloCobro";
            lblTituloCobro.Size = new Size(237, 28);
            lblTituloCobro.TabIndex = 0;
            lblTituloCobro.Text = "Cobro por transferencia";
            // 
            // lblAyudaCobro
            // 
            lblAyudaCobro.AutoSize = true;
            lblAyudaCobro.Location = new Point(19, 60);
            lblAyudaCobro.MaximumSize = new Size(460, 0);
            lblAyudaCobro.Name = "lblAyudaCobro";
            lblAyudaCobro.Size = new Size(459, 60);
            lblAyudaCobro.TabIndex = 1;
            lblAyudaCobro.Text = "Si se paga algo por transferencia, cada paquete de estos rubros se cobra más caro (los sueltos no). Y no se puede transferir menos que el mínimo.";
            // 
            // lblRubrosConRecargo
            // 
            lblRubrosConRecargo.Anchor = AnchorStyles.Left;
            lblRubrosConRecargo.AutoSize = true;
            lblRubrosConRecargo.Location = new Point(19, 128);
            lblRubrosConRecargo.Margin = new Padding(3, 8, 12, 8);
            lblRubrosConRecargo.Name = "lblRubrosConRecargo";
            lblRubrosConRecargo.Size = new Size(138, 20);
            lblRubrosConRecargo.TabIndex = 2;
            lblRubrosConRecargo.Text = "Rubros con recargo";
            // 
            // lstRecargos
            // 
            lstRecargos.FormattingEnabled = true;
            lstRecargos.IntegralHeight = false;
            lstRecargos.Location = new Point(19, 159);
            lstRecargos.Name = "lstRecargos";
            lstRecargos.Size = new Size(460, 150);
            lstRecargos.TabIndex = 3;
            // 
            // lblAgregarRubro
            // 
            lblAgregarRubro.Anchor = AnchorStyles.Left;
            lblAgregarRubro.AutoSize = true;
            lblAgregarRubro.Location = new Point(19, 320);
            lblAgregarRubro.Margin = new Padding(3, 8, 12, 8);
            lblAgregarRubro.Name = "lblAgregarRubro";
            lblAgregarRubro.Size = new Size(280, 20);
            lblAgregarRubro.TabIndex = 4;
            lblAgregarRubro.Text = "Agregar un rubro (o cambiar su recargo):";
            // 
            // filaAgregarRubro
            // 
            filaAgregarRubro.AutoSize = true;
            filaAgregarRubro.Controls.Add(cmbRubroRecargo);
            filaAgregarRubro.Controls.Add(lblMas);
            filaAgregarRubro.Controls.Add(numRecargo);
            filaAgregarRubro.Controls.Add(lblPorPaquete);
            filaAgregarRubro.Location = new Point(19, 351);
            filaAgregarRubro.Name = "filaAgregarRubro";
            filaAgregarRubro.Size = new Size(449, 33);
            filaAgregarRubro.TabIndex = 5;
            // 
            // cmbRubroRecargo
            // 
            cmbRubroRecargo.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbRubroRecargo.FormattingEnabled = true;
            cmbRubroRecargo.Location = new Point(3, 3);
            cmbRubroRecargo.Name = "cmbRubroRecargo";
            cmbRubroRecargo.Size = new Size(180, 28);
            cmbRubroRecargo.TabIndex = 0;
            cmbRubroRecargo.SelectedIndexChanged += CmbRubroRecargo_SelectedIndexChanged;
            // 
            // lblMas
            // 
            lblMas.AutoSize = true;
            lblMas.Location = new Point(192, 8);
            lblMas.Margin = new Padding(6, 8, 0, 3);
            lblMas.Name = "lblMas";
            lblMas.Size = new Size(31, 20);
            lblMas.TabIndex = 1;
            lblMas.Text = "+ $";
            // 
            // numRecargo
            // 
            numRecargo.DecimalPlaces = 2;
            numRecargo.Location = new Point(226, 3);
            numRecargo.Maximum = new decimal(new int[] { 1000000, 0, 0, 0 });
            numRecargo.Name = "numRecargo";
            numRecargo.Size = new Size(120, 27);
            numRecargo.TabIndex = 2;
            numRecargo.TextAlign = HorizontalAlignment.Right;
            numRecargo.ThousandsSeparator = true;
            // 
            // lblPorPaquete
            // 
            lblPorPaquete.AutoSize = true;
            lblPorPaquete.Location = new Point(352, 8);
            lblPorPaquete.Margin = new Padding(3, 8, 6, 3);
            lblPorPaquete.Name = "lblPorPaquete";
            lblPorPaquete.Size = new Size(91, 20);
            lblPorPaquete.TabIndex = 3;
            lblPorPaquete.Text = "por paquete";
            // 
            // filaBotonesRecargo
            // 
            filaBotonesRecargo.AutoSize = true;
            filaBotonesRecargo.Controls.Add(btnAgregarRecargo);
            filaBotonesRecargo.Controls.Add(btnQuitarRecargo);
            filaBotonesRecargo.Location = new Point(19, 390);
            filaBotonesRecargo.Name = "filaBotonesRecargo";
            filaBotonesRecargo.Size = new Size(336, 50);
            filaBotonesRecargo.TabIndex = 6;
            // 
            // btnAgregarRecargo
            // 
            btnAgregarRecargo.Location = new Point(4, 4);
            btnAgregarRecargo.Margin = new Padding(4);
            btnAgregarRecargo.Name = "btnAgregarRecargo";
            btnAgregarRecargo.Size = new Size(140, 42);
            btnAgregarRecargo.TabIndex = 0;
            btnAgregarRecargo.Text = "Agregar";
            btnAgregarRecargo.UseVisualStyleBackColor = true;
            btnAgregarRecargo.Click += BtnAgregarRecargo_Click;
            // 
            // btnQuitarRecargo
            // 
            btnQuitarRecargo.Location = new Point(152, 4);
            btnQuitarRecargo.Margin = new Padding(4);
            btnQuitarRecargo.Name = "btnQuitarRecargo";
            btnQuitarRecargo.Size = new Size(180, 42);
            btnQuitarRecargo.TabIndex = 1;
            btnQuitarRecargo.Text = "Quitar el elegido";
            btnQuitarRecargo.UseVisualStyleBackColor = true;
            btnQuitarRecargo.Click += BtnQuitarRecargo_Click;
            // 
            // lblMinimo
            // 
            lblMinimo.Anchor = AnchorStyles.Left;
            lblMinimo.AutoSize = true;
            lblMinimo.Location = new Point(19, 451);
            lblMinimo.Margin = new Padding(3, 8, 12, 8);
            lblMinimo.Name = "lblMinimo";
            lblMinimo.Size = new Size(179, 20);
            lblMinimo.TabIndex = 7;
            lblMinimo.Text = "Mínimo para transferir ($)";
            // 
            // numMinimo
            // 
            numMinimo.DecimalPlaces = 2;
            numMinimo.Location = new Point(19, 482);
            numMinimo.Maximum = new decimal(new int[] { 1000000, 0, 0, 0 });
            numMinimo.Name = "numMinimo";
            numMinimo.Size = new Size(200, 27);
            numMinimo.TabIndex = 8;
            numMinimo.TextAlign = HorizontalAlignment.Right;
            numMinimo.ThousandsSeparator = true;
            numMinimo.ValueChanged += NumMinimo_ValueChanged;
            numMinimo.Leave += NumMinimo_ValueChanged;
            // 
            // lblCobro
            // 
            lblCobro.AutoSize = true;
            lblCobro.Location = new Point(19, 524);
            lblCobro.Margin = new Padding(3, 12, 3, 3);
            lblCobro.MaximumSize = new Size(460, 0);
            lblCobro.Name = "lblCobro";
            lblCobro.Size = new Size(0, 20);
            lblCobro.TabIndex = 9;
            // 
            // VistaConfiguracion
            // 
            Controls.Add(panelCobro);
            Controls.Add(panelVistaPrevia);
            Controls.Add(panelImpresora);
            Name = "VistaConfiguracion";
            Size = new Size(1177, 719);
            panelImpresora.ResumeLayout(false);
            panelImpresora.PerformLayout();
            filaImpresora.ResumeLayout(false);
            panelVistaPrevia.ResumeLayout(false);
            panelVistaPrevia.PerformLayout();
            panelCobro.ResumeLayout(false);
            panelCobro.PerformLayout();
            filaAgregarRubro.ResumeLayout(false);
            filaAgregarRubro.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)numRecargo).EndInit();
            filaBotonesRecargo.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)numMinimo).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private TableLayoutPanel panelImpresora;
        private Label lblTituloImpresora;
        private Label lblAyudaImpresora;
        private Label lblImpresora;
        private FlowLayoutPanel filaImpresora;
        private ComboBox cmbImpresora;
        private Button btnActualizarLista;
        private Label lblPapel;
        private ComboBox cmbPapel;
        private CheckBox chkImprimirSiempre;
        private Button btnImprimirPrueba;
        private Label lblEstado;
        private Label lblTituloApariencia;
        private Label lblTemaTitulo;
        private ComboBox cmbTema;
        private Label lblAvisoTema;
        private Button btnReiniciar;
        private TableLayoutPanel panelVistaPrevia;
        private Label lblTituloTicket;
        private TextBox txtVistaPrevia;
        private TableLayoutPanel panelCobro;
        private Label lblTituloCobro;
        private Label lblAyudaCobro;
        private Label lblRubrosConRecargo;
        private ListBox lstRecargos;
        private Label lblAgregarRubro;
        private FlowLayoutPanel filaAgregarRubro;
        private ComboBox cmbRubroRecargo;
        private Label lblMas;
        private NumericUpDown numRecargo;
        private Label lblPorPaquete;
        private FlowLayoutPanel filaBotonesRecargo;
        private Button btnAgregarRecargo;
        private Button btnQuitarRecargo;
        private Label lblMinimo;
        private NumericUpDown numMinimo;
        private Label lblCobro;
    }
}

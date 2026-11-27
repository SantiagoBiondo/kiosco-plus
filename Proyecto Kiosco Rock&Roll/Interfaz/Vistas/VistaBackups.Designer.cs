namespace Proyecto_Kiosco_Rock_Roll.Interfaz.Vistas
{
    partial class VistaBackups
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
            DataGridViewCellStyle estiloFecha = new DataGridViewCellStyle();
            lblExplicacion = new Label();
            panelCarpetas = new TableLayoutPanel();
            lblCarpetaPrincipalTitulo = new Label();
            lblCarpetaPrincipal = new Label();
            btnAbrirCarpeta = new Button();
            lblCarpetaExtraTitulo = new Label();
            lblCarpetaExtra = new Label();
            filaBotonesExtra = new FlowLayoutPanel();
            btnElegirCarpeta = new Button();
            btnQuitarExtra = new Button();
            grilla = new DataGridView();
            colNombre = new DataGridViewTextBoxColumn();
            colFecha = new DataGridViewTextBoxColumn();
            colTamanio = new DataGridViewTextBoxColumn();
            panelAbajo = new FlowLayoutPanel();
            btnHacerBackup = new Button();
            btnRestaurar = new Button();
            lblEstado = new Label();
            panelCarpetas.SuspendLayout();
            filaBotonesExtra.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)grilla).BeginInit();
            panelAbajo.SuspendLayout();
            SuspendLayout();
            //
            // lblExplicacion
            //
            lblExplicacion.AutoSize = true;
            lblExplicacion.Dock = DockStyle.Top;
            lblExplicacion.Name = "lblExplicacion";
            lblExplicacion.Padding = new Padding(8);
            lblExplicacion.TabIndex = 0;
            lblExplicacion.Text = "Cada vez que cerrás el programa se guarda una copia de seguridad del día.";
            //
            // panelCarpetas
            //
            panelCarpetas.AutoSize = true;
            panelCarpetas.ColumnCount = 3;
            panelCarpetas.ColumnStyles.Add(new ColumnStyle());
            panelCarpetas.ColumnStyles.Add(new ColumnStyle());
            panelCarpetas.ColumnStyles.Add(new ColumnStyle());
            panelCarpetas.Controls.Add(lblCarpetaPrincipalTitulo, 0, 0);
            panelCarpetas.Controls.Add(lblCarpetaPrincipal, 1, 0);
            panelCarpetas.Controls.Add(btnAbrirCarpeta, 2, 0);
            panelCarpetas.Controls.Add(lblCarpetaExtraTitulo, 0, 1);
            panelCarpetas.Controls.Add(lblCarpetaExtra, 1, 1);
            panelCarpetas.Controls.Add(filaBotonesExtra, 2, 1);
            panelCarpetas.Dock = DockStyle.Top;
            panelCarpetas.Name = "panelCarpetas";
            panelCarpetas.Padding = new Padding(6);
            panelCarpetas.RowCount = 2;
            panelCarpetas.RowStyles.Add(new RowStyle());
            panelCarpetas.RowStyles.Add(new RowStyle());
            panelCarpetas.TabIndex = 1;
            //
            // lblCarpetaPrincipalTitulo
            //
            lblCarpetaPrincipalTitulo.Anchor = AnchorStyles.Left;
            lblCarpetaPrincipalTitulo.AutoSize = true;
            lblCarpetaPrincipalTitulo.Margin = new Padding(3, 8, 12, 8);
            lblCarpetaPrincipalTitulo.Name = "lblCarpetaPrincipalTitulo";
            lblCarpetaPrincipalTitulo.TabIndex = 0;
            lblCarpetaPrincipalTitulo.Text = "Carpeta principal:";
            //
            // lblCarpetaPrincipal
            //
            lblCarpetaPrincipal.AutoSize = true;
            lblCarpetaPrincipal.Margin = new Padding(3, 10, 3, 3);
            lblCarpetaPrincipal.Name = "lblCarpetaPrincipal";
            lblCarpetaPrincipal.TabIndex = 1;
            //
            // btnAbrirCarpeta
            //
            btnAbrirCarpeta.Margin = new Padding(4);
            btnAbrirCarpeta.Name = "btnAbrirCarpeta";
            btnAbrirCarpeta.Size = new Size(150, 42);
            btnAbrirCarpeta.TabIndex = 2;
            btnAbrirCarpeta.Text = "Abrir carpeta";
            btnAbrirCarpeta.UseVisualStyleBackColor = true;
            btnAbrirCarpeta.Click += BtnAbrirCarpeta_Click;
            //
            // lblCarpetaExtraTitulo
            //
            lblCarpetaExtraTitulo.Anchor = AnchorStyles.Left;
            lblCarpetaExtraTitulo.AutoSize = true;
            lblCarpetaExtraTitulo.Margin = new Padding(3, 8, 12, 8);
            lblCarpetaExtraTitulo.Name = "lblCarpetaExtraTitulo";
            lblCarpetaExtraTitulo.TabIndex = 3;
            lblCarpetaExtraTitulo.Text = "Carpeta extra:";
            //
            // lblCarpetaExtra
            //
            lblCarpetaExtra.AutoSize = true;
            lblCarpetaExtra.Margin = new Padding(3, 10, 3, 3);
            lblCarpetaExtra.Name = "lblCarpetaExtra";
            lblCarpetaExtra.TabIndex = 4;
            //
            // filaBotonesExtra
            //
            filaBotonesExtra.AutoSize = true;
            filaBotonesExtra.Controls.Add(btnElegirCarpeta);
            filaBotonesExtra.Controls.Add(btnQuitarExtra);
            filaBotonesExtra.Name = "filaBotonesExtra";
            filaBotonesExtra.TabIndex = 5;
            //
            // btnElegirCarpeta
            //
            btnElegirCarpeta.Margin = new Padding(4);
            btnElegirCarpeta.Name = "btnElegirCarpeta";
            btnElegirCarpeta.Size = new Size(150, 42);
            btnElegirCarpeta.TabIndex = 0;
            btnElegirCarpeta.Text = "Elegir carpeta…";
            btnElegirCarpeta.UseVisualStyleBackColor = true;
            btnElegirCarpeta.Click += BtnElegirCarpeta_Click;
            //
            // btnQuitarExtra
            //
            btnQuitarExtra.Margin = new Padding(4);
            btnQuitarExtra.Name = "btnQuitarExtra";
            btnQuitarExtra.Size = new Size(100, 42);
            btnQuitarExtra.TabIndex = 1;
            btnQuitarExtra.Text = "Quitar";
            btnQuitarExtra.UseVisualStyleBackColor = true;
            btnQuitarExtra.Click += BtnQuitarExtra_Click;
            //
            // grilla
            //
            grilla.AllowUserToAddRows = false;
            grilla.AllowUserToDeleteRows = false;
            grilla.AllowUserToResizeRows = false;
            grilla.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            grilla.Columns.AddRange(new DataGridViewColumn[] { colNombre, colFecha, colTamanio });
            grilla.Dock = DockStyle.Fill;
            grilla.MultiSelect = false;
            grilla.Name = "grilla";
            grilla.ReadOnly = true;
            grilla.RowHeadersVisible = false;
            grilla.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            grilla.TabIndex = 2;
            //
            // colNombre
            //
            colNombre.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            colNombre.DataPropertyName = "Nombre";
            colNombre.HeaderText = "Backup";
            colNombre.Name = "colNombre";
            colNombre.ReadOnly = true;
            //
            // colFecha
            //
            colFecha.DataPropertyName = "Fecha";
            estiloFecha.Alignment = DataGridViewContentAlignment.MiddleRight;
            estiloFecha.Format = "dd/MM/yyyy HH:mm";
            colFecha.DefaultCellStyle = estiloFecha;
            colFecha.HeaderText = "Última actualización";
            colFecha.Name = "colFecha";
            colFecha.ReadOnly = true;
            colFecha.Width = 200;
            //
            // colTamanio
            //
            colTamanio.DataPropertyName = "Tamanio";
            colTamanio.HeaderText = "Tamaño";
            colTamanio.Name = "colTamanio";
            colTamanio.ReadOnly = true;
            colTamanio.Width = 110;
            //
            // panelAbajo
            //
            panelAbajo.AutoSize = true;
            panelAbajo.Controls.Add(btnHacerBackup);
            panelAbajo.Controls.Add(btnRestaurar);
            panelAbajo.Controls.Add(lblEstado);
            panelAbajo.Dock = DockStyle.Bottom;
            panelAbajo.Name = "panelAbajo";
            panelAbajo.Padding = new Padding(6);
            panelAbajo.TabIndex = 3;
            //
            // btnHacerBackup
            //
            btnHacerBackup.Margin = new Padding(4);
            btnHacerBackup.Name = "btnHacerBackup";
            btnHacerBackup.Size = new Size(200, 42);
            btnHacerBackup.TabIndex = 0;
            btnHacerBackup.Text = "Hacer backup ahora";
            btnHacerBackup.UseVisualStyleBackColor = true;
            btnHacerBackup.Click += BtnHacerBackup_Click;
            //
            // btnRestaurar
            //
            btnRestaurar.Margin = new Padding(4);
            btnRestaurar.Name = "btnRestaurar";
            btnRestaurar.Size = new Size(210, 42);
            btnRestaurar.TabIndex = 1;
            btnRestaurar.Text = "Restaurar el elegido…";
            btnRestaurar.UseVisualStyleBackColor = true;
            btnRestaurar.Click += BtnRestaurar_Click;
            //
            // lblEstado
            //
            lblEstado.AutoSize = true;
            lblEstado.Margin = new Padding(12, 14, 3, 3);
            lblEstado.MaximumSize = new Size(900, 0);
            lblEstado.Name = "lblEstado";
            lblEstado.TabIndex = 2;
            //
            // VistaBackups
            //
            // El orden importa: lo que tiene Dock = Fill (la grilla) va primero.
            Controls.Add(grilla);
            Controls.Add(panelAbajo);
            Controls.Add(panelCarpetas);
            Controls.Add(lblExplicacion);
            Font = new Font("Segoe UI", 11F);
            Name = "VistaBackups";
            Size = new Size(1400, 700);
            panelCarpetas.ResumeLayout(false);
            panelCarpetas.PerformLayout();
            filaBotonesExtra.ResumeLayout(false);
            filaBotonesExtra.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)grilla).EndInit();
            panelAbajo.ResumeLayout(false);
            panelAbajo.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Label lblExplicacion;
        private TableLayoutPanel panelCarpetas;
        private Label lblCarpetaPrincipalTitulo;
        private Label lblCarpetaPrincipal;
        private Button btnAbrirCarpeta;
        private Label lblCarpetaExtraTitulo;
        private Label lblCarpetaExtra;
        private FlowLayoutPanel filaBotonesExtra;
        private Button btnElegirCarpeta;
        private Button btnQuitarExtra;
        private DataGridView grilla;
        private DataGridViewTextBoxColumn colNombre;
        private DataGridViewTextBoxColumn colFecha;
        private DataGridViewTextBoxColumn colTamanio;
        private FlowLayoutPanel panelAbajo;
        private Button btnHacerBackup;
        private Button btnRestaurar;
        private Label lblEstado;
    }
}

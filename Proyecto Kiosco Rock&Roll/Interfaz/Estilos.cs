using System.Globalization;

namespace Proyecto_Kiosco_Rock_Roll.Interfaz
{
    /// <summary>Ayudas para que todas las pantallas se vean parecidas.</summary>
    public static class Estilos
    {
        public static readonly Font Normal = new("Segoe UI", 11F);
        public static readonly Font Grande = new("Segoe UI", 16F);
        public static readonly Font Titulo = new("Segoe UI", 15F, FontStyle.Bold);
        public static readonly Font Total = new("Segoe UI", 28F, FontStyle.Bold);
        public static readonly Font Tachada = new("Segoe UI", 11F, FontStyle.Strikeout);

        // ---------- Tema claro u oscuro ----------

        /// <summary>Si la interfaz usa el tema oscuro. Se decide una vez, al arrancar (ver <see cref="AplicarTema"/>).</summary>
        public static bool Oscuro { get; private set; }

        /// <summary>
        /// Elige el tema. Tiene que llamarse antes de crear cualquier ventana: Windows define
        /// los colores de los controles al crearlos, por eso cambiar de tema pide reiniciar.
        /// </summary>
        public static void AplicarTema(bool oscuro)
        {
            Oscuro = oscuro;
#pragma warning disable WFO5001 // El modo oscuro de WinForms todavía está marcado como experimental.
            Application.SetColorMode(oscuro ? SystemColorMode.Dark : SystemColorMode.Classic);
#pragma warning restore WFO5001
        }

        // Fondos.
        public static Color FondoStockBajo => Oscuro ? Color.FromArgb(92, 38, 42) : Color.FromArgb(255, 220, 220);
        public static Color FondoPanelLateral => Oscuro ? Color.FromArgb(45, 45, 48) : Color.FromArgb(243, 246, 250);
        public static Color FondoBarra => Oscuro ? Color.FromArgb(20, 20, 22) : Color.FromArgb(32, 36, 44);
        public static Color SeccionActiva => Color.FromArgb(220, 53, 69);

        // Botones destacados (siempre con letra blanca).
        public static Color FondoBotonPrincipal => Color.ForestGreen;
        public static Color FondoBotonPeligro => Color.Firebrick;

        // Colores de texto: en el tema oscuro, más claros para que se lean sobre fondo oscuro.
        public static Color Exito => Oscuro ? Color.FromArgb(110, 205, 110) : Color.ForestGreen;
        public static Color Advertencia => Color.DarkOrange;
        public static Color Error => Oscuro ? Color.FromArgb(255, 110, 110) : Color.Red;

        public static void ConfigurarGrilla(DataGridView grilla)
        {
            grilla.Dock = DockStyle.Fill;
            grilla.AutoGenerateColumns = false;
            grilla.AllowUserToAddRows = false;
            grilla.AllowUserToDeleteRows = false;
            grilla.AllowUserToResizeRows = false;
            grilla.MultiSelect = false;
            grilla.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            grilla.RowHeadersVisible = false;
            grilla.BackgroundColor = SystemColors.Window;
            grilla.Font = Normal;
            grilla.RowTemplate.Height = 32;
            grilla.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            ActivarDobleBuffer(grilla);

            if (Oscuro)
            {
                // La grilla no toma sola los colores oscuros: se los damos a mano.
                grilla.BackgroundColor = Color.FromArgb(30, 30, 30);
                grilla.GridColor = Color.FromArgb(62, 62, 66);
                grilla.EnableHeadersVisualStyles = false;
                grilla.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(45, 45, 48);
                grilla.ColumnHeadersDefaultCellStyle.ForeColor = Color.Gainsboro;
                grilla.ColumnHeadersDefaultCellStyle.SelectionBackColor = Color.FromArgb(45, 45, 48);
                grilla.DefaultCellStyle.BackColor = Color.FromArgb(37, 37, 38);
                grilla.DefaultCellStyle.ForeColor = Color.Gainsboro;
                grilla.DefaultCellStyle.SelectionBackColor = Color.FromArgb(0, 90, 158);
                grilla.DefaultCellStyle.SelectionForeColor = Color.White;
            }
        }

        /// <summary>
        /// La grilla se dibuja primero en memoria y después se muestra de una vez. Sin esto, cada línea
        /// de cada celda se dibuja directo en la pantalla y una lista llena tarda casi medio segundo en aparecer.
        /// DoubleBuffered es protegida en DataGridView, por eso se activa por reflexión.
        /// </summary>
        private static void ActivarDobleBuffer(DataGridView grilla) =>
            typeof(DataGridView)
                .GetProperty("DoubleBuffered", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)!
                .SetValue(grilla, true);

        public static DataGridViewTextBoxColumn Columna(
            DataGridView grilla, string propiedad, string titulo,
            string? formato = null, int ancho = 120, bool llenar = false, bool soloLectura = true)
        {
            var columna = new DataGridViewTextBoxColumn
            {
                DataPropertyName = propiedad,
                Name = propiedad,
                HeaderText = titulo,
                ReadOnly = soloLectura,
                Width = ancho,
                AutoSizeMode = llenar ? DataGridViewAutoSizeColumnMode.Fill : DataGridViewAutoSizeColumnMode.None
            };
            if (formato != null)
            {
                columna.DefaultCellStyle.Format = formato;
                columna.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            }
            grilla.Columns.Add(columna);
            return columna;
        }

        public static Button Boton(string texto, EventHandler alHacerClic, int ancho = 170)
        {
            var boton = new Button { Text = texto, Width = ancho, Height = 42, Font = Normal, Margin = new Padding(4) };
            boton.Click += alHacerClic;
            return boton;
        }

        /// <summary>
        /// Pinta un botón importante (cobrar, confirmar, anular) con su color y letra blanca.
        /// En el tema oscuro Windows dibuja los botones comunes a su manera e ignora el color de fondo,
        /// por eso ahí se lo dibuja plano: así se ve del mismo color en los dos temas.
        /// </summary>
        public static void Destacar(Button boton, Color fondo)
        {
            boton.BackColor = fondo;
            boton.ForeColor = Color.White;
            if (Oscuro)
            {
                boton.FlatStyle = FlatStyle.Flat;
                boton.FlatAppearance.BorderColor = ControlPaint.Light(fondo);
                boton.FlatAppearance.MouseOverBackColor = ControlPaint.Light(fondo, 0.3f);
                boton.FlatAppearance.MouseDownBackColor = ControlPaint.Dark(fondo, 0.1f);
            }
        }

        public static NumericUpDown CampoNumero(int decimales, decimal maximo = 100_000_000)
        {
            return new NumericUpDown
            {
                DecimalPlaces = decimales,
                Maximum = maximo,
                Minimum = 0,
                ThousandsSeparator = true,
                Width = 200,
                Font = Normal,
                TextAlign = HorizontalAlignment.Right
            };
        }

        /// <summary>Lista desplegable de opciones fijas, mostrando cada una con un texto a elección.</summary>
        public static ComboBox Opciones<T>(IEnumerable<T> opciones, Func<T, string> texto, int ancho = 260) where T : notnull
        {
            var combo = new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                Width = ancho,
                FormattingEnabled = true, // sin esto WinForms ignora el evento Format
                DataSource = opciones.ToList()
            };
            combo.Format += (_, e) => e.Value = texto((T)e.ListItem!);
            return combo;
        }

        /// <summary>Reemplaza las opciones de una lista creada con <see cref="Opciones"/>.</summary>
        public static void CambiarOpciones<T>(ComboBox combo, IEnumerable<T> opciones) =>
            combo.DataSource = opciones.ToList();

        /// <summary>Elige la primera opción que cumpla la condición (o la primera de todas si ninguna cumple).</summary>
        public static void Elegir<T>(ComboBox combo, Func<T, bool> condicion)
        {
            for (var i = 0; i < combo.Items.Count; i++)
            {
                if (condicion((T)combo.Items[i]!))
                {
                    combo.SelectedIndex = i;
                    return;
                }
            }
            if (combo.Items.Count > 0)
                combo.SelectedIndex = 0;
        }

        /// <summary>
        /// Lee lo que está escrito en el campo aunque todavía no se haya confirmado
        /// (Value recién se actualiza al salir del campo).
        /// </summary>
        public static decimal ValorTipeado(this NumericUpDown campo) =>
            decimal.TryParse(campo.Text, NumberStyles.Number, CultureInfo.CurrentCulture, out var valor) ? valor : 0;

        /// <summary>"1 producto" o "3 productos".</summary>
        public static string Productos(int cantidad) => cantidad == 1 ? "1 producto" : $"{cantidad} productos";

        public static Label Etiqueta(string texto, Font? fuente = null) =>
            new() { Text = texto, AutoSize = true, Font = fuente ?? Normal, Anchor = AnchorStyles.Left, Margin = new Padding(3, 8, 12, 8) };
    }
}

using KioscoRockAndRoll.Dominio.Entidades;
using KioscoRockAndRoll.Dominio.Servicios;

namespace Proyecto_Kiosco_Rock_Roll.Interfaz.Controles
{
    /// <summary>Un producto elegido en la barra y cuántas unidades ("3*coca" = 3).</summary>
    public sealed record ProductoElegido(Producto Producto, int Cantidad);

    /// <summary>Un mensaje para mostrarle al usuario (por ejemplo, "No hay ningún producto que diga...").</summary>
    public sealed record AvisoDeBarra(string Texto, Color Color);

    /// <summary>
    /// Una sola barra para escanear el código o escribir el nombre de un producto. El lector de códigos funciona
    /// como un teclado (escribe el código y aprieta Enter); si se escriben letras, aparecen debajo los productos
    /// que coinciden y se elige con ↑ ↓ y Enter, o con un clic. La usan la Venta y las Etiquetas.
    /// <para>
    /// La pantalla que la usa decide qué hacer con el producto elegido (<see cref="Elegido"/>), con un código
    /// que no existe (<see cref="CodigoDesconocido"/>) y con las teclas de acción (<see cref="AccionDeTecla"/>):
    /// ↑ ↓ cuando no hay lista abierta, y Supr, + y − con la barra vacía.
    /// </para>
    /// </summary>
    public sealed class BarraDeProductos
    {
        private const int MaximoSugerencias = 8;
        private const int AnchoMinimoDeLaLista = 760;

        private readonly TextBox _barra;
        private readonly Control _anfitrion;
        private readonly InventarioServicio _inventario;
        private bool _descartarCaracter;

        // Flota sobre la pantalla, justo debajo de la barra.
        private readonly ListBox lstSugerencias = new()
        {
            Font = Estilos.Grande,
            DrawMode = DrawMode.OwnerDrawFixed,
            ItemHeight = 36,
            IntegralHeight = false,
            BorderStyle = BorderStyle.FixedSingle,
            TabStop = false,
            Visible = false
        };

        public event EventHandler<ProductoElegido>? Elegido;

        /// <summary>Se escaneó (o escribió) un código que no está cargado.</summary>
        public event EventHandler<CodigoEscaneado>? CodigoDesconocido;

        public event EventHandler<AvisoDeBarra>? Aviso;

        /// <summary>
        /// Lo que hace la pantalla con ↑ ↓ (sin lista abierta) y con Supr, + y − (con la barra vacía).
        /// Recibe <see cref="Keys.Up"/>, <see cref="Keys.Down"/>, <see cref="Keys.Delete"/>, <see cref="Keys.Add"/> o
        /// <see cref="Keys.Subtract"/> y devuelve si la usó.
        /// </summary>
        public Func<Keys, bool>? AccionDeTecla { get; set; }

        /// <param name="barra">El cuadro de texto donde se escanea o escribe.</param>
        /// <param name="anfitrion">La pantalla sobre la que flota la lista de coincidencias.</param>
        public BarraDeProductos(TextBox barra, Control anfitrion, InventarioServicio inventario)
        {
            _barra = barra;
            _anfitrion = anfitrion;
            _inventario = inventario;

            anfitrion.Controls.Add(lstSugerencias);
            lstSugerencias.BringToFront();

            barra.KeyDown += Barra_KeyDown;
            barra.KeyPress += Barra_KeyPress;
            barra.TextChanged += (_, _) => ActualizarSugerencias();
            // Si el foco se va a otro lado (que no sea la lista misma), la lista se cierra.
            barra.Leave += (_, _) => anfitrion.BeginInvoke(() =>
            {
                if (!lstSugerencias.Focused && !barra.Focused)
                    OcultarSugerencias();
            });
            lstSugerencias.DrawItem += DibujarSugerencia;
            lstSugerencias.MouseMove += (_, e) =>
            {
                var indice = lstSugerencias.IndexFromPoint(e.Location);
                if (indice >= 0 && indice != lstSugerencias.SelectedIndex)
                    lstSugerencias.SelectedIndex = indice;
            };
            lstSugerencias.MouseClick += (_, _) => ElegirDesdeLaBarra();
        }

        /// <summary>Si la lista de coincidencias está abierta (Esc tiene que cerrarla antes que hacer otra cosa).</summary>
        public bool ListaAbierta => lstSugerencias.Visible;

        public void OcultarSugerencias()
        {
            lstSugerencias.Visible = false;
            lstSugerencias.Items.Clear();
        }

        // ---------- Teclas ----------

        private void Barra_KeyDown(object? sender, KeyEventArgs e)
        {
            _descartarCaracter = false;
            var barraVacia = _barra.Text == "";
            var manejada = e.KeyCode switch
            {
                Keys.Enter => Hacer(ElegirDesdeLaBarra),
                // ↑ ↓ recorren la lista de coincidencias si está abierta; si no, son para la pantalla.
                Keys.Down or Keys.Up when lstSugerencias.Visible => Hacer(() => MoverEnLaLista(e.KeyCode == Keys.Down ? 1 : -1)),
                Keys.Down or Keys.Up => Accion(e.KeyCode),
                // Con la barra vacía, Supr, + y − son para la pantalla (por ejemplo, sobre el producto elegido).
                Keys.Delete when barraVacia => Accion(Keys.Delete),
                Keys.Add or Keys.Oemplus when barraVacia => Accion(Keys.Add),
                Keys.Subtract or Keys.OemMinus when barraVacia => Accion(Keys.Subtract),
                _ => false
            };
            if (manejada)
            {
                e.Handled = true;
                e.SuppressKeyPress = true; // evita el "beep" de Windows
                _descartarCaracter = true; // y que el "+" o el "−" se escriban en la barra (ver KeyPress)
            }
        }

        // SuppressKeyPress no siempre alcanza a descartar el carácter de la tecla (llega en un mensaje aparte):
        // si la tecla ya se usó como acción, su carácter se descarta acá.
        private void Barra_KeyPress(object? sender, KeyPressEventArgs e)
        {
            if (!_descartarCaracter)
                return;
            _descartarCaracter = false;
            e.Handled = true;
        }

        private static bool Hacer(Action accion)
        {
            accion();
            return true;
        }

        private bool Accion(Keys tecla) => AccionDeTecla?.Invoke(tecla) ?? false;

        private void MoverEnLaLista(int diferencia) =>
            lstSugerencias.SelectedIndex = Math.Clamp(lstSugerencias.SelectedIndex + diferencia, 0, lstSugerencias.Items.Count - 1);

        // ---------- Enter ----------

        /// <summary>
        /// Primero se busca el código exacto (lo que manda el lector); si no hay, el producto elegido en la lista de
        /// coincidencias; si se escribió un nombre, se busca por nombre. Un código que no existe se avisa con
        /// <see cref="CodigoDesconocido"/>.
        /// </summary>
        private void ElegirDesdeLaBarra()
        {
            var texto = _barra.Text;
            var sugerido = lstSugerencias.Visible ? lstSugerencias.SelectedItem as Producto : null;

            if (!CodigoEscaneado.TryInterpretar(texto, out var escaneado))
            {
                if (texto.Trim() != "")
                    Avisar("No se entendió. Usá \"cantidad*código\", por ejemplo 3*779123.", Estilos.Error);
                return;
            }

            OcultarSugerencias();
            _barra.Clear();

            if (_inventario.ObtenerPorCodigo(escaneado.Codigo) is { } producto)
            {
                Elegido?.Invoke(this, new ProductoElegido(producto, escaneado.Cantidad));
                return;
            }
            if (sugerido != null)
            {
                Elegido?.Invoke(this, new ProductoElegido(sugerido, escaneado.Cantidad));
                return;
            }
            if (escaneado.Codigo.Any(char.IsLetter))
            {
                var encontrados = _inventario.Buscar(escaneado.Codigo);
                if (encontrados.Count == 1)
                {
                    Elegido?.Invoke(this, new ProductoElegido(encontrados[0], escaneado.Cantidad));
                    return;
                }
                // Ninguno (o varios sin elegir): se deja lo escrito para corregirlo.
                _barra.Text = texto;
                _barra.SelectionStart = texto.Length;
                Avisar(encontrados.Count == 0
                    ? $"No hay ningún producto que diga \"{escaneado.Codigo}\"."
                    : "Elegí uno de la lista con ↑ ↓ y apretá Enter.", Estilos.Advertencia);
                return;
            }

            CodigoDesconocido?.Invoke(this, escaneado);
        }

        private void Avisar(string texto, Color color) => Aviso?.Invoke(this, new AvisoDeBarra(texto, color));

        // ---------- Coincidencias por nombre ----------

        /// <summary>
        /// Si se escribieron letras, muestra debajo de la barra los productos que coinciden. Con solo números
        /// (lo que manda el lector de códigos) no aparece nada.
        /// </summary>
        private void ActualizarSugerencias()
        {
            var texto = _barra.Text;
            var asterisco = texto.IndexOf('*');
            if (asterisco >= 0)
                texto = texto[(asterisco + 1)..]; // "3*coca" busca "coca"
            texto = texto.Trim();

            if (texto.Length < 2 || !texto.Any(char.IsLetter) || !_barra.Enabled)
            {
                OcultarSugerencias();
                return;
            }

            var encontrados = _inventario.Buscar(texto).Take(MaximoSugerencias).ToList();
            if (encontrados.Count == 0)
            {
                OcultarSugerencias();
                return;
            }

            lstSugerencias.BeginUpdate();
            lstSugerencias.Items.Clear();
            foreach (var producto in encontrados)
                lstSugerencias.Items.Add(producto);
            lstSugerencias.EndUpdate();
            lstSugerencias.SelectedIndex = 0;

            // Justo debajo de la barra, más ancha que ella para que entren los nombres largos.
            var origen = _anfitrion.PointToClient(_barra.Parent!.PointToScreen(new Point(_barra.Left, _barra.Bottom)));
            lstSugerencias.SetBounds(origen.X, origen.Y + 2, Math.Max(_barra.Width, AnchoMinimoDeLaLista),
                encontrados.Count * lstSugerencias.ItemHeight + 4);
            lstSugerencias.Visible = true;
            lstSugerencias.BringToFront();
        }

        /// <summary>Cada coincidencia: el nombre a la izquierda y el precio a la derecha.</summary>
        private void DibujarSugerencia(object? sender, DrawItemEventArgs e)
        {
            e.DrawBackground();
            if (e.Index < 0 || lstSugerencias.Items[e.Index] is not Producto producto)
                return;
            var color = (e.State & DrawItemState.Selected) != 0 ? SystemColors.HighlightText : lstSugerencias.ForeColor;
            const int anchoPrecio = 170;
            var zona = Rectangle.Inflate(e.Bounds, -8, 0);
            var zonaNombre = zona with { Width = zona.Width - anchoPrecio };
            TextRenderer.DrawText(e.Graphics, producto.Nombre, e.Font, zonaNombre, color,
                TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);
            TextRenderer.DrawText(e.Graphics, producto.PrecioVenta.ToString("C2"), e.Font, zona, color,
                TextFormatFlags.Right | TextFormatFlags.VerticalCenter);
        }
    }
}

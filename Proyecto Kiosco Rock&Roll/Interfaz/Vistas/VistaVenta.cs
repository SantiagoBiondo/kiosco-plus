using KioscoRockAndRoll.Datos;
using KioscoRockAndRoll.Dominio.Entidades;
using KioscoRockAndRoll.Dominio.Servicios;
using KioscoRockAndRoll.Reportes;
using Proyecto_Kiosco_Rock_Roll.Interfaz.Controles;
using Proyecto_Kiosco_Rock_Roll.Interfaz.Impresion;

namespace Proyecto_Kiosco_Rock_Roll.Interfaz.Vistas
{
    /// <summary>
    /// Mostrador: se escanean los productos y se cobra.
    /// Una sola barra sirve para el código y para el nombre: el lector de códigos funciona como un teclado
    /// (escribe el código y aprieta Enter) y, si se escriben letras, aparecen debajo los productos que coinciden.
    /// Desde la barra vacía, Supr, + y − actúan sobre el producto elegido del carrito (el último agregado).
    /// El alta de productos nuevos y el cobro se abren en el panel de la derecha.
    /// </summary>
    public class VistaVenta : Vista
    {
        private readonly InventarioServicio _inventario;
        private readonly VentaServicio _ventas;
        private readonly CajaServicio _caja;
        private readonly ImpresoraDeTickets _impresora;
        private readonly AjustesDeCobro _ajustesDeCobro;
        private readonly Carrito _carrito = new();
        private ReglasDeCobro _reglas = ReglasDeCobro.Ninguna;

        private readonly PanelLateral panelLateral = new();
        private readonly EditorProducto editor;
        private readonly PanelAbrirCaja panelAbrirCaja;
        private readonly PanelCobro panelCobro = new();
        private readonly PanelPrecioAlVender panelPrecio = new();

        private const int MaximoSugerencias = 8;

        private readonly TextBox txtCodigo = new() { Font = Estilos.Grande, Width = 520, PlaceholderText = "Escaneá el código o escribí el nombre" };

        // Los productos que coinciden con lo que se escribe (flota sobre el carrito, debajo de la barra).
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
        private readonly DataGridView grilla = new();
        private readonly DataGridViewTextBoxColumn colCantidad;
        private readonly Label lblTotal = new() { Font = Estilos.Total, AutoSize = true };
        private readonly Label lblEstado = new() { AutoSize = true, Font = Estilos.Normal, Anchor = AnchorStyles.Left };

        public VistaVenta(InventarioServicio inventario, VentaServicio ventas, CajaServicio caja, ImpresoraDeTickets impresora,
            AjustesDeCobro ajustesDeCobro, EditorProducto editor, PanelAbrirCaja panelAbrirCaja)
        {
            _ajustesDeCobro = ajustesDeCobro;
            _inventario = inventario;
            _ventas = ventas;
            _caja = caja;
            _impresora = impresora;
            this.editor = editor;
            this.panelAbrirCaja = panelAbrirCaja;

            // Zona de escaneo.
            var escaneo = new FlowLayoutPanel { Dock = DockStyle.Top, AutoSize = true, Padding = new Padding(6) };
            escaneo.Controls.Add(new Label { Text = "Código o nombre:", AutoSize = true, Font = Estilos.Grande, Margin = new Padding(3, 8, 3, 3) });
            escaneo.Controls.Add(txtCodigo);
            escaneo.Controls.Add(new Label
            {
                Text = "\"3*código\" agrega 3  ·  Supr quita el elegido  ·  + y − cambian la cantidad  ·  ↑ ↓ eligen otro",
                AutoSize = true,
                ForeColor = SystemColors.GrayText,
                Margin = new Padding(12, 12, 3, 3)
            });

            // Carrito. La grilla no está enlazada a datos: se redibuja desde el Carrito cada vez que cambia.
            Estilos.ConfigurarGrilla(grilla);
            Estilos.Columna(grilla, "Codigo", "Código", ancho: 160);
            Estilos.Columna(grilla, "Nombre", "Producto", llenar: true);
            colCantidad = Estilos.Columna(grilla, "Cantidad", "Cant.", "N0", 90, soloLectura: false);
            Estilos.Columna(grilla, "Precio", "Precio", "C2", 140);
            Estilos.Columna(grilla, "Subtotal", "Subtotal", "C2", 160);
            grilla.CellValidating += Grilla_CellValidating;
            grilla.CellEndEdit += Grilla_CellEndEdit;
            grilla.KeyDown += (_, e) =>
            {
                if (e.KeyCode == Keys.Delete) { QuitarSeleccionado(); e.Handled = true; }
            };

            // Barra de abajo: acciones sobre el carrito, total y cobrar.
            var abajo = new TableLayoutPanel { Dock = DockStyle.Bottom, ColumnCount = 3, AutoSize = true, Padding = new Padding(6) };
            abajo.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            abajo.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            abajo.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));

            var acciones = new FlowLayoutPanel { AutoSize = true };
            acciones.Controls.Add(Estilos.Boton("+1", (_, _) => SumarAlSeleccionado(+1), 60));
            acciones.Controls.Add(Estilos.Boton("-1", (_, _) => SumarAlSeleccionado(-1), 60));
            acciones.Controls.Add(Estilos.Boton("Quitar (Supr)", (_, _) => QuitarSeleccionado(), 150));
            acciones.Controls.Add(Estilos.Boton("Cancelar venta", (_, _) => CancelarVenta(), 170));

            var totalYCobro = new FlowLayoutPanel { AutoSize = true };
            totalYCobro.Controls.Add(new Label { Text = "TOTAL:", AutoSize = true, Font = Estilos.Grande, Margin = new Padding(3, 16, 3, 3) });
            totalYCobro.Controls.Add(lblTotal);
            var btnCobrar = Estilos.Boton("COBRAR (F12)", (_, _) => AbrirCobro(), 200);
            btnCobrar.Height = 60;
            btnCobrar.Font = Estilos.Grande;
            Estilos.Destacar(btnCobrar, Estilos.FondoBotonPrincipal);
            totalYCobro.Controls.Add(btnCobrar);

            abajo.Controls.Add(acciones, 0, 0);
            abajo.Controls.Add(lblEstado, 1, 0);
            abajo.Controls.Add(totalYCobro, 2, 0);

            // El orden importa: lo que tiene Dock = Fill va primero.
            var principal = new Panel { Dock = DockStyle.Fill };
            principal.Controls.Add(grilla);
            principal.Controls.Add(abajo);
            principal.Controls.Add(escaneo);
            Controls.Add(principal);
            Controls.Add(panelLateral);
            Controls.Add(lstSugerencias);
            lstSugerencias.BringToFront();

            txtCodigo.KeyDown += TxtCodigo_KeyDown;
            txtCodigo.KeyPress += TxtCodigo_KeyPress;
            txtCodigo.TextChanged += (_, _) => ActualizarSugerencias();
            // Si el foco se va a otro lado (que no sea la lista misma), la lista se cierra.
            txtCodigo.Leave += (_, _) => BeginInvoke(() =>
            {
                if (!lstSugerencias.Focused && !txtCodigo.Focused)
                    OcultarSugerencias();
            });
            lstSugerencias.DrawItem += DibujarSugerencia;
            lstSugerencias.MouseMove += (_, e) =>
            {
                var indice = lstSugerencias.IndexFromPoint(e.Location);
                if (indice >= 0 && indice != lstSugerencias.SelectedIndex)
                    lstSugerencias.SelectedIndex = indice;
            };
            lstSugerencias.MouseClick += (_, _) => AgregarDesdeLaBarra();
            _carrito.Cambio += (_, _) => DibujarCarrito();

            editor.Guardado += (_, producto) => { CerrarPanel(); Agregar(producto, _cantidadPendiente); };
            editor.Cancelado += (_, _) => { CerrarPanel(); MostrarEstado("No se dio de alta el producto.", SystemColors.GrayText); };
            panelPrecio.Confirmado += (_, elegido) =>
            {
                CerrarPanel();
                AgregarAlCarrito(elegido.Producto, elegido.Cantidad, elegido.Precio);
            };
            panelPrecio.Cancelado += (_, _) => { CerrarPanel(); MostrarEstado("No se agregó el producto.", SystemColors.GrayText); };
            panelCobro.Confirmado += (_, cobro) => Cobrar(cobro);
            panelCobro.Cancelado += (_, _) => CerrarPanel();
            // Si se quiso cobrar con la caja cerrada: al abrirla, se sigue con el cobro.
            panelAbrirCaja.Abierta += (_, turno) =>
            {
                MostrarEstado($"Caja abierta con {turno.EfectivoInicial:C2}.", Estilos.Exito);
                AbrirCobro();
            };
            panelAbrirCaja.Cancelado += (_, _) =>
            {
                CerrarPanel();
                MostrarEstado("Para cobrar hay que abrir la caja.", Estilos.Advertencia);
            };

            DibujarCarrito();
        }

        public override string Titulo => "Venta";
        public override Keys Atajo => Keys.F1;

        public override void AlMostrar()
        {
            // Si se editó un producto (precio, stock) en otra sección mientras había una venta a medias,
            // el carrito toma los datos nuevos.
            if (!_carrito.EstaVacio)
                _carrito.Refrescar(_inventario.ObtenerPorId);
            if (_caja.TurnoAbierto is null && !panelLateral.Abierto)
                MostrarEstado("La caja está cerrada: se abre al cobrar la primera venta (o desde la sección Caja).", SystemColors.GrayText);
            EnfocarPanelOCodigo();
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            // Las teclas que manejan el buscador, el editor o el cobro llegan primero a ellos.
            switch (keyData)
            {
                // F5 era "buscar por nombre": ahora se busca en la misma barra, así que solo la deja lista para escribir.
                case Keys.F5 when txtCodigo.Enabled: txtCodigo.Focus(); txtCodigo.SelectAll(); return true;
                case Keys.F12: AbrirCobro(); return true;
                case Keys.Escape when lstSugerencias.Visible: OcultarSugerencias(); return true;
                case Keys.Escape when !grilla.IsCurrentCellInEditMode: CancelarVenta(); return true;
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                // Los controles del panel lateral pueden no estar en pantalla: se liberan a mano.
                editor.Dispose();
                panelCobro.Dispose();
                panelPrecio.Dispose();
                panelAbrirCaja.Dispose();
            }
            base.Dispose(disposing);
        }

        // ---------- Escaneo ----------

        private int _cantidadPendiente = 1;

        private void TxtCodigo_KeyDown(object? sender, KeyEventArgs e)
        {
            _descartarCaracter = false;
            var barraVacia = txtCodigo.Text == "";
            var manejada = true;
            switch (e.KeyCode)
            {
                case Keys.Enter:
                    AgregarDesdeLaBarra();
                    break;
                // ↑ ↓ recorren la lista de coincidencias si está abierta; si no, eligen otro producto del carrito.
                case Keys.Down or Keys.Up when lstSugerencias.Visible:
                    lstSugerencias.SelectedIndex = Math.Clamp(lstSugerencias.SelectedIndex + (e.KeyCode == Keys.Down ? 1 : -1),
                        0, lstSugerencias.Items.Count - 1);
                    break;
                case Keys.Down or Keys.Up:
                    MoverSeleccion(e.KeyCode == Keys.Down ? 1 : -1);
                    break;
                // Con la barra vacía, Supr, + y − actúan sobre el producto elegido (el último agregado).
                case Keys.Delete when barraVacia:
                    QuitarSeleccionado();
                    break;
                case Keys.Add or Keys.Oemplus when barraVacia:
                    SumarAlSeleccionado(+1);
                    break;
                case Keys.Subtract or Keys.OemMinus when barraVacia:
                    SumarAlSeleccionado(-1);
                    break;
                default:
                    manejada = false;
                    break;
            }
            if (manejada)
            {
                e.Handled = true;
                e.SuppressKeyPress = true; // evita el "beep" de Windows
                _descartarCaracter = true; // y que el "+" o el "−" se escriban en la barra (ver KeyPress)
            }
        }

        // SuppressKeyPress no siempre alcanza a descartar el carácter de la tecla (llega en un mensaje aparte):
        // si la tecla ya se usó como acción, su carácter se descarta acá.
        private bool _descartarCaracter;

        private void TxtCodigo_KeyPress(object? sender, KeyPressEventArgs e)
        {
            if (!_descartarCaracter)
                return;
            _descartarCaracter = false;
            e.Handled = true;
        }

        /// <summary>
        /// Enter en la barra: primero se busca el código exacto (lo que manda el lector); si no hay, el producto
        /// elegido en la lista de coincidencias; si se escribió un nombre, se busca por nombre. Un código
        /// que no existe se da de alta ahí mismo.
        /// </summary>
        private void AgregarDesdeLaBarra()
        {
            var texto = txtCodigo.Text;
            var sugerido = lstSugerencias.Visible ? lstSugerencias.SelectedItem as Producto : null;

            if (!CodigoEscaneado.TryInterpretar(texto, out var escaneado))
            {
                if (texto.Trim() != "")
                    MostrarEstado("No se entendió. Usá \"cantidad*código\", por ejemplo 3*779123.", Estilos.Error);
                return;
            }

            OcultarSugerencias();
            txtCodigo.Clear();

            if (_inventario.ObtenerPorCodigo(escaneado.Codigo) is { } producto)
            {
                Agregar(producto, escaneado.Cantidad);
                return;
            }
            if (sugerido != null)
            {
                Agregar(sugerido, escaneado.Cantidad);
                return;
            }
            if (escaneado.Codigo.Any(char.IsLetter))
            {
                var encontrados = _inventario.Buscar(escaneado.Codigo);
                if (encontrados.Count == 1)
                {
                    Agregar(encontrados[0], escaneado.Cantidad);
                    return;
                }
                // Ninguno (o varios sin elegir): se deja lo escrito para corregirlo.
                txtCodigo.Text = texto;
                txtCodigo.SelectionStart = texto.Length;
                MostrarEstado(encontrados.Count == 0
                    ? $"No hay ningún producto que diga \"{escaneado.Codigo}\"."
                    : "Elegí uno de la lista con ↑ ↓ y apretá Enter.", Estilos.Advertencia);
                return;
            }

            // Código desconocido: se da de alta ahí mismo y, al guardar, se agrega al carrito.
            _cantidadPendiente = escaneado.Cantidad;
            editor.NuevoProducto(escaneado.Codigo);
            AbrirPanel(editor);
            editor.Enfocar();
            MostrarEstado($"El código {escaneado.Codigo} no está registrado: completá los datos a la derecha.", Estilos.Advertencia);
        }

        // ---------- Coincidencias por nombre ----------

        /// <summary>
        /// Si se escribieron letras, muestra debajo de la barra los productos que coinciden. Con solo números
        /// (lo que manda el lector de códigos) no aparece nada.
        /// </summary>
        private void ActualizarSugerencias()
        {
            var texto = txtCodigo.Text;
            var asterisco = texto.IndexOf('*');
            if (asterisco >= 0)
                texto = texto[(asterisco + 1)..]; // "3*coca" busca "coca"
            texto = texto.Trim();

            if (texto.Length < 2 || !texto.Any(char.IsLetter) || !txtCodigo.Enabled)
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
            var origen = PointToClient(txtCodigo.Parent!.PointToScreen(new Point(txtCodigo.Left, txtCodigo.Bottom)));
            lstSugerencias.SetBounds(origen.X, origen.Y + 2, Math.Max(txtCodigo.Width, 760),
                encontrados.Count * lstSugerencias.ItemHeight + 4);
            lstSugerencias.Visible = true;
            lstSugerencias.BringToFront();
        }

        private void OcultarSugerencias()
        {
            lstSugerencias.Visible = false;
            lstSugerencias.Items.Clear();
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

        /// <summary>
        /// Agrega el producto al carrito. Si el precio se pone al vender, primero se pregunta en el panel de la derecha.
        /// </summary>
        private void Agregar(Producto producto, int cantidad)
        {
            if (producto.PrecioAlVender)
            {
                AbrirPanel(panelPrecio);
                panelPrecio.Iniciar(producto, cantidad);
                MostrarEstado($"Poné el precio de \"{producto.Nombre}\" a la derecha.", SystemColors.ControlText);
                return;
            }
            AgregarAlCarrito(producto, cantidad, null);
        }

        private void AgregarAlCarrito(Producto producto, int cantidad, decimal? precio)
        {
            var item = _carrito.Agregar(producto, cantidad, precio);
            Seleccionar(item);
            InformarItem(item);
            txtCodigo.Focus();
        }

        // ---------- Edición del carrito ----------

        // Cada fila guarda su renglón del carrito: un mismo producto con precio al vender puede estar en varias.
        private ItemCarrito? ItemSeleccionado => grilla.CurrentRow?.Tag as ItemCarrito;

        private void SumarAlSeleccionado(int diferencia)
        {
            if (ItemSeleccionado is not { } item)
                return;

            _carrito.CambiarCantidad(item, item.Cantidad + diferencia);
            if (_carrito.Items.Contains(item))
            {
                Seleccionar(item);
                InformarItem(item);
            }
            txtCodigo.Focus();
        }

        private void QuitarSeleccionado()
        {
            if (ItemSeleccionado is { } item)
            {
                _carrito.Quitar(item);
                MostrarEstado($"Se quitó \"{item.Producto.Nombre}\".", SystemColors.GrayText);
            }
            txtCodigo.Focus();
        }

        /// <summary>Elige el producto de arriba o de abajo en el carrito, sin salir de la barra.</summary>
        private void MoverSeleccion(int diferencia)
        {
            if (grilla.Rows.Count == 0)
                return;
            var actual = grilla.CurrentRow?.Index ?? grilla.Rows.Count - 1;
            grilla.CurrentCell = grilla.Rows[Math.Clamp(actual + diferencia, 0, grilla.Rows.Count - 1)].Cells[0];
            if (ItemSeleccionado is { } item)
                InformarItem(item);
        }

        private void Grilla_CellValidating(object? sender, DataGridViewCellValidatingEventArgs e)
        {
            if (e.ColumnIndex != colCantidad.Index || !grilla.IsCurrentCellInEditMode)
                return;
            if (!int.TryParse(e.FormattedValue?.ToString(), out var cantidad) || cantidad < 0)
            {
                MostrarEstado("La cantidad tiene que ser un número entero.", Estilos.Error);
                e.Cancel = true;
            }
        }

        private void Grilla_CellEndEdit(object? sender, DataGridViewCellEventArgs e)
        {
            var fila = grilla.Rows[e.RowIndex];
            if (fila.Tag is not ItemCarrito item || !int.TryParse(fila.Cells[e.ColumnIndex].Value?.ToString(), out var cantidad))
                return;

            // Se difiere porque redibujar la grilla dentro de su propio evento de edición no está permitido.
            BeginInvoke(() =>
            {
                _carrito.CambiarCantidad(item, cantidad);
                txtCodigo.Focus();
            });
        }

        private void CancelarVenta()
        {
            if (!_carrito.EstaVacio && Mensajes.Confirmar(this, "¿Cancelar la venta actual?"))
            {
                _carrito.Vaciar();
                MostrarEstado("Venta cancelada.", SystemColors.GrayText);
            }
            txtCodigo.Focus();
        }

        // ---------- Panel lateral ----------

        private void AbrirCobro()
        {
            if (_carrito.EstaVacio)
            {
                MostrarEstado("No hay productos para cobrar.", Estilos.Advertencia);
                txtCodigo.Focus();
                return;
            }
            if (_caja.TurnoAbierto is null)
            {
                AbrirPanel(panelAbrirCaja);
                panelAbrirCaja.Iniciar("La caja está cerrada. Abrila para poder cobrar.");
                return;
            }
            _reglas = _ajustesDeCobro.Leer();
            AbrirPanel(panelCobro);
            panelCobro.Iniciar(_carrito, _reglas, _impresora.Configuracion);
        }

        private void Cobrar(Cobro cobro)
        {
            var imprimirTicket = panelCobro.ImprimirTicket;
            var venta = _ventas.Cobrar(_carrito, cobro, _reglas);
            CerrarPanel();
            var vuelto = cobro.Vuelto > 0 ? $"  -  VUELTO: {cobro.Vuelto:C2}" : "";
            MostrarEstado($"Venta Nº {venta.Id} registrada ({venta.Total:C2}, {cobro.Descripcion}){vuelto}", Estilos.Exito);

            if (!imprimirTicket)
                return;
            // La venta ya quedó guardada: si la impresora falla, solo se avisa.
            try
            {
                _impresora.Imprimir(Ticket.DeVenta(venta, cobro, ExportadorDeReportes.NombreNegocio));
            }
            catch (NoSePudoImprimirException ex)
            {
                MostrarEstado($"Venta Nº {venta.Id} registrada{vuelto}. El ticket no salió: {ex.Message}", Estilos.Advertencia);
            }
        }

        private void AbrirPanel(Control contenido)
        {
            OcultarSugerencias();
            panelLateral.Mostrar(contenido);
            // Mientras hay un panel abierto, no se puede escanear para no mezclar acciones.
            txtCodigo.Enabled = false;
        }

        private void CerrarPanel()
        {
            panelLateral.Ocultar();
            txtCodigo.Enabled = true;
            txtCodigo.Focus();
        }

        private void EnfocarPanelOCodigo()
        {
            switch (panelLateral.Contenido)
            {
                case EditorProducto e: e.Enfocar(); break;
                case Control c: c.SelectNextControl(c, true, true, true, true); break;
                default: txtCodigo.Focus(); break;
            }
        }

        // ---------- Presentación ----------

        private void DibujarCarrito()
        {
            var seleccionado = ItemSeleccionado;

            grilla.Rows.Clear();
            foreach (var item in _carrito.Items)
            {
                var indice = grilla.Rows.Add(item.Producto.CodigoBarras, item.Producto.Nombre, item.Cantidad,
                    item.PrecioUnitario, item.Subtotal);
                var fila = grilla.Rows[indice];
                fila.Tag = item;
                if (item.SuperaElStock)
                    fila.DefaultCellStyle.ForeColor = Estilos.Advertencia;
            }

            lblTotal.Text = _carrito.Total.ToString("C2");
            // Queda elegido el mismo producto que antes; si ya no está (se quitó), el último del carrito,
            // así se puede seguir quitando o cambiando cantidades sin tocar el mouse.
            if (seleccionado == null || !Seleccionar(seleccionado))
            {
                if (grilla.Rows.Count > 0)
                    grilla.CurrentCell = grilla.Rows[^1].Cells[0];
            }
        }

        /// <returns>False si el producto ya no está en el carrito.</returns>
        private bool Seleccionar(ItemCarrito item)
        {
            foreach (DataGridViewRow fila in grilla.Rows)
            {
                if (ReferenceEquals(fila.Tag, item))
                {
                    grilla.CurrentCell = fila.Cells[0];
                    return true;
                }
            }
            return false;
        }

        private void InformarItem(ItemCarrito item)
        {
            if (item.SuperaElStock)
                MostrarEstado($"Ojo: de \"{item.Producto.Nombre}\" figuran solo {item.Producto.Stock} en stock.", Estilos.Advertencia);
            else
                MostrarEstado($"{item.Producto.Nombre}  x{item.Cantidad}", SystemColors.ControlText);
        }

        private void MostrarEstado(string texto, Color color)
        {
            lblEstado.Text = texto;
            lblEstado.ForeColor = color;
        }
    }
}

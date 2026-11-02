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
    /// El lector de códigos funciona como un teclado: escribe el código y aprieta Enter.
    /// La búsqueda, el alta de productos nuevos y el cobro se abren en el panel de la derecha.
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
        private readonly BuscadorProducto buscador;
        private readonly EditorProducto editor;
        private readonly PanelAbrirCaja panelAbrirCaja;
        private readonly PanelCobro panelCobro = new();
        private readonly PanelPrecioAlVender panelPrecio = new();

        private readonly TextBox txtCodigo = new() { Font = Estilos.Grande, Width = 420, PlaceholderText = "Escaneá o escribí el código" };
        private readonly DataGridView grilla = new();
        private readonly DataGridViewTextBoxColumn colCantidad;
        private readonly Label lblTotal = new() { Font = Estilos.Total, AutoSize = true };
        private readonly Label lblEstado = new() { AutoSize = true, Font = Estilos.Normal, Anchor = AnchorStyles.Left };

        public VistaVenta(InventarioServicio inventario, VentaServicio ventas, CajaServicio caja, ImpresoraDeTickets impresora,
            AjustesDeCobro ajustesDeCobro, BuscadorProducto buscador, EditorProducto editor, PanelAbrirCaja panelAbrirCaja)
        {
            _ajustesDeCobro = ajustesDeCobro;
            _inventario = inventario;
            _ventas = ventas;
            _caja = caja;
            _impresora = impresora;
            this.buscador = buscador;
            this.editor = editor;
            this.panelAbrirCaja = panelAbrirCaja;

            // Zona de escaneo.
            var escaneo = new FlowLayoutPanel { Dock = DockStyle.Top, AutoSize = true, Padding = new Padding(6) };
            escaneo.Controls.Add(new Label { Text = "Código:", AutoSize = true, Font = Estilos.Grande, Margin = new Padding(3, 8, 3, 3) });
            escaneo.Controls.Add(txtCodigo);
            escaneo.Controls.Add(Estilos.Boton("Buscar por nombre (F5)", (_, _) => AbrirBuscador(), 210));
            escaneo.Controls.Add(new Label
            {
                Text = "Tip: \"3*código\" agrega 3 unidades",
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

            txtCodigo.KeyDown += TxtCodigo_KeyDown;
            _carrito.Cambio += (_, _) => DibujarCarrito();

            buscador.Elegido += (_, producto) => { CerrarPanel(); Agregar(producto, 1); };
            buscador.Cancelado += (_, _) => CerrarPanel();
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
                case Keys.F5: AbrirBuscador(); return true;
                case Keys.F12: AbrirCobro(); return true;
                case Keys.Escape when !grilla.IsCurrentCellInEditMode: CancelarVenta(); return true;
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                // Los controles del panel lateral pueden no estar en pantalla: se liberan a mano.
                buscador.Dispose();
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
            if (e.KeyCode != Keys.Enter)
                return;
            e.SuppressKeyPress = true; // evita el "beep" de Windows

            var texto = txtCodigo.Text;
            txtCodigo.Clear();

            if (!CodigoEscaneado.TryInterpretar(texto, out var escaneado))
            {
                if (texto.Trim() != "")
                    MostrarEstado("No se entendió el código. Usá \"cantidad*código\", por ejemplo 3*779123.", Estilos.Error);
                return;
            }

            if (_inventario.ObtenerPorCodigo(escaneado.Codigo) is { } producto)
            {
                Agregar(producto, escaneado.Cantidad);
                return;
            }

            // Código desconocido: se da de alta ahí mismo y, al guardar, se agrega al carrito.
            _cantidadPendiente = escaneado.Cantidad;
            editor.NuevoProducto(escaneado.Codigo);
            AbrirPanel(editor);
            editor.Enfocar();
            MostrarEstado($"El código {escaneado.Codigo} no está registrado: completá los datos a la derecha.", Estilos.Advertencia);
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
                _carrito.Quitar(item);
            txtCodigo.Focus();
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

        private void AbrirBuscador()
        {
            AbrirPanel(buscador);
            buscador.Activar();
        }

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
            if (seleccionado != null)
                Seleccionar(seleccionado);
        }

        private void Seleccionar(ItemCarrito item)
        {
            foreach (DataGridViewRow fila in grilla.Rows)
            {
                if (ReferenceEquals(fila.Tag, item))
                {
                    grilla.CurrentCell = fila.Cells[0];
                    return;
                }
            }
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

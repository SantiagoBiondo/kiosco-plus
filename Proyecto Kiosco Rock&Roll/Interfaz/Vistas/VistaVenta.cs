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
    /// Los controles están en VistaVenta.Designer.cs (se editan con el diseñador de Visual Studio);
    /// el panel de la derecha y lo que se abre en él se agregan acá porque los arma el programa.
    /// </summary>
    public partial class VistaVenta : Vista
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

        private readonly BarraDeProductos barra;

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

            InitializeComponent();
            Controls.Add(panelLateral);

            // Lo que el diseñador no puede hacer: la grilla y los colores del tema.
            Estilos.ConfigurarGrilla(grilla);
            Estilos.Destacar(btnCobrar, Estilos.FondoBotonPrincipal);

            barra = new BarraDeProductos(txtCodigo, this, inventario);
            barra.Elegido += (_, e) => Agregar(e.Producto, e.Cantidad);
            barra.CodigoDesconocido += (_, e) => DarDeAlta(e);
            barra.Aviso += (_, e) => MostrarEstado(e.Texto, e.Color);
            // Con la barra vacía, Supr, + y − actúan sobre el producto elegido (el último agregado); ↑ ↓ eligen otro.
            barra.AccionDeTecla = tecla =>
            {
                switch (tecla)
                {
                    case Keys.Delete: QuitarSeleccionado(); return true;
                    case Keys.Add: SumarAlSeleccionado(+1); return true;
                    case Keys.Subtract: SumarAlSeleccionado(-1); return true;
                    case Keys.Up or Keys.Down: MoverSeleccion(tecla == Keys.Down ? 1 : -1); return true;
                    default: return false;
                }
            };
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
                case Keys.Escape when barra.ListaAbierta: barra.OcultarSugerencias(); return true;
                case Keys.Escape when !grilla.IsCurrentCellInEditMode: CancelarVenta(); return true;
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }

        // ---------- Lo que pasa al tocar cada cosa (conectado en el diseñador) ----------

        private void Grilla_KeyDown(object? sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Delete) { QuitarSeleccionado(); e.Handled = true; }
        }

        private void BtnSumar_Click(object? sender, EventArgs e) => SumarAlSeleccionado(+1);

        private void BtnRestar_Click(object? sender, EventArgs e) => SumarAlSeleccionado(-1);

        private void BtnQuitar_Click(object? sender, EventArgs e) => QuitarSeleccionado();

        private void BtnCancelarVenta_Click(object? sender, EventArgs e) => CancelarVenta();

        private void BtnCobrar_Click(object? sender, EventArgs e) => AbrirCobro();

        // ---------- Escaneo ----------

        private int _cantidadPendiente = 1;

        /// <summary>Código desconocido: se da de alta ahí mismo y, al guardar, se agrega al carrito.</summary>
        private void DarDeAlta(CodigoEscaneado escaneado)
        {
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
            barra.OcultarSugerencias();
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

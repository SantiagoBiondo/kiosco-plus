using System.Drawing.Printing;
using KioscoRockAndRoll.Datos;
using KioscoRockAndRoll.Dominio;
using KioscoRockAndRoll.Dominio.Entidades;
using KioscoRockAndRoll.Dominio.Servicios;
using Proyecto_Kiosco_Rock_Roll.Interfaz.Controles;
using Proyecto_Kiosco_Rock_Roll.Interfaz.Impresion;

namespace Proyecto_Kiosco_Rock_Roll.Interfaz.Vistas
{
    /// <summary>
    /// Etiquetas de precio para las góndolas: se escanean (o se escriben por nombre, en la misma barra que la Venta)
    /// los productos y se imprimen en hojas A4, una al lado de la otra, con el nombre y el precio.
    /// A la derecha se ve cómo va a quedar la hoja.
    /// Los controles están en VistaEtiquetas.Designer.cs (se editan con el diseñador de Visual Studio).
    /// </summary>
    public partial class VistaEtiquetas : Vista
    {
        private readonly InventarioServicio _inventario;
        private readonly AjustesDeEtiquetas _ajustes;
        private readonly ListaDeEtiquetas _lista = new();
        private bool _cargando;

        private readonly BarraDeProductos barra;

        public VistaEtiquetas(InventarioServicio inventario, AjustesDeEtiquetas ajustes)
        {
            _inventario = inventario;
            _ajustes = ajustes;

            InitializeComponent();

            // Lo que el diseñador no puede hacer: la grilla, los colores del tema y la lista de impresoras.
            Estilos.ConfigurarGrilla(grilla);
            Estilos.Destacar(btnImprimir, Estilos.FondoBotonPrincipal);
            _cargando = true;
            Estilos.PrepararOpciones<string>(cmbImpresora, [], nombre => nombre);
            _cargando = false;

            barra = new BarraDeProductos(txtCodigo, this, inventario);
            barra.Elegido += (_, e) => Agregar(e.Producto, e.Cantidad);
            barra.CodigoDesconocido += (_, e) =>
                MostrarEstado($"El código {e.Codigo} no está registrado. Cargalo primero en Inventario.", Estilos.Error);
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
            _lista.Cambio += (_, _) => Dibujar();

            Dibujar();
        }

        // ---------- Lo que pasa al tocar cada cosa (conectado en el diseñador) ----------

        private void Grilla_KeyDown(object? sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Delete) { QuitarSeleccionado(); e.Handled = true; }
        }

        private void BtnSumar_Click(object? sender, EventArgs e) => SumarAlSeleccionado(+1);

        private void BtnRestar_Click(object? sender, EventArgs e) => SumarAlSeleccionado(-1);

        private void BtnQuitar_Click(object? sender, EventArgs e) => QuitarSeleccionado();

        private void BtnVaciar_Click(object? sender, EventArgs e) => Vaciar();

        private void BtnImprimir_Click(object? sender, EventArgs e) => Imprimir();

        private void BtnHojaAnterior_Click(object? sender, EventArgs e) => CambiarHoja(-1);

        private void BtnHojaSiguiente_Click(object? sender, EventArgs e) => CambiarHoja(+1);

        private void CmbImpresora_SelectedIndexChanged(object? sender, EventArgs e) => GuardarImpresora();

        public override string Titulo => "Etiquetas";

        public override void AlMostrar()
        {
            CargarImpresoras();
            // Los precios pudieron cambiar en otra sección (por ejemplo, un aumento): se toman los nuevos.
            if (!_lista.EstaVacia)
                _lista.Refrescar(_inventario.ObtenerPorId);
            txtCodigo.Focus();
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            switch (keyData)
            {
                // F5 era "buscar por nombre": ahora se busca en la misma barra, así que solo la deja lista para escribir.
                case Keys.F5: txtCodigo.Focus(); txtCodigo.SelectAll(); return true;
                case Keys.F12: Imprimir(); return true;
                case Keys.Escape when barra.ListaAbierta: barra.OcultarSugerencias(); return true;
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }

        // ---------- Escaneo ----------

        private void Agregar(Producto producto, int cantidad)
        {
            var pedida = _lista.Agregar(producto, cantidad);
            Seleccionar(producto.Id);
            MostrarEstado($"{producto.Nombre}: {Etiquetas(pedida.Cantidad)}", SystemColors.ControlText);
            txtCodigo.Focus();
        }

        // ---------- Lista ----------

        private long? SeleccionadoId => grilla.CurrentRow?.Tag as long?;

        private void SumarAlSeleccionado(int diferencia)
        {
            if (SeleccionadoId is not { } id || _lista.Buscar(id) is not { } pedida)
                return;
            _lista.CambiarCantidad(id, pedida.Cantidad + diferencia);
            Seleccionar(id);
        }

        private void QuitarSeleccionado()
        {
            if (SeleccionadoId is { } id && _lista.Buscar(id) is { } pedida)
            {
                _lista.Quitar(id);
                MostrarEstado($"Se quitó \"{pedida.Nombre}\".", SystemColors.GrayText);
            }
            txtCodigo.Focus();
        }

        /// <summary>Elige el producto de arriba o de abajo en la lista, sin salir de la barra.</summary>
        private void MoverSeleccion(int diferencia)
        {
            if (grilla.Rows.Count == 0)
                return;
            var actual = grilla.CurrentRow?.Index ?? grilla.Rows.Count - 1;
            grilla.CurrentCell = grilla.Rows[Math.Clamp(actual + diferencia, 0, grilla.Rows.Count - 1)].Cells[0];
        }

        private void Vaciar()
        {
            if (_lista.EstaVacia || !Mensajes.Confirmar(this, "¿Vaciar la lista de etiquetas?"))
                return;
            _lista.Vaciar();
            MostrarEstado("Lista vacía.", SystemColors.GrayText);
            txtCodigo.Focus();
        }

        /// <returns>False si el producto ya no está en la lista.</returns>
        private bool Seleccionar(long productoId)
        {
            foreach (DataGridViewRow fila in grilla.Rows)
            {
                if (fila.Tag as long? == productoId)
                {
                    grilla.CurrentCell = fila.Cells[0];
                    return true;
                }
            }
            return false;
        }

        private void Dibujar()
        {
            var seleccionado = SeleccionadoId;
            grilla.Rows.Clear();
            foreach (var pedida in _lista.Pedidas)
            {
                var indice = grilla.Rows.Add(pedida.Nombre, pedida.Precio, pedida.Cantidad);
                grilla.Rows[indice].Tag = pedida.Producto.Id;
            }
            // Queda elegido el mismo producto; si se quitó, el último de la lista (así se puede seguir con Supr).
            if (seleccionado is not { } id || !Seleccionar(id))
            {
                if (grilla.Rows.Count > 0)
                    grilla.CurrentCell = grilla.Rows[^1].Cells[0];
            }

            var hojas = _lista.Hojas(ImpresoraDeEtiquetas.PorHoja);
            lblTotal.Text = _lista.EstaVacia
                ? "Sin etiquetas"
                : $"{Etiquetas(_lista.Total)}  ·  {(hojas == 1 ? "1 hoja" : $"{hojas} hojas")}";
            ActualizarVistaPrevia();
        }

        private static string Etiquetas(int cantidad) => cantidad == 1 ? "1 etiqueta" : $"{cantidad} etiquetas";

        // ---------- Vista previa ----------

        private void ActualizarVistaPrevia()
        {
            var anterior = vistaPrevia.Document;
            try
            {
                vistaPrevia.Document = _lista.EstaVacia
                    ? null
                    : ImpresoraDeEtiquetas.Documento(_lista.Etiquetas(), ImpresoraElegida);
            }
            catch (InvalidPrinterException)
            {
                vistaPrevia.Document = null;
                MostrarEstado("No se puede mostrar la hoja: no hay ninguna impresora instalada.", Estilos.Advertencia);
            }
            anterior?.Dispose();

            var hojas = _lista.Hojas(ImpresoraDeEtiquetas.PorHoja);
            vistaPrevia.StartPage = Math.Clamp(vistaPrevia.StartPage, 0, Math.Max(hojas - 1, 0));
            MostrarHoja(hojas);
        }

        private void CambiarHoja(int diferencia)
        {
            var hojas = _lista.Hojas(ImpresoraDeEtiquetas.PorHoja);
            vistaPrevia.StartPage = Math.Clamp(vistaPrevia.StartPage + diferencia, 0, Math.Max(hojas - 1, 0));
            MostrarHoja(hojas);
        }

        private void MostrarHoja(int hojas)
        {
            lblHoja.Text = hojas == 0 ? "" : $"Hoja {vistaPrevia.StartPage + 1} de {hojas}";
            btnHojaAnterior.Visible = btnHojaSiguiente.Visible = hojas > 1;
            btnHojaAnterior.Enabled = vistaPrevia.StartPage > 0;
            btnHojaSiguiente.Enabled = vistaPrevia.StartPage < hojas - 1;
        }

        // ---------- Impresora ----------

        private string? ImpresoraElegida => cmbImpresora.SelectedItem as string;

        /// <summary>Lista las impresoras de Windows y elige la guardada (o la predeterminada).</summary>
        private void CargarImpresoras()
        {
            var instaladas = ImpresoraDeTickets.ImpresorasInstaladas();
            var elegida = _ajustes.Impresora ?? new PrinterSettings().PrinterName;

            _cargando = true;
            Estilos.CambiarOpciones(cmbImpresora, instaladas);
            Estilos.Elegir<string>(cmbImpresora, nombre => nombre == elegida);
            _cargando = false;
            ActualizarVistaPrevia();
        }

        private void GuardarImpresora()
        {
            if (_cargando)
                return;
            _ajustes.Impresora = ImpresoraElegida;
            ActualizarVistaPrevia(); // el tamaño exacto del A4 depende de la impresora
        }

        private void Imprimir()
        {
            if (_lista.EstaVacia)
            {
                MostrarEstado("No hay etiquetas para imprimir: escaneá algún producto.", Estilos.Advertencia);
                txtCodigo.Focus();
                return;
            }

            var hojas = _lista.Hojas(ImpresoraDeEtiquetas.PorHoja);
            var pregunta = $"¿Imprimir {Etiquetas(_lista.Total)} ({(hojas == 1 ? "1 hoja A4" : $"{hojas} hojas A4")}) " +
                           $"en \"{ImpresoraElegida}\"?";
            if (!Mensajes.Confirmar(this, pregunta))
                return;

            try
            {
                Cursor = Cursors.WaitCursor;
                ImpresoraDeEtiquetas.Imprimir(_lista.Etiquetas(), ImpresoraElegida);
                MostrarEstado($"Se mandaron {Etiquetas(_lista.Total)} a la impresora. " +
                              "Cuando salgan bien, podés vaciar la lista.", Estilos.Exito);
            }
            catch (NoSePudoImprimirException ex)
            {
                MostrarEstado(ex.Message, Estilos.Error);
            }
            finally
            {
                Cursor = Cursors.Default;
            }
        }

        private void MostrarEstado(string texto, Color color)
        {
            lblEstado.Text = texto;
            lblEstado.ForeColor = color;
        }
    }
}

using KioscoRockAndRoll.Dominio;
using KioscoRockAndRoll.Dominio.Entidades;
using KioscoRockAndRoll.Dominio.Servicios;

namespace Proyecto_Kiosco_Rock_Roll.Interfaz.Controles
{
    /// <summary>
    /// Cambio de precios para un grupo de productos, con vista previa antes de aplicar:
    /// aumentar o bajar un porcentaje, o poner el mismo precio a todos (por ejemplo, variedades
    /// de un chocolate que valen lo mismo). Enter aplica, Esc cancela.
    /// </summary>
    public class PanelAjustePrecios : UserControl
    {
        private static readonly decimal[] OpcionesRedondeo = [0, 1, 5, 10, 50, 100];

        private sealed record Modo(bool PrecioFijo, string Texto);

        private static readonly Modo[] Modos =
        [
            new(false, "Aumentar o bajar un porcentaje"),
            new(true, "Poner el mismo precio a todos")
        ];

        private static readonly PreciosAAjustar[] PreciosParaPorcentaje = Enum.GetValues<PreciosAAjustar>();
        private static readonly PreciosAAjustar[] PreciosParaPrecioFijo = [PreciosAAjustar.SoloVenta, PreciosAAjustar.SoloCosto];

        private readonly InventarioServicio _inventario;
        private IReadOnlyList<Producto> _productos = [];
        private string _queProductos = "";
        private bool _cambiandoModo;

        private readonly ComboBox cmbModo = Estilos.Opciones(Modos, m => m.Texto, 300);
        private readonly NumericUpDown numPorcentaje = Estilos.CampoNumero(2, AjustePrecios.PorcentajeMaximo);
        private readonly NumericUpDown numPrecio = Estilos.CampoNumero(2);
        private readonly ComboBox cmbPrecios = Estilos.Opciones(PreciosParaPorcentaje, p => p.Descripcion());
        private readonly ComboBox cmbRedondeo = Estilos.Opciones(OpcionesRedondeo,
            multiplo => multiplo == 0 ? "Sin redondear" : $"Hacia arriba, a múltiplos de {multiplo:C0}");
        private readonly Control[] filaPorcentaje;
        private readonly Control[] filaPrecio;
        private readonly Control[] filaRedondeo;
        private readonly Label lblResumen = new() { AutoSize = true, MaximumSize = new Size(480, 0), Margin = new Padding(3, 10, 3, 6) };
        private readonly DataGridView grilla = new();
        private readonly DataGridViewTextBoxColumn colAntes;
        private readonly DataGridViewTextBoxColumn colDespues;

        /// <summary>Se aplicó el ajuste. El valor es la cantidad de productos actualizados.</summary>
        public event EventHandler<int>? Aplicado;

        public event EventHandler? Cancelado;

        public PanelAjustePrecios(InventarioServicio inventario)
        {
            _inventario = inventario;
            Font = Estilos.Normal;

            numPorcentaje.Minimum = -99.99m;
            numPorcentaje.Width = 140;
            numPorcentaje.Font = Estilos.Grande;
            numPrecio.Width = 180;
            numPrecio.Font = Estilos.Grande;

            var tabla = new TableLayoutPanel { ColumnCount = 2, AutoSize = true, Dock = DockStyle.Top };
            var titulo = Estilos.Etiqueta("Actualizar precios", Estilos.Titulo);
            tabla.Controls.Add(titulo);
            tabla.SetColumnSpan(titulo, 2);
            Fila(tabla, "Cómo", cmbModo);
            filaPorcentaje = Fila(tabla, "Porcentaje (%)", numPorcentaje);
            filaPrecio = Fila(tabla, "Precio nuevo ($)", numPrecio);
            Fila(tabla, "Qué precio", cmbPrecios);
            filaRedondeo = Fila(tabla, "Redondeo venta", cmbRedondeo);
            tabla.Controls.Add(lblResumen);
            tabla.SetColumnSpan(lblResumen, 2);

            Estilos.ConfigurarGrilla(grilla);
            grilla.ReadOnly = true;
            grilla.TabStop = false;
            Estilos.Columna(grilla, nameof(CambioDePrecio.Nombre), "Producto", llenar: true);
            colAntes = Estilos.Columna(grilla, nameof(CambioDePrecio.VentaAntes), "Antes", "C2", 105);
            colDespues = Estilos.Columna(grilla, nameof(CambioDePrecio.VentaDespues), "Después", "C2", 105);
            colDespues.DefaultCellStyle.Font = new Font(Estilos.Normal, FontStyle.Bold);

            var botones = new FlowLayoutPanel { Dock = DockStyle.Bottom, AutoSize = true, Padding = new Padding(0, 8, 0, 0) };
            var btnAplicar = Estilos.Boton("Aplicar (Enter)", (_, _) => Aplicar(), 160);
            Estilos.Destacar(btnAplicar, Estilos.FondoBotonPrincipal);
            botones.Controls.Add(btnAplicar);
            botones.Controls.Add(Estilos.Boton("Cancelar (Esc)", (_, _) => Cancelado?.Invoke(this, EventArgs.Empty), 160));

            Controls.Add(grilla);
            Controls.Add(botones);
            Controls.Add(tabla);

            cmbModo.SelectedIndexChanged += (_, _) => ModoCambiado();
            foreach (var campo in new[] { numPorcentaje, numPrecio })
            {
                campo.ValueChanged += (_, _) => ActualizarVistaPrevia();
                campo.KeyUp += (_, _) => ActualizarVistaPrevia();
            }
            cmbPrecios.SelectedIndexChanged += (_, _) => { if (!_cambiandoModo) ActualizarVistaPrevia(); };
            cmbRedondeo.SelectedIndexChanged += (_, _) => ActualizarVistaPrevia();
        }

        /// <param name="productos">Los productos a los que se va a aplicar el ajuste.</param>
        /// <param name="queProductos">Cómo se eligieron, en palabras (ej.: "categoría Golosinas" o "los 3 que marcaste").</param>
        /// <param name="precioFijo">Empezar con "Poner el mismo precio a todos" (cuando se marcaron productos uno por uno).</param>
        public void Iniciar(IReadOnlyList<Producto> productos, string queProductos, bool precioFijo = false)
        {
            foreach (var campo in new[] { numPorcentaje, numPrecio })
            {
                campo.Value = 0;
                campo.Text = "";
            }
            _productos = productos;
            _queProductos = queProductos;
            Estilos.Elegir<Modo>(cmbModo, m => m.PrecioFijo == precioFijo);
            ModoCambiado();
        }

        /// <summary>Cambia el grupo de productos sin perder lo que ya se cargó (por ejemplo, al filtrar o marcar).</summary>
        public void CambiarProductos(IReadOnlyList<Producto> productos, string queProductos)
        {
            _productos = productos;
            _queProductos = queProductos;
            ActualizarVistaPrevia();
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            switch (keyData)
            {
                case Keys.Enter: Aplicar(); return true;
                case Keys.Escape: Cancelado?.Invoke(this, EventArgs.Empty); return true;
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }

        private static Control[] Fila(TableLayoutPanel tabla, string etiqueta, Control campo)
        {
            var label = Estilos.Etiqueta(etiqueta);
            tabla.Controls.Add(label);
            tabla.Controls.Add(campo);
            return [label, campo];
        }

        private bool EsPrecioFijo => cmbModo.SelectedItem is Modo { PrecioFijo: true };

        private PreciosAAjustar PreciosElegidos => (PreciosAAjustar)cmbPrecios.SelectedItem!;

        /// <summary>
        /// Muestra los campos del modo elegido. Con un precio fijo no hay "costo y venta" (un mismo número no
        /// puede ser los dos) ni redondeo.
        /// </summary>
        private void ModoCambiado()
        {
            var fijo = EsPrecioFijo;
            foreach (var control in filaPorcentaje.Concat(filaRedondeo))
                control.Visible = !fijo;
            foreach (var control in filaPrecio)
                control.Visible = fijo;

            _cambiandoModo = true;
            var anterior = cmbPrecios.SelectedItem as PreciosAAjustar?;
            Estilos.CambiarOpciones(cmbPrecios, fijo ? PreciosParaPrecioFijo : PreciosParaPorcentaje);
            Estilos.Elegir<PreciosAAjustar>(cmbPrecios, p => p == (fijo ? PreciosAAjustar.SoloVenta : anterior ?? PreciosAAjustar.CostoYVenta));
            _cambiandoModo = false;

            (fijo ? numPrecio : numPorcentaje).Focus();
            ActualizarVistaPrevia();
        }

        /// <summary>Arma el ajuste con lo cargado en pantalla. Si no es válido devuelve null y dice por qué.</summary>
        private AjustePrecios? LeerAjuste(out string error)
        {
            error = "";
            try
            {
                return EsPrecioFijo
                    ? AjustePrecios.APrecioFijo(numPrecio.ValorTipeado(), PreciosElegidos)
                    : new AjustePrecios(numPorcentaje.ValorTipeado(), PreciosElegidos, (decimal)cmbRedondeo.SelectedItem!);
            }
            catch (ReglaDeNegocioException ex)
            {
                error = ex.Message;
                return null;
            }
        }

        private void ActualizarVistaPrevia()
        {
            if (cmbPrecios.SelectedItem is not PreciosAAjustar)
                return;

            // El redondeo solo aplica a la venta.
            cmbRedondeo.Enabled = PreciosElegidos != PreciosAAjustar.SoloCosto;

            // Si solo se toca el costo, la vista previa muestra el costo; si no, la venta.
            var muestraCosto = PreciosElegidos == PreciosAAjustar.SoloCosto;
            colAntes.DataPropertyName = muestraCosto ? nameof(CambioDePrecio.CostoAntes) : nameof(CambioDePrecio.VentaAntes);
            colDespues.DataPropertyName = muestraCosto ? nameof(CambioDePrecio.CostoDespues) : nameof(CambioDePrecio.VentaDespues);
            colAntes.HeaderText = muestraCosto ? "Costo antes" : "Venta antes";
            colDespues.HeaderText = muestraCosto ? "Costo nuevo" : "Venta nueva";

            var cantidad = $"Se aplica a {Estilos.Productos(_productos.Count)}: {_queProductos}.\n" +
                           "Para elegir otros, marcá ✓ en la lista o cambiá los filtros de arriba (nombre, categoría o proveedor).";
            if (LeerAjuste(out var error) is not { } ajuste)
            {
                grilla.DataSource = null;
                lblResumen.Text = $"{cantidad}\n{error}";
                lblResumen.ForeColor = SystemColors.GrayText;
                return;
            }

            grilla.DataSource = _inventario.PrevisualizarAjuste(_productos, ajuste).ToList();
            lblResumen.Text = $"{cantidad}\nRevisá cómo quedan y apretá Enter para aplicar.";
            lblResumen.ForeColor = SystemColors.ControlText;
        }

        private void Aplicar()
        {
            if (LeerAjuste(out var error) is not { } ajuste)
            {
                lblResumen.Text = error;
                lblResumen.ForeColor = Estilos.Error;
                return;
            }

            var pregunta = $"¿Aplicar {ajuste.Descripcion} a {Estilos.Productos(_productos.Count)} ({_queProductos})?" +
                           "\n\nNo se puede deshacer automáticamente.";
            if (!Mensajes.Confirmar(this, pregunta))
                return;

            try
            {
                var actualizados = _inventario.AplicarAjuste(_productos.Select(p => p.Id).ToList(), ajuste);
                Aplicado?.Invoke(this, actualizados);
            }
            catch (ReglaDeNegocioException ex)
            {
                lblResumen.Text = ex.Message;
                lblResumen.ForeColor = Estilos.Error;
            }
        }
    }
}

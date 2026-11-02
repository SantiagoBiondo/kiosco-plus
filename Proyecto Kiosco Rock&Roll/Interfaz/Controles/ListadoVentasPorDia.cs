using System.Diagnostics;
using KioscoRockAndRoll.Dominio.Entidades;
using KioscoRockAndRoll.Dominio.Servicios;
using KioscoRockAndRoll.Reportes;

namespace Proyecto_Kiosco_Rock_Roll.Interfaz.Controles
{
    /// <summary>
    /// Ventas de un día con totales por medio de pago. Las anuladas se ven en gris y no suman.
    /// Enter o doble clic sobre una venta pide ver su detalle.
    /// A la derecha, cuánto se vendió ese día de un rubro y/o proveedor (por ejemplo, Cigarrillos o Arcor).
    /// </summary>
    public class ListadoVentasPorDia : UserControl
    {
        private readonly VentaServicio _ventas;
        private readonly ExportadorDeReportes _exportador;
        private readonly ReporteDeVentasServicio _reportes;
        private readonly CatalogoServicio<Categoria> _categorias;
        private readonly CatalogoServicio<Proveedor> _proveedores;
        private bool _cargandoFiltros;

        private readonly ComboBox cmbRubro = Estilos.Opciones<Opcion<FiltroClasificacion>>([], o => o.Texto, 200);
        private readonly ComboBox cmbProveedor = Estilos.Opciones<Opcion<FiltroClasificacion>>([], o => o.Texto, 200);
        private readonly DataGridView grillaVendido = new();
        private readonly Label lblVendido = new() { Dock = DockStyle.Bottom, AutoSize = true, Padding = new Padding(8), Font = Estilos.Grande, MaximumSize = new Size(640, 0) };

        private readonly DateTimePicker dtpDia = new() { Format = DateTimePickerFormat.Long, Width = 300 };
        private readonly DataGridView grilla = new();
        private readonly Label lblResumen = new() { Dock = DockStyle.Bottom, AutoSize = true, Padding = new Padding(8), Font = Estilos.Grande };
        private readonly Label lblEstado = new() { AutoSize = true, Margin = new Padding(12, 14, 3, 3), MaximumSize = new Size(700, 0) };

        /// <summary>Se eligió una venta para ver su detalle (o anularla).</summary>
        public event EventHandler<ResumenVenta>? VentaElegida;

        public ListadoVentasPorDia(VentaServicio ventas, ExportadorDeReportes exportador, ReporteDeVentasServicio reportes,
            CatalogoServicio<Categoria> categorias, CatalogoServicio<Proveedor> proveedores)
        {
            _ventas = ventas;
            _exportador = exportador;
            _reportes = reportes;
            _categorias = categorias;
            _proveedores = proveedores;
            Font = Estilos.Normal;

            var arriba = new FlowLayoutPanel { Dock = DockStyle.Top, AutoSize = true, Padding = new Padding(6) };
            arriba.Controls.Add(new Label { Text = "Día:", AutoSize = true, Margin = new Padding(3, 6, 3, 3) });
            arriba.Controls.Add(dtpDia);
            arriba.Controls.Add(Estilos.Boton("Hoy", (_, _) => dtpDia.Value = DateTime.Today, 80));
            arriba.Controls.Add(Estilos.Boton("Ver detalle / anular (Enter)", (_, _) => Elegir(), 260));
            arriba.Controls.Add(Estilos.Boton("Exportar PDF…", (_, _) => ExportarPdf(), 160));
            arriba.Controls.Add(lblEstado);

            Estilos.ConfigurarGrilla(grilla);
            grilla.ReadOnly = true;
            Estilos.Columna(grilla, nameof(ResumenVenta.Id), "Nº", "N0", 80);
            Estilos.Columna(grilla, nameof(ResumenVenta.Fecha), "Hora", "HH:mm", 90);
            Estilos.Columna(grilla, nameof(ResumenVenta.Unidades), "Unidades", "N0", 100);
            Estilos.Columna(grilla, nameof(ResumenVenta.MedioDePagoDescripcion), "Medio de pago", llenar: true);
            Estilos.Columna(grilla, nameof(ResumenVenta.Estado), "Estado", ancho: 110);
            Estilos.Columna(grilla, nameof(ResumenVenta.Total), "Total", "C2", 150);

            // El estilo se aplica al formatear cada celda (no al pintar la fila): cambiar el estilo
            // de una fila mientras se pinta la vuelve a invalidar y la grilla queda repintándose sin fin.
            grilla.CellFormatting += (_, e) =>
            {
                if (e.RowIndex >= 0 && grilla.Rows[e.RowIndex].DataBoundItem is ResumenVenta { Anulada: true })
                {
                    e.CellStyle!.ForeColor = SystemColors.GrayText;
                    e.CellStyle.Font = Estilos.Tachada;
                }
            };
            grilla.CellDoubleClick += (_, e) => { if (e.RowIndex >= 0) Elegir(); };

            // Lo vendido de un rubro o proveedor en el día.
            var vendido = new Panel { Dock = DockStyle.Right, Width = 660, Padding = new Padding(8, 0, 0, 0) };
            var filtros = new FlowLayoutPanel { Dock = DockStyle.Top, AutoSize = true };
            var titulo = Estilos.Etiqueta("Lo vendido por rubro o proveedor", Estilos.Titulo);
            filtros.Controls.Add(titulo);
            filtros.SetFlowBreak(titulo, true);
            filtros.Controls.Add(new Label { Text = "Rubro:", AutoSize = true, Margin = new Padding(3, 8, 3, 3) });
            filtros.Controls.Add(cmbRubro);
            filtros.Controls.Add(new Label { Text = "Proveedor:", AutoSize = true, Margin = new Padding(12, 8, 3, 3) });
            filtros.Controls.Add(cmbProveedor);

            Estilos.ConfigurarGrilla(grillaVendido);
            grillaVendido.ReadOnly = true;
            grillaVendido.TabStop = false;
            Estilos.Columna(grillaVendido, nameof(RendimientoProducto.Nombre), "Producto", llenar: true);
            Estilos.Columna(grillaVendido, nameof(RendimientoProducto.Unidades), "Unid.", "N0", 80);
            Estilos.Columna(grillaVendido, nameof(RendimientoProducto.Vendido), "Vendido", "C2", 130);
            Estilos.Columna(grillaVendido, nameof(RendimientoProducto.Ganancia), "Ganancia", "C2", 130);

            vendido.Controls.Add(grillaVendido);
            vendido.Controls.Add(lblVendido);
            vendido.Controls.Add(filtros);

            Controls.Add(grilla);
            Controls.Add(vendido);
            Controls.Add(lblResumen);
            Controls.Add(arriba);

            dtpDia.ValueChanged += (_, _) => { lblEstado.Text = ""; Cargar(); };
            cmbRubro.SelectedIndexChanged += (_, _) => { if (!_cargandoFiltros) CargarVendido(); };
            cmbProveedor.SelectedIndexChanged += (_, _) => { if (!_cargandoFiltros) CargarVendido(); };
        }

        public void Cargar()
        {
            var seleccionada = (grilla.CurrentRow?.DataBoundItem as ResumenVenta)?.Id;
            var resumen = _ventas.ResumenDelDia(DateOnly.FromDateTime(dtpDia.Value));
            var lista = resumen.Ventas.ToList();
            grilla.DataSource = lista;

            var indice = lista.FindIndex(v => v.Id == seleccionada);
            if (indice >= 0)
                grilla.CurrentCell = grilla.Rows[indice].Cells[0];

            var porMedio = string.Join("     ",
                resumen.TotalPorMedio.Select(par => $"{par.Key.Descripcion()}: {par.Value:C2}"));
            var anuladas = resumen.CantidadAnuladas > 0 ? $"  ({resumen.CantidadAnuladas} anuladas, no suman)" : "";

            lblResumen.Text = $"{resumen.CantidadVigentes} ventas  ·  TOTAL {resumen.Total:C2}{anuladas}" +
                              (porMedio == "" ? "" : $"\n{porMedio}");

            RecargarFiltros();
            CargarVendido();
        }

        private static FiltroClasificacion Elegido(ComboBox combo) =>
            (combo.SelectedItem as Opcion<FiltroClasificacion>)?.Valor ?? FiltroClasificacion.Todos;

        /// <summary>Vuelve a leer rubros y proveedores (por si se agregó alguno) sin perder lo elegido.</summary>
        private void RecargarFiltros()
        {
            var rubroElegido = Elegido(cmbRubro);
            var proveedorElegido = Elegido(cmbProveedor);
            _cargandoFiltros = true;
            Estilos.CambiarOpciones(cmbRubro, OpcionesDeCatalogo.ParaFiltrar(_categorias.Listar(), "Todos", "Sin rubro"));
            Estilos.CambiarOpciones(cmbProveedor, OpcionesDeCatalogo.ParaFiltrar(_proveedores.Listar(), "Todos", "Sin proveedor"));
            Estilos.Elegir<Opcion<FiltroClasificacion>>(cmbRubro, o => o.Valor == rubroElegido);
            Estilos.Elegir<Opcion<FiltroClasificacion>>(cmbProveedor, o => o.Valor == proveedorElegido);
            _cargandoFiltros = false;
        }

        /// <summary>Lo vendido en el día del rubro y proveedor elegidos, con el total abajo.</summary>
        private void CargarVendido()
        {
            var vendidos = _reportes.VendidoPor(PeriodoReporte.Dia(DateOnly.FromDateTime(dtpDia.Value)),
                Elegido(cmbRubro), Elegido(cmbProveedor));
            grillaVendido.DataSource = vendidos.ToList();

            var filtro = new List<string>();
            if (Elegido(cmbRubro) != FiltroClasificacion.Todos)
                filtro.Add(((Opcion<FiltroClasificacion>)cmbRubro.SelectedItem!).Texto);
            if (Elegido(cmbProveedor) != FiltroClasificacion.Todos)
                filtro.Add(((Opcion<FiltroClasificacion>)cmbProveedor.SelectedItem!).Texto);
            var de = filtro.Count == 0 ? "Todo" : string.Join(" de ", filtro);

            var unidades = vendidos.Sum(v => v.Unidades);
            lblVendido.Text = vendidos.Count == 0
                ? $"{de}: no se vendió nada este día."
                : $"{de}: {unidades} {(unidades == 1 ? "unidad" : "unidades")}  ·  {vendidos.Sum(v => v.Vendido):C2}\n" +
                  $"Ganancia: {vendidos.Sum(v => v.Ganancia):C2}";
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == Keys.Enter && grilla.Focused)
            {
                Elegir();
                return true;
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }

        private void Elegir()
        {
            if (grilla.CurrentRow?.DataBoundItem is ResumenVenta venta)
                VentaElegida?.Invoke(this, venta);
        }

        private void ExportarPdf()
        {
            var dia = DateOnly.FromDateTime(dtpDia.Value);
            Directory.CreateDirectory(ExportadorDeReportes.CarpetaPorDefecto);
            using var dialogo = new SaveFileDialog
            {
                Title = "Guardar las ventas del día en PDF",
                Filter = "PDF (*.pdf)|*.pdf",
                InitialDirectory = ExportadorDeReportes.CarpetaPorDefecto,
                FileName = ExportadorDeReportes.NombreDeArchivo(dia)
            };
            if (dialogo.ShowDialog(this) != DialogResult.OK)
                return;

            try
            {
                var ruta = _exportador.GuardarVentasDelDia(dia, dialogo.FileName);
                lblEstado.Text = $"PDF guardado: {Path.GetFileName(ruta)}";
                lblEstado.ForeColor = Estilos.Exito;
                Process.Start(new ProcessStartInfo { FileName = ruta, UseShellExecute = true });
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
            {
                // Lo más común: el PDF está abierto en otro programa y no se puede reemplazar.
                lblEstado.Text = $"No se pudo guardar el PDF: {ex.Message}";
                lblEstado.ForeColor = Estilos.Error;
            }
        }
    }
}

using System.Diagnostics;
using KioscoRockAndRoll.Dominio;
using KioscoRockAndRoll.Dominio.Entidades;
using KioscoRockAndRoll.Dominio.Servicios;
using KioscoRockAndRoll.Reportes;

namespace Proyecto_Kiosco_Rock_Roll.Interfaz.Vistas
{
    /// <summary>
    /// Ventas y ganancia de un período, agrupadas por día o por mes, y el ranking de productos.
    /// Los controles están en VistaReportes.Designer.cs (se editan con el diseñador de Visual Studio).
    /// </summary>
    public partial class VistaReportes : Vista
    {
        /// <summary>Un período rápido de la lista; Calcular es null para "Personalizado".</summary>
        private sealed record OpcionPeriodo(string Texto, Func<DateOnly, PeriodoReporte>? Calcular);

        private static readonly OpcionPeriodo[] Periodos =
        [
            new("Hoy", PeriodoReporte.Dia),
            new("Ayer", hoy => PeriodoReporte.Dia(hoy.AddDays(-1))),
            new("Esta semana", PeriodoReporte.SemanaHasta),
            new("Este mes", PeriodoReporte.MesHasta),
            new("Mes anterior", PeriodoReporte.MesAnterior),
            new("Este año", PeriodoReporte.AnioHasta),
            new("Personalizado", null)
        ];

        private readonly ReporteDeVentasServicio _reportes;
        private readonly ExportadorDeReportes _exportador;
        private readonly TimeProvider _reloj;
        private ReporteDeVentas? _reporte;
        private bool _actualizandoFechas;

        public VistaReportes(ReporteDeVentasServicio reportes, ExportadorDeReportes exportador, TimeProvider reloj)
        {
            _reportes = reportes;
            _exportador = exportador;
            _reloj = reloj;

            InitializeComponent();

            // Lo que el diseñador no puede hacer: las grillas y las opciones de cada lista.
            Estilos.ConfigurarGrilla(grillaPeriodos);
            Estilos.ConfigurarGrilla(grillaProductos);
            Estilos.PrepararOpciones(cmbPeriodo, Periodos, p => p.Texto);
            Estilos.PrepararOpciones(cmbAgrupar, Enum.GetValues<Agrupacion>(), a => a == Agrupacion.PorMes ? "Por mes" : "Por día");
            Estilos.PrepararOpciones(cmbOrden, Enum.GetValues<OrdenProductos>(), a => a switch
            {
                OrdenProductos.PorUnidades => "Unidades",
                OrdenProductos.PorGanancia => "Ganancia",
                _ => "Recaudación"
            });
        }

        // ---------- Lo que pasa al tocar cada cosa (conectado en el diseñador) ----------

        private void CmbPeriodo_SelectedIndexChanged(object? sender, EventArgs e) => AplicarPeriodoRapido();

        private void Fecha_ValueChanged(object? sender, EventArgs e) => FechasCambiadasAMano();

        private void CmbAgrupar_SelectedIndexChanged(object? sender, EventArgs e) => Cargar();

        private void CmbOrden_SelectedIndexChanged(object? sender, EventArgs e) => MostrarProductos();

        private void BtnExportar_Click(object? sender, EventArgs e) => ExportarPdf();

        private void GrillaPeriodos_CellFormatting(object? sender, DataGridViewCellFormattingEventArgs e) =>
            FormatearFila(grillaPeriodos, e);

        private void GrillaProductos_CellFormatting(object? sender, DataGridViewCellFormattingEventArgs e) =>
            FormatearFila(grillaProductos, e);

        public override string Titulo => "Reportes";

        private bool _yaSeMostro;

        public override void AlMostrar()
        {
            // La opción inicial se elige recién acá: las listas enlazadas a datos se llenan
            // cuando el control ya está dentro de la ventana, no en el constructor.
            if (!_yaSeMostro)
            {
                _yaSeMostro = true;
                Estilos.Elegir<OpcionPeriodo>(cmbPeriodo, p => p.Texto == "Este mes");
            }

            // Los períodos rápidos se recalculan por si cambió el día desde la última vez.
            AplicarPeriodoRapido();
            grillaPeriodos.Focus();
        }

        private Agrupacion AgrupacionElegida => cmbAgrupar.SelectedItem is Agrupacion a ? a : Agrupacion.PorDia;

        private OrdenProductos OrdenElegido => cmbOrden.SelectedItem is OrdenProductos o ? o : OrdenProductos.PorRecaudacion;

        private DateOnly Hoy => DateOnly.FromDateTime(_reloj.GetLocalNow().DateTime);

        private void AplicarPeriodoRapido()
        {
            if (cmbPeriodo.SelectedItem is not OpcionPeriodo { Calcular: { } calcular })
            {
                Cargar();
                return;
            }

            var periodo = calcular(Hoy);
            _actualizandoFechas = true;
            dtpDesde.Value = periodo.Desde.ToDateTime(TimeOnly.MinValue);
            dtpHasta.Value = periodo.Hasta.ToDateTime(TimeOnly.MinValue);
            _actualizandoFechas = false;

            // Para un año entero se ve mejor por mes; para lo demás, por día.
            cmbAgrupar.SelectedItem = periodo.Hasta.DayNumber - periodo.Desde.DayNumber > 62 ? Agrupacion.PorMes : Agrupacion.PorDia;
            Cargar();
        }

        private void FechasCambiadasAMano()
        {
            if (_actualizandoFechas)
                return;
            Estilos.Elegir<OpcionPeriodo>(cmbPeriodo, p => p.Calcular is null);
        }

        private void Cargar()
        {
            try
            {
                var periodo = new PeriodoReporte(DateOnly.FromDateTime(dtpDesde.Value), DateOnly.FromDateTime(dtpHasta.Value));
                _reporte = _reportes.Generar(periodo, AgrupacionElegida);
                MostrarEstado("", SystemColors.ControlText);
            }
            catch (ReglaDeNegocioException ex)
            {
                _reporte = null;
                MostrarEstado(ex.Message, Estilos.Error);
            }

            lblVendido.Text = _reporte?.Vendido.ToString("C2") ?? "-";
            lblCosto.Text = _reporte?.Costo.ToString("C2") ?? "-";
            lblGanancia.Text = _reporte?.Ganancia.ToString("C2") ?? "-";
            lblGanancia.ForeColor = _reporte?.Ganancia < 0 ? Estilos.Error : Estilos.Exito;
            lblPorcentaje.Text = _reporte?.PorcentajeSobreCosto?.ToString("P0") ?? "-";
            lblVentas.Text = _reporte?.CantidadVentas.ToString("N0") ?? "-";
            grillaPeriodos.DataSource = _reporte?.Filas.ToList();
            MostrarProductos();
        }

        private void MostrarProductos() =>
            grillaProductos.DataSource = _reporte?.ProductosOrdenados(OrdenElegido).ToList();

        // Pérdidas en rojo; períodos sin ventas, en gris.
        private static void FormatearFila(DataGridView grilla, DataGridViewCellFormattingEventArgs e)
        {
            if (e.RowIndex < 0 || grilla.Rows[e.RowIndex].DataBoundItem is not ConGanancia fila)
                return;
            if (fila.Vendido == 0)
                e.CellStyle!.ForeColor = SystemColors.GrayText;
            else if (grilla.Columns[e.ColumnIndex].DataPropertyName == nameof(ConGanancia.Ganancia) && fila.Ganancia < 0)
                e.CellStyle!.ForeColor = Estilos.Error;
        }

        private void ExportarPdf()
        {
            if (_reporte is null)
                return;

            Directory.CreateDirectory(ExportadorDeReportes.CarpetaPorDefecto);
            using var dialogo = new SaveFileDialog
            {
                Title = "Guardar el reporte en PDF",
                Filter = "PDF (*.pdf)|*.pdf",
                InitialDirectory = ExportadorDeReportes.CarpetaPorDefecto,
                FileName = ExportadorDeReportes.NombreDeArchivo(_reporte.Periodo)
            };
            if (dialogo.ShowDialog(this) != DialogResult.OK)
                return;

            try
            {
                var ruta = _exportador.GuardarReporte(_reporte, OrdenElegido, dialogo.FileName);
                MostrarEstado($"PDF guardado: {Path.GetFileName(ruta)}", Estilos.Exito);
                Process.Start(new ProcessStartInfo { FileName = ruta, UseShellExecute = true });
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
            {
                MostrarEstado($"No se pudo guardar el PDF: {ex.Message}", Estilos.Error);
            }
        }

        private void MostrarEstado(string texto, Color color)
        {
            lblEstado.Text = texto;
            lblEstado.ForeColor = color;
        }
    }
}

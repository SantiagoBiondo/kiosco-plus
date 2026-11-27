using KioscoRockAndRoll.Dominio.Entidades;
using KioscoRockAndRoll.Dominio.Servicios;
using Proyecto_Kiosco_Rock_Roll.Interfaz.Controles;

namespace Proyecto_Kiosco_Rock_Roll.Interfaz.Vistas
{
    /// <summary>
    /// Caja: abrir y cerrar el turno, registrar ingresos y retiros de efectivo,
    /// ver los cierres anteriores y las ventas de cualquier día.
    /// Los controles están en VistaCaja.Designer.cs (se editan con el diseñador de Visual Studio);
    /// el panel de la derecha, lo que se abre en él y el listado de ventas por día se agregan acá
    /// porque los arma el programa.
    /// </summary>
    public partial class VistaCaja : Vista
    {
        private readonly CajaServicio _caja;

        private readonly PanelLateral panelLateral = new(ancho: 500);
        private readonly PanelAbrirCaja panelAbrir;
        private readonly PanelMovimientoCaja panelMovimiento;
        private readonly PanelCerrarCaja panelCerrar;
        private readonly PanelDetalleVenta panelDetalle;
        private readonly ListadoVentasPorDia ventasPorDia;

        // Las filas de las cuentas que solo tienen sentido con la caja ya cerrada.
        private readonly Control[] filasDeCierre;

        public VistaCaja(CajaServicio caja, ListadoVentasPorDia ventasPorDia,
            PanelAbrirCaja panelAbrir, PanelMovimientoCaja panelMovimiento, PanelCerrarCaja panelCerrar,
            PanelDetalleVenta panelDetalle)
        {
            _caja = caja;
            this.ventasPorDia = ventasPorDia;
            this.panelAbrir = panelAbrir;
            this.panelMovimiento = panelMovimiento;
            this.panelCerrar = panelCerrar;
            this.panelDetalle = panelDetalle;

            InitializeComponent();
            Controls.Add(panelLateral);

            // Lo que el diseñador no puede hacer: las grillas y el listado de ventas por día.
            Estilos.ConfigurarGrilla(grillaMovimientos);
            Estilos.ConfigurarGrilla(grillaTurnos);
            ventasPorDia.Dock = DockStyle.Fill;
            tabVentas.Controls.Add(ventasPorDia);
            filasDeCierre = [lblConceptoContado, lblContado, lblConceptoDiferencia, lblDiferencia];

            panelAbrir.Abierta += (_, _) => CerrarPanel();
            panelAbrir.Cancelado += (_, _) => CerrarPanel();
            panelMovimiento.Registrado += (_, _) => CerrarPanel();
            panelMovimiento.Cancelado += (_, _) => CerrarPanel();
            panelCerrar.Cerrada += (_, cierre) =>
            {
                CerrarPanel();
                if (cierre.RutaPdf is { } ruta)
                    MostrarAviso($"PDF guardado en {ruta}", Estilos.Exito);
                else if (cierre.ErrorPdf is { } error)
                    MostrarAviso($"La caja se cerró, pero no se pudo guardar el PDF: {error}", Estilos.Error);
            };
            panelCerrar.Cancelado += (_, _) => CerrarPanel();

            // Detalle y anulación desde la pestaña "Ventas por día": al terminar se sigue en esa pestaña.
            ventasPorDia.VentaElegida += (_, venta) => AbrirPanel(panelDetalle, () => panelDetalle.Mostrar(venta.Id));
            panelDetalle.Anulada += (_, _) => CerrarPanel(volverAlTurno: false);
            panelDetalle.Cerrado += (_, _) => CerrarPanel(volverAlTurno: false);
        }

        public override string Titulo => "Caja";
        public override Keys Atajo => Keys.F4;

        public override void AlMostrar()
        {
            Cargar();
            if (panelLateral.Contenido is { } panel)
                panel.SelectNextControl(panel, true, true, true, true);
            else
                pestanias.Focus();
        }

        // ---------- Lo que pasa al tocar cada cosa (conectado en el diseñador) ----------

        private void BtnAbrir_Click(object? sender, EventArgs e) => AbrirPanel(panelAbrir, () => panelAbrir.Iniciar());

        private void BtnIngreso_Click(object? sender, EventArgs e) =>
            AbrirPanel(panelMovimiento, () => panelMovimiento.Iniciar(TipoMovimientoCaja.Ingreso));

        private void BtnRetiro_Click(object? sender, EventArgs e) =>
            AbrirPanel(panelMovimiento, () => panelMovimiento.Iniciar(TipoMovimientoCaja.Retiro));

        private void BtnCerrar_Click(object? sender, EventArgs e) => AbrirPanel(panelCerrar, panelCerrar.Iniciar);

        private void Pestanias_SelectedIndexChanged(object? sender, EventArgs e) => Cargar();

        private void Cargar()
        {
            var abierto = _caja.TurnoAbierto;
            btnAbrir.Enabled = abierto is null;
            btnIngreso.Enabled = btnRetiro.Enabled = btnCerrar.Enabled = abierto is not null;

            var anteriores = _caja.TurnosAnteriores();
            lblEstado.Text = abierto is not null
                ? $"Caja abierta desde el {abierto.Apertura:dd/MM HH:mm}"
                : "Caja cerrada";
            lblEstado.ForeColor = abierto is not null ? Estilos.Exito : Estilos.Advertencia;

            // Si está abierta se ven las cuentas del turno; si no, las del último cierre.
            var resumen = abierto is not null ? _caja.Resumen(abierto) : anteriores.FirstOrDefault();
            MostrarResumen(resumen);

            switch (pestanias.SelectedIndex)
            {
                case 1: grillaTurnos.DataSource = anteriores.ToList(); break;
                case 2: ventasPorDia.Cargar(); break;
            }
        }

        private void MostrarResumen(ResumenTurno? resumen)
        {
            if (resumen is null)
            {
                lblTituloResumen.Text = "Todavía no hay turnos. Abrí la caja para empezar.";
                foreach (var valor in new[] { lblInicial, lblVentasEfectivo, lblIngresos, lblRetiros, lblEsperado, lblOtrosMedios, lblTotalVendido })
                    valor.Text = "-";
                foreach (var fila in filasDeCierre)
                    fila.Visible = false;
                grillaMovimientos.DataSource = null;
                return;
            }

            var cerrado = !resumen.Turno.EstaAbierto;
            lblTituloResumen.Text = cerrado
                ? $"Último turno ({resumen.Apertura:dd/MM HH:mm} a {resumen.Cierre:HH:mm})"
                : "Turno actual";
            lblInicial.Text = resumen.EfectivoInicial.ToString("C2");
            lblVentasEfectivo.Text = $"{resumen.VentasEnEfectivo:C2}";
            lblIngresos.Text = resumen.Ingresos.ToString("C2");
            lblRetiros.Text = resumen.Retiros.ToString("C2");
            lblEsperado.Text = resumen.EfectivoEsperado.ToString("C2");

            foreach (var fila in filasDeCierre)
                fila.Visible = cerrado;
            if (cerrado)
            {
                lblContado.Text = resumen.EfectivoContado?.ToString("C2");
                var diferencia = resumen.Diferencia ?? 0;
                lblDiferencia.Text = ResumenTurno.DescribirDiferencia(diferencia);
                lblDiferencia.ForeColor = diferencia == 0 ? Estilos.Exito : diferencia > 0 ? Estilos.Advertencia : Estilos.Error;
            }

            var otros = resumen.Ventas.Where(v => v.Medio != MedioDePago.Efectivo).ToList();
            lblOtrosMedios.Text = otros.Count == 0
                ? "-"
                : string.Join("\n", otros.Select(v => $"{v.Medio.Descripcion()}: {v.Total:C2}"));
            lblTotalVendido.Text = $"{resumen.TotalVendido:C2}  ({resumen.CantidadVentas} ventas)";

            grillaMovimientos.DataSource = resumen.Movimientos.ToList();
        }

        private void FormatearDiferencia(object? sender, DataGridViewCellFormattingEventArgs e)
        {
            if (grillaTurnos.Columns[e.ColumnIndex].DataPropertyName != nameof(ResumenTurno.Diferencia) || e.Value is not decimal diferencia)
                return;
            e.Value = ResumenTurno.DescribirDiferencia(diferencia);
            e.CellStyle!.ForeColor = diferencia == 0 ? Estilos.Exito : diferencia > 0 ? Estilos.Advertencia : Estilos.Error;
            e.FormattingApplied = true;
        }

        private void MostrarAviso(string texto, Color color)
        {
            lblAviso.Text = texto;
            lblAviso.ForeColor = color;
        }

        private void AbrirPanel(Control panel, Action iniciar)
        {
            MostrarAviso("", SystemColors.ControlText);
            panelLateral.Mostrar(panel);
            iniciar();
        }

        private void CerrarPanel(bool volverAlTurno = true)
        {
            panelLateral.Ocultar();
            if (volverAlTurno)
                pestanias.SelectedTab = tabTurno;
            Cargar();
            pestanias.SelectedTab?.SelectNextControl(pestanias.SelectedTab, true, true, true, true);
        }
    }
}

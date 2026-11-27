using KioscoRockAndRoll.Datos;
using KioscoRockAndRoll.Dominio.Entidades;
using KioscoRockAndRoll.Dominio.Servicios;
using Proyecto_Kiosco_Rock_Roll.Interfaz.Impresion;

namespace Proyecto_Kiosco_Rock_Roll.Interfaz.Vistas
{
    /// <summary>
    /// Configuración: la impresora de tickets (cuál usar, de qué ancho es el papel y si se imprime
    /// siempre al cobrar), la apariencia (tema claro u oscuro) y las reglas para cobrar por transferencia
    /// (recargo por paquete de cada rubro y monto mínimo). Los cambios se guardan apenas se hacen.
    /// <para>
    /// Los controles y su ubicación están en VistaConfiguracion.Designer.cs, así se puede abrir y editar
    /// con el diseñador de Visual Studio. Acá queda lo que el diseñador no puede hacer: cargar las listas
    /// (impresoras, rubros, temas) y lo que pasa al tocar cada cosa. Las tres columnas tienen Dock = Left:
    /// la que se agrega última en el diseñador queda más a la izquierda. La vista previa del ticket es
    /// siempre blanca con letra negra, también en el tema oscuro, porque representa el papel.
    /// </para>
    /// </summary>
    public partial class VistaConfiguracion : Vista
    {
        private const string Ninguna = "(ninguna: no imprimir tickets)";

        private readonly AjustesDeTicket _ajustes;
        private readonly AjustesDeApariencia _apariencia;
        private readonly AjustesDeCobro _cobro;
        private readonly CatalogoServicio<Categoria> _categorias;
        private bool _cargando;

        // Los rubros con recargo por transferencia, cada uno con su monto por paquete.
        private List<RecargoDeRubro> _recargos = [];
        private Dictionary<long, string> _nombreDeRubro = [];

        public VistaConfiguracion(AjustesDeTicket ajustes, AjustesDeApariencia apariencia,
            AjustesDeCobro cobro, CatalogoServicio<Categoria> categorias)
        {
            _ajustes = ajustes;
            _apariencia = apariencia;
            _cobro = cobro;
            _categorias = categorias;

            InitializeComponent();

            // Cargar las listas dispara sus eventos de cambio: mientras tanto no se guarda nada.
            _cargando = true;
            Estilos.PrepararOpciones<Opcion<string?>>(cmbImpresora, [], o => o.Texto);
            Estilos.PrepararOpciones(cmbPapel, Enum.GetValues<AnchoDePapel>(),
                a => a == AnchoDePapel.Rollo80mm ? "Rollo de 80 mm" : "Rollo de 58 mm");
            Estilos.PrepararOpciones(cmbTema, new[] { false, true }, oscuro => oscuro ? "Oscuro" : "Claro");
            Estilos.PrepararOpciones<Opcion<long>>(cmbRubroRecargo, [], o => o.Texto);
            _cargando = false;

            lstRecargos.Format += (_, e) =>
            {
                if (e.ListItem is RecargoDeRubro r)
                    e.Value = $"{NombreDeRubro(r.CategoriaId)}:  + {r.PorPaquete:C0} por paquete";
            };
        }

        // ---------- Lo que pasa al tocar cada cosa (conectado en el diseñador) ----------

        private void BtnActualizarLista_Click(object? sender, EventArgs e) => CargarImpresoras();

        private void BtnImprimirPrueba_Click(object? sender, EventArgs e) => ImprimirPrueba();

        private void BtnReiniciar_Click(object? sender, EventArgs e) => Application.Restart();

        private void CmbImpresora_SelectedIndexChanged(object? sender, EventArgs e) => Guardar();

        private void CmbPapel_SelectedIndexChanged(object? sender, EventArgs e) => Guardar();

        private void ChkImprimirSiempre_CheckedChanged(object? sender, EventArgs e) => Guardar();

        private void CmbTema_SelectedIndexChanged(object? sender, EventArgs e) => CambiarTema();

        private void BtnAgregarRecargo_Click(object? sender, EventArgs e) => AgregarRecargo();

        private void BtnQuitarRecargo_Click(object? sender, EventArgs e) => QuitarRecargo();

        private void NumMinimo_ValueChanged(object? sender, EventArgs e) => GuardarCobro();

        // Al elegir un rubro que ya tiene recargo, se muestra su monto (así "Agregar" lo cambia).
        private void CmbRubroRecargo_SelectedIndexChanged(object? sender, EventArgs e)
        {
            if (!_cargando && cmbRubroRecargo.SelectedItem is Opcion<long> rubro
                && _recargos.FirstOrDefault(r => r.CategoriaId == rubro.Valor) is { } existente)
                numRecargo.Value = Math.Min(existente.PorPaquete, numRecargo.Maximum);
        }

        public override string Titulo => "Configuración";

        public override void AlMostrar()
        {
            CargarImpresoras();
            CargarTema();
            CargarCobro();
            cmbImpresora.Focus();
        }

        private void CargarTema()
        {
            _cargando = true;
            cmbTema.SelectedItem = _apariencia.TemaOscuro;
            _cargando = false;
            MostrarAvisoDeTema();
        }

        private void CambiarTema()
        {
            if (_cargando || cmbTema.SelectedItem is not bool oscuro)
                return;
            _apariencia.TemaOscuro = oscuro;
            MostrarAvisoDeTema();
        }

        // El tema se aplica al abrir el programa: si el elegido no es el que se está usando, hay que reiniciar.
        private void MostrarAvisoDeTema()
        {
            var hayQueReiniciar = _apariencia.TemaOscuro != Estilos.Oscuro;
            lblAvisoTema.Text = hayQueReiniciar
                ? "Guardado. El cambio se ve al reiniciar el programa (no se pierde nada)."
                : "Los colores del programa. Si te cansa la vista o la pantalla es muy brillante, probá el oscuro.";
            lblAvisoTema.ForeColor = hayQueReiniciar ? Estilos.Advertencia : SystemColors.GrayText;
            btnReiniciar.Visible = hayQueReiniciar;
        }

        /// <summary>Muestra las reglas de cobro guardadas (los rubros se recargan por si se agregó alguno).</summary>
        private void CargarCobro()
        {
            var reglas = _cobro.Leer();
            var rubros = _categorias.Listar();
            _nombreDeRubro = rubros.ToDictionary(c => c.Id, c => c.Nombre);
            _recargos = reglas.Recargos.ToList();

            _cargando = true;
            Estilos.CambiarOpciones(cmbRubroRecargo, rubros.Select(c => new Opcion<long>(c.Id, c.Nombre)));
            numRecargo.Value = AjustesDeCobro.RecargoPorDefecto;
            numMinimo.Value = Math.Min(reglas.MinimoTransferencia, numMinimo.Maximum);
            _cargando = false;

            MostrarRecargos();
            DescribirCobro(reglas, guardado: false);
        }

        private string NombreDeRubro(long id) => _nombreDeRubro.GetValueOrDefault(id, "(rubro borrado)");

        private void MostrarRecargos()
        {
            lstRecargos.BeginUpdate();
            lstRecargos.Items.Clear();
            foreach (var recargo in _recargos.OrderBy(r => NombreDeRubro(r.CategoriaId), StringComparer.CurrentCultureIgnoreCase))
                lstRecargos.Items.Add(recargo);
            lstRecargos.EndUpdate();
        }

        /// <summary>Agrega el rubro elegido con su recargo; si ya estaba, le cambia el monto.</summary>
        private void AgregarRecargo()
        {
            if (cmbRubroRecargo.SelectedItem is not Opcion<long> rubro)
            {
                MostrarErrorDeCobro("Primero creá los rubros en Inventario → Categorías.");
                return;
            }
            var monto = numRecargo.ValorTipeado();
            if (monto <= 0)
            {
                MostrarErrorDeCobro("Poné cuánto se suma a cada paquete.");
                return;
            }
            _recargos.RemoveAll(r => r.CategoriaId == rubro.Valor);
            _recargos.Add(new RecargoDeRubro(rubro.Valor, monto));
            MostrarRecargos();
            GuardarCobro();
        }

        private void QuitarRecargo()
        {
            if (lstRecargos.SelectedItem is not RecargoDeRubro elegido)
            {
                MostrarErrorDeCobro("Elegí en la lista el rubro que querés quitar.");
                return;
            }
            _recargos.Remove(elegido);
            MostrarRecargos();
            GuardarCobro();
        }

        private void GuardarCobro()
        {
            if (_cargando)
                return;
            var reglas = new ReglasDeCobro(_recargos, numMinimo.ValorTipeado());
            if (reglas.Iguales(_cobro.Leer()))
                return;
            _cobro.Guardar(reglas);
            DescribirCobro(reglas, guardado: true);
        }

        private void MostrarErrorDeCobro(string texto)
        {
            lblCobro.Text = texto;
            lblCobro.ForeColor = Estilos.Error;
        }

        private void DescribirCobro(ReglasDeCobro reglas, bool guardado)
        {
            var recargo = reglas.Recargos.Count == 0
                ? "Sin recargo por transferencia."
                : "Por transferencia, cada paquete sale más caro: " +
                  string.Join(", ", reglas.Recargos
                      .OrderBy(r => NombreDeRubro(r.CategoriaId), StringComparer.CurrentCultureIgnoreCase)
                      .Select(r => $"{NombreDeRubro(r.CategoriaId)} + {r.PorPaquete:C0}")) + ".";
            var minimo = reglas.MinimoTransferencia > 0
                ? $"Se puede transferir desde {reglas.MinimoTransferencia:C0}."
                : "Se puede transferir cualquier monto.";
            lblCobro.Text = (guardado ? "Guardado. " : "") + $"{recargo} {minimo}";
            lblCobro.ForeColor = guardado ? Estilos.Exito : SystemColors.GrayText;
        }

        /// <summary>Lista las impresoras de Windows y muestra la configuración guardada.</summary>
        private void CargarImpresoras()
        {
            var configuracion = _ajustes.Leer();
            var instaladas = ImpresoraDeTickets.ImpresorasInstaladas();

            var opciones = new List<Opcion<string?>> { new(null, Ninguna) };
            opciones.AddRange(instaladas.Select(i => new Opcion<string?>(i, i)));
            // La elegida puede no estar si está desenchufada: se muestra igual, para no perder la configuración.
            if (configuracion.HayImpresora && !instaladas.Contains(configuracion.Impresora))
                opciones.Add(new Opcion<string?>(configuracion.Impresora, $"{configuracion.Impresora} (no encontrada)"));

            _cargando = true;
            Estilos.CambiarOpciones(cmbImpresora, opciones);
            Estilos.Elegir<Opcion<string?>>(cmbImpresora, o => o.Valor == configuracion.Impresora);
            cmbPapel.SelectedItem = configuracion.Ancho;
            chkImprimirSiempre.Checked = configuracion.ImprimirAlCobrar;
            _cargando = false;

            MostrarVistaPrevia(configuracion);
            MostrarEstado(configuracion.HayImpresora ? "" : "Todavía no hay impresora elegida: no se imprimen tickets.",
                SystemColors.GrayText);
        }

        private ConfiguracionDeTicket ConfiguracionEnPantalla => new(
            (cmbImpresora.SelectedItem as Opcion<string?>)?.Valor,
            cmbPapel.SelectedItem is AnchoDePapel ancho ? ancho : AnchoDePapel.Rollo58mm,
            chkImprimirSiempre.Checked);

        private void Guardar()
        {
            if (_cargando)
                return;

            var configuracion = ConfiguracionEnPantalla;
            _ajustes.Guardar(configuracion);
            chkImprimirSiempre.Enabled = configuracion.HayImpresora;
            MostrarVistaPrevia(configuracion);
            MostrarEstado("Guardado.", Estilos.Exito);
        }

        private void MostrarVistaPrevia(ConfiguracionDeTicket configuracion)
        {
            chkImprimirSiempre.Enabled = configuracion.HayImpresora;
            txtVistaPrevia.Text = ImpresoraDeTickets.VistaPrevia(ImpresoraDeTickets.TicketDePrueba(), configuracion);
        }

        private void ImprimirPrueba()
        {
            try
            {
                Cursor = Cursors.WaitCursor;
                ImpresoraDeTickets.Imprimir(ImpresoraDeTickets.TicketDePrueba(), ConfiguracionEnPantalla);
                MostrarEstado("Se mandó el ticket de prueba a la impresora.", Estilos.Exito);
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

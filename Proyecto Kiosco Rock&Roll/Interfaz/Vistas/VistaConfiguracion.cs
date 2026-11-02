using KioscoRockAndRoll.Datos;
using KioscoRockAndRoll.Dominio.Entidades;
using KioscoRockAndRoll.Dominio.Servicios;
using Proyecto_Kiosco_Rock_Roll.Interfaz.Impresion;

namespace Proyecto_Kiosco_Rock_Roll.Interfaz.Vistas
{
    /// <summary>
    /// Configuración: la impresora de tickets (cuál usar, de qué ancho es el papel y si se imprime
    /// siempre al cobrar), la apariencia (tema claro u oscuro) y las reglas para cobrar por transferencia
    /// (recargo por paquete y monto mínimo). Los cambios se guardan apenas se hacen.
    /// </summary>
    public class VistaConfiguracion : Vista
    {
        private const string Ninguna = "(ninguna: no imprimir tickets)";

        private readonly AjustesDeTicket _ajustes;
        private readonly AjustesDeApariencia _apariencia;
        private readonly AjustesDeCobro _cobro;
        private readonly CatalogoServicio<Categoria> _categorias;
        private bool _cargando;

        private readonly ComboBox cmbRubroRecargo = Estilos.Opciones<Opcion<long?>>([], o => o.Texto, 260);
        private readonly NumericUpDown numRecargo = Estilos.CampoNumero(2, 1_000_000);
        private readonly NumericUpDown numMinimo = Estilos.CampoNumero(2, 1_000_000);
        private readonly Label lblCobro = new() { AutoSize = true, MaximumSize = new Size(460, 0), Margin = new Padding(3, 12, 3, 3) };

        private readonly ComboBox cmbTema = Estilos.Opciones(new[] { false, true }, oscuro => oscuro ? "Oscuro" : "Claro", 200);
        private readonly Label lblTema = new() { AutoSize = true, MaximumSize = new Size(620, 0), Margin = new Padding(3, 8, 3, 3) };
        private readonly Button btnReiniciar;

        private readonly ComboBox cmbImpresora = Estilos.Opciones<Opcion<string?>>([], o => o.Texto, 420);
        private readonly ComboBox cmbPapel = Estilos.Opciones(Enum.GetValues<AnchoDePapel>(),
            a => a == AnchoDePapel.Rollo80mm ? "Rollo de 80 mm" : "Rollo de 58 mm", 200);
        private readonly CheckBox chkImprimirSiempre = new()
        {
            Text = "Imprimir el ticket siempre al cobrar (si no, se marca la casilla cuando el cliente lo pide)",
            AutoSize = true,
            Margin = new Padding(3, 12, 3, 3)
        };
        private readonly Label lblEstado = new() { AutoSize = true, Margin = new Padding(3, 12, 3, 3), MaximumSize = new Size(700, 0) };
        private readonly TextBox txtVistaPrevia = new()
        {
            Multiline = true,
            ReadOnly = true,
            Font = new Font("Consolas", 11F),
            // Siempre blanco con letra negra, también en el tema oscuro: representa el papel del ticket.
            BackColor = Color.White,
            ForeColor = Color.Black,
            ScrollBars = ScrollBars.Vertical,
            Width = 520,
            Height = 480,
            TabStop = false
        };

        public VistaConfiguracion(AjustesDeTicket ajustes, AjustesDeApariencia apariencia,
            AjustesDeCobro cobro, CatalogoServicio<Categoria> categorias)
        {
            _ajustes = ajustes;
            _apariencia = apariencia;
            _cobro = cobro;
            _categorias = categorias;

            var opciones = new TableLayoutPanel { ColumnCount = 1, AutoSize = true, Dock = DockStyle.Left, Padding = new Padding(16) };
            opciones.Controls.Add(Estilos.Etiqueta("Impresora de tickets", Estilos.Titulo));
            opciones.Controls.Add(new Label
            {
                AutoSize = true,
                MaximumSize = new Size(620, 0),
                Text = "Elegí por qué impresora salen los tickets. Sirve cualquier impresora instalada en Windows: " +
                       "una de tickets (térmica) o una común. Los cambios se guardan solos."
            });

            opciones.Controls.Add(Estilos.Etiqueta("Impresora"));
            var filaImpresora = new FlowLayoutPanel { AutoSize = true };
            filaImpresora.Controls.Add(cmbImpresora);
            filaImpresora.Controls.Add(Estilos.Boton("Actualizar lista", (_, _) => CargarImpresoras(), 160));
            opciones.Controls.Add(filaImpresora);

            opciones.Controls.Add(Estilos.Etiqueta("Ancho del papel"));
            opciones.Controls.Add(cmbPapel);
            opciones.Controls.Add(chkImprimirSiempre);

            var btnPrueba = Estilos.Boton("Imprimir ticket de prueba", (_, _) => ImprimirPrueba(), 240);
            btnPrueba.Margin = new Padding(3, 16, 3, 3);
            opciones.Controls.Add(btnPrueba);
            opciones.Controls.Add(lblEstado);

            // Apariencia.
            var tituloApariencia = Estilos.Etiqueta("Apariencia", Estilos.Titulo);
            tituloApariencia.Margin = new Padding(3, 32, 3, 8);
            opciones.Controls.Add(tituloApariencia);
            opciones.Controls.Add(Estilos.Etiqueta("Tema"));
            opciones.Controls.Add(cmbTema);
            opciones.Controls.Add(lblTema);
            btnReiniciar = Estilos.Boton("Reiniciar ahora", (_, _) => Application.Restart(), 180);
            opciones.Controls.Add(btnReiniciar);
            cmbTema.SelectedIndexChanged += (_, _) => CambiarTema();

            var vistaPrevia = new TableLayoutPanel { ColumnCount = 1, AutoSize = true, Dock = DockStyle.Left, Padding = new Padding(16) };
            vistaPrevia.Controls.Add(Estilos.Etiqueta("Así sale el ticket", Estilos.Titulo));
            vistaPrevia.Controls.Add(txtVistaPrevia);

            // Cobro por transferencia.
            var cobroPanel = new TableLayoutPanel { ColumnCount = 1, AutoSize = true, Dock = DockStyle.Left, Padding = new Padding(16) };
            cobroPanel.Controls.Add(Estilos.Etiqueta("Cobro por transferencia", Estilos.Titulo));
            cobroPanel.Controls.Add(new Label
            {
                AutoSize = true,
                MaximumSize = new Size(460, 0),
                Text = "Si se paga algo por transferencia, cada paquete del rubro elegido se cobra más caro " +
                       "(los sueltos no). Y no se puede transferir menos que el mínimo."
            });
            cobroPanel.Controls.Add(Estilos.Etiqueta("Rubro con recargo"));
            cobroPanel.Controls.Add(cmbRubroRecargo);
            cobroPanel.Controls.Add(Estilos.Etiqueta("Recargo por paquete ($)"));
            cobroPanel.Controls.Add(numRecargo);
            cobroPanel.Controls.Add(Estilos.Etiqueta("Mínimo para transferir ($)"));
            cobroPanel.Controls.Add(numMinimo);
            cobroPanel.Controls.Add(lblCobro);

            // El orden importa con Dock = Left: lo último que se agrega queda más a la izquierda.
            Controls.Add(cobroPanel);
            Controls.Add(vistaPrevia);
            Controls.Add(opciones);

            cmbRubroRecargo.SelectedIndexChanged += (_, _) => GuardarCobro();
            foreach (var campo in new[] { numRecargo, numMinimo })
            {
                campo.ValueChanged += (_, _) => GuardarCobro();
                campo.Leave += (_, _) => GuardarCobro();
            }

            cmbImpresora.SelectedIndexChanged += (_, _) => Guardar();
            cmbPapel.SelectedIndexChanged += (_, _) => Guardar();
            chkImprimirSiempre.CheckedChanged += (_, _) => Guardar();
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
            lblTema.Text = hayQueReiniciar
                ? "Guardado. El cambio se ve al reiniciar el programa (no se pierde nada)."
                : "Los colores del programa. Si te cansa la vista o la pantalla es muy brillante, probá el oscuro.";
            lblTema.ForeColor = hayQueReiniciar ? Estilos.Advertencia : SystemColors.GrayText;
            btnReiniciar.Visible = hayQueReiniciar;
        }

        /// <summary>Muestra las reglas de cobro guardadas (las categorías se recargan por si se agregó alguna).</summary>
        private void CargarCobro()
        {
            var reglas = _cobro.Leer();
            _cargando = true;
            Estilos.CambiarOpciones(cmbRubroRecargo, OpcionesDeCatalogo.ParaAsignar(_categorias.Listar(), "(ninguno: sin recargo)"));
            Estilos.Elegir<Opcion<long?>>(cmbRubroRecargo, o => o.Valor == reglas.CategoriaConRecargo);
            numRecargo.Value = Math.Min(reglas.RecargoPorPaquete, numRecargo.Maximum);
            numMinimo.Value = Math.Min(reglas.MinimoTransferencia, numMinimo.Maximum);
            _cargando = false;
            DescribirCobro(reglas, guardado: false);
        }

        private void GuardarCobro()
        {
            if (_cargando)
                return;
            var reglas = new ReglasDeCobro(
                (cmbRubroRecargo.SelectedItem as Opcion<long?>)?.Valor, numRecargo.ValorTipeado(), numMinimo.ValorTipeado());
            if (reglas == _cobro.Leer())
                return;
            _cobro.Guardar(reglas);
            DescribirCobro(reglas, guardado: true);
        }

        private void DescribirCobro(ReglasDeCobro reglas, bool guardado)
        {
            var rubro = (cmbRubroRecargo.SelectedItem as Opcion<long?>)?.Texto;
            var recargo = reglas.CategoriaConRecargo != null && reglas.RecargoPorPaquete > 0
                ? $"Por transferencia, cada paquete de {rubro} sale {reglas.RecargoPorPaquete:C0} más."
                : "Sin recargo por transferencia.";
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

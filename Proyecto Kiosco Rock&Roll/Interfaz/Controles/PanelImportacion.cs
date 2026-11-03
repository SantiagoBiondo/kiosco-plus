using KioscoRockAndRoll.Datos;
using KioscoRockAndRoll.Dominio;
using KioscoRockAndRoll.Dominio.Entidades;
using KioscoRockAndRoll.Dominio.Servicios;

namespace Proyecto_Kiosco_Rock_Roll.Interfaz.Controles
{
    /// <summary>
    /// Importación de productos desde una planilla de otro programa (Excel .xlsx o CSV).
    /// Se elige el archivo, se revisa qué columna es cada dato (el programa lo adivina por los títulos)
    /// y se ve fila por fila qué va a pasar antes de importar. Antes de importar se guarda un backup.
    /// </summary>
    public class PanelImportacion : UserControl
    {
        /// <summary>Ancho del panel lateral mientras se importa: la vista previa necesita lugar.</summary>
        public const int AnchoSugerido = 1000;

        private static readonly Opcion<SiElCodigoYaExiste>[] OpcionesSiYaExiste =
        [
            new(SiElCodigoYaExiste.Actualizar, "Actualizar sus datos con los de la planilla"),
            new(SiElCodigoYaExiste.DejarComoEsta, "Dejarlo como está")
        ];

        private readonly ImportadorDeProductos _importador;
        private readonly GestorDeBackups _backups;
        private TablaImportada? _tabla;
        private MapeoDeColumnas _mapeo = new();
        private VistaPreviaImportacion? _vistaPrevia;
        private bool _sincronizando;

        private readonly Label lblArchivo = new() { AutoSize = true, Margin = new Padding(12, 14, 3, 3), MaximumSize = new Size(700, 0) };
        private readonly Dictionary<CampoImportable, ComboBox> combos = [];
        private readonly ComboBox cmbSiYaExiste = Estilos.Opciones(OpcionesSiYaExiste, o => o.Texto, 380);
        private readonly TableLayoutPanel zonaColumnas = new() { ColumnCount = 4, AutoSize = true, Margin = new Padding(0, 8, 0, 0) };
        private readonly Label lblResumen = new() { AutoSize = true, Font = Estilos.Grande, Margin = new Padding(3, 10, 3, 6), MaximumSize = new Size(940, 0) };
        private readonly DataGridView grilla = new();
        private readonly Button btnImportar;
        private readonly Label lblEstado = new() { AutoSize = true, Margin = new Padding(12, 14, 3, 3), MaximumSize = new Size(560, 0) };

        /// <summary>Se importó: el resultado (cuántos nuevos, cuántos actualizados).</summary>
        public event EventHandler<ResultadoImportacion>? Importado;

        public event EventHandler? Cerrado;

        public PanelImportacion(ImportadorDeProductos importador, GestorDeBackups backups)
        {
            _importador = importador;
            _backups = backups;
            Font = Estilos.Normal;

            var arriba = new TableLayoutPanel { ColumnCount = 1, AutoSize = true, Dock = DockStyle.Top };
            arriba.Controls.Add(Estilos.Etiqueta("Importar productos de otro programa", Estilos.Titulo));
            arriba.Controls.Add(new Label
            {
                AutoSize = true,
                MaximumSize = new Size(940, 0),
                Text = "Exportá los productos del programa anterior a Excel (.xlsx) o CSV: una fila por producto y, " +
                       "en la primera fila, los títulos de las columnas. Revisá abajo qué columna es cada dato y cómo quedaría cada producto."
            });

            var filaArchivo = new FlowLayoutPanel { AutoSize = true };
            filaArchivo.Controls.Add(Estilos.Boton("Elegir archivo…", (_, _) => ElegirArchivo(), 180));
            filaArchivo.Controls.Add(lblArchivo);
            arriba.Controls.Add(filaArchivo);

            // Una lista por dato, en dos columnas: "Código de barras [A · Código]   Nombre [B · Descripción]".
            foreach (var campo in Enum.GetValues<CampoImportable>())
            {
                var combo = Estilos.Opciones<Opcion<int?>>([], o => o.Texto, 260);
                combo.SelectedIndexChanged += (_, _) => ColumnaElegida(campo);
                combos[campo] = combo;
                zonaColumnas.Controls.Add(Estilos.Etiqueta(campo.EsObligatorio() ? $"{campo.Descripcion()} *" : campo.Descripcion()));
                zonaColumnas.Controls.Add(combo);
            }
            arriba.Controls.Add(zonaColumnas);
            var filaSiYaExiste = new FlowLayoutPanel { AutoSize = true, Margin = new Padding(0, 4, 0, 0) };
            filaSiYaExiste.Controls.Add(Estilos.Etiqueta("Si el código ya existe:"));
            filaSiYaExiste.Controls.Add(cmbSiYaExiste);
            arriba.Controls.Add(filaSiYaExiste);
            arriba.Controls.Add(lblResumen);
            cmbSiYaExiste.SelectedIndexChanged += (_, _) => { if (!_sincronizando) ActualizarVistaPrevia(); };

            Estilos.ConfigurarGrilla(grilla);
            grilla.ReadOnly = true;
            grilla.TabStop = false;
            // El mensaje completo de "Qué pasa" también se ve al pasar el mouse por la celda.
            Estilos.Columna(grilla, nameof(FilaImportada.NumeroDeFila), "Fila", "N0", 50);
            Estilos.Columna(grilla, nameof(FilaImportada.EstadoDescripcion), "Qué pasa", ancho: 250);
            Estilos.Columna(grilla, nameof(FilaImportada.Codigo), "Código", ancho: 130);
            Estilos.Columna(grilla, nameof(FilaImportada.Nombre), "Nombre", llenar: true).MinimumWidth = 160;
            Estilos.Columna(grilla, nameof(FilaImportada.Costo), "Costo", "C2", 90);
            var colVenta = Estilos.Columna(grilla, nameof(FilaImportada.Venta), "Venta", "C2", 90);
            Estilos.Columna(grilla, nameof(FilaImportada.Stock), "Stock", "N0", 60);
            Estilos.Columna(grilla, nameof(FilaImportada.Categoria), "Rubro", ancho: 110);
            grilla.CellFormatting += (_, e) =>
            {
                if (e.RowIndex < 0 || grilla.Rows[e.RowIndex].DataBoundItem is not FilaImportada fila)
                    return;
                if (fila.Estado == EstadoFilaImportada.Error)
                {
                    e.CellStyle!.ForeColor = Estilos.Error;
                    // En una fila con error el precio no se pudo leer: mejor vacío que "$ 0,00".
                    if (e.ColumnIndex == colVenta.Index)
                    {
                        e.Value = "";
                        e.FormattingApplied = true;
                    }
                }
                else if (fila.Estado == EstadoFilaImportada.YaExiste)
                    e.CellStyle!.ForeColor = SystemColors.GrayText;
            };

            var abajo = new FlowLayoutPanel { Dock = DockStyle.Bottom, AutoSize = true, Padding = new Padding(0, 8, 0, 0) };
            btnImportar = Estilos.Boton("Importar (Enter)", (_, _) => Importar(), 180);
            Estilos.Destacar(btnImportar, Estilos.FondoBotonPrincipal);
            abajo.Controls.Add(btnImportar);
            abajo.Controls.Add(Estilos.Boton("Cerrar (Esc)", (_, _) => Cerrado?.Invoke(this, EventArgs.Empty), 150));
            abajo.Controls.Add(lblEstado);

            Controls.Add(grilla);
            Controls.Add(abajo);
            Controls.Add(arriba);
        }

        /// <summary>Deja el panel listo para elegir un archivo.</summary>
        public void Iniciar()
        {
            _tabla = null;
            _vistaPrevia = null;
            _mapeo = new MapeoDeColumnas();
            lblArchivo.Text = "Todavía no elegiste ningún archivo.";
            lblArchivo.ForeColor = SystemColors.GrayText;
            lblEstado.Text = "";
            CargarListasDeColumnas([]);
            _sincronizando = true;
            Estilos.Elegir<Opcion<SiElCodigoYaExiste>>(cmbSiYaExiste, o => o.Valor == SiElCodigoYaExiste.Actualizar);
            _sincronizando = false;
            MostrarVistaPrevia();
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            switch (keyData)
            {
                case Keys.Enter: Importar(); return true;
                case Keys.Escape: Cerrado?.Invoke(this, EventArgs.Empty); return true;
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }

        private void ElegirArchivo()
        {
            using var dialogo = new OpenFileDialog
            {
                Title = "Elegí la planilla con los productos",
                Filter = "Excel o CSV (*.xlsx;*.csv;*.txt)|*.xlsx;*.csv;*.txt|Todos los archivos (*.*)|*.*"
            };
            if (dialogo.ShowDialog(this) != DialogResult.OK)
                return;

            try
            {
                Cursor = Cursors.WaitCursor;
                _tabla = LectorDePlanillas.Leer(dialogo.FileName);
                _mapeo = MapeoDeColumnas.Detectar(_tabla.Encabezados);
                lblArchivo.Text = $"{Path.GetFileName(dialogo.FileName)}  ·  {_tabla.Filas.Count} filas";
                lblArchivo.ForeColor = SystemColors.ControlText;
                CargarListasDeColumnas(_tabla.Encabezados);
                ActualizarVistaPrevia();
            }
            catch (Exception ex) when (ex is ReglaDeNegocioException or IOException or UnauthorizedAccessException)
            {
                _tabla = null;
                lblArchivo.Text = ex.Message;
                lblArchivo.ForeColor = Estilos.Error;
                CargarListasDeColumnas([]);
                _vistaPrevia = null;
                MostrarVistaPrevia();
            }
            finally
            {
                Cursor = Cursors.Default;
            }
        }

        /// <summary>Cada lista ofrece "(no importar)" y las columnas de la planilla, y muestra lo que adivinó el mapeo.</summary>
        private void CargarListasDeColumnas(IReadOnlyList<string> encabezados)
        {
            var opciones = new List<Opcion<int?>> { new(null, "(no importar)") };
            opciones.AddRange(encabezados.Select((titulo, i) =>
                new Opcion<int?>(i, $"{TablaImportada.LetraDeColumna(i)} · {(titulo == "" ? "(sin título)" : titulo)}")));

            _sincronizando = true;
            foreach (var (campo, combo) in combos)
            {
                Estilos.CambiarOpciones(combo, opciones);
                combo.Enabled = encabezados.Count > 0;
            }
            _sincronizando = false;
            MostrarMapeo();
        }

        /// <summary>Pone en cada lista la columna que tiene asignada (una columna asignada a otro dato se libera sola).</summary>
        private void MostrarMapeo()
        {
            _sincronizando = true;
            foreach (var (campo, combo) in combos)
                Estilos.Elegir<Opcion<int?>>(combo, o => o.Valor == _mapeo.Columna(campo));
            _sincronizando = false;
        }

        private void ColumnaElegida(CampoImportable campo)
        {
            if (_sincronizando)
                return;
            _mapeo.Asignar(campo, (combos[campo].SelectedItem as Opcion<int?>)?.Valor);
            MostrarMapeo();
            ActualizarVistaPrevia();
        }

        private SiElCodigoYaExiste SiYaExiste =>
            (cmbSiYaExiste.SelectedItem as Opcion<SiElCodigoYaExiste>)?.Valor ?? SiElCodigoYaExiste.Actualizar;

        private void ActualizarVistaPrevia()
        {
            if (_tabla == null)
                return;
            try
            {
                _vistaPrevia = _importador.Previsualizar(_tabla, _mapeo, SiYaExiste);
                MostrarVistaPrevia();
            }
            catch (ReglaDeNegocioException ex)
            {
                _vistaPrevia = null;
                MostrarVistaPrevia(ex.Message);
            }
        }

        private void MostrarVistaPrevia(string? aviso = null)
        {
            grilla.DataSource = _vistaPrevia?.Filas.ToList() ?? [];
            btnImportar.Enabled = _vistaPrevia?.HayAlgoParaImportar == true;

            if (_vistaPrevia is not { } vista)
            {
                lblResumen.Text = aviso ?? "";
                lblResumen.ForeColor = aviso == null ? SystemColors.ControlText : Estilos.Advertencia;
                return;
            }

            var partes = new List<string> { ProductosNuevos(vista.Nuevos) };
            if (vista.Actualizados > 0)
                partes.Add(vista.Actualizados == 1 ? "1 se actualiza" : $"{vista.Actualizados} se actualizan");
            if (vista.SinTocar > 0)
                partes.Add(vista.SinTocar == 1 ? "1 ya existe y no se toca" : $"{vista.SinTocar} ya existen y no se tocan");
            if (vista.ConError > 0)
                partes.Add(vista.ConError == 1 ? "1 con error (no se importa, está en rojo)" : $"{vista.ConError} con error (no se importan, están en rojo)");
            lblResumen.Text = string.Join("  ·  ", partes);
            lblResumen.ForeColor = vista.ConError > 0 ? Estilos.Advertencia : SystemColors.ControlText;
        }

        private static string ProductosNuevos(int cantidad) =>
            cantidad == 1 ? "1 producto nuevo" : $"{cantidad} productos nuevos";

        private void Importar()
        {
            if (_vistaPrevia is not { HayAlgoParaImportar: true } vista)
                return;

            var pregunta = $"¿Importar {ProductosNuevos(vista.Nuevos)}" +
                           (vista.Actualizados > 0 ? $" y actualizar {vista.Actualizados}" : "") + "?" +
                           (vista.ConError == 1 ? "\n\nLa fila con error no se importa." : "") +
                           (vista.ConError > 1 ? $"\n\nLas {vista.ConError} filas con error no se importan." : "") +
                           "\n\nAntes se guarda una copia de seguridad de los datos.";
            if (!Mensajes.Confirmar(this, pregunta))
                return;

            try
            {
                Cursor = Cursors.WaitCursor;
                _backups.HacerCopiaEspecial("antes-de-importar");
                var resultado = _importador.Importar(vista);
                Importado?.Invoke(this, resultado);
            }
            catch (Exception ex) when (ex is ReglaDeNegocioException or IOException or UnauthorizedAccessException)
            {
                lblEstado.Text = $"No se importó nada: {ex.Message}";
                lblEstado.ForeColor = Estilos.Error;
            }
            finally
            {
                Cursor = Cursors.Default;
            }
        }
    }
}

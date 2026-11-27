using KioscoRockAndRoll.Dominio;
using KioscoRockAndRoll.Dominio.Entidades;
using KioscoRockAndRoll.Dominio.Servicios;
using Proyecto_Kiosco_Rock_Roll.Interfaz.Controles;

namespace Proyecto_Kiosco_Rock_Roll.Interfaz.Vistas
{
    /// <summary>
    /// Listado de productos con filtros. El alta, la edición, el ajuste de precios, el pedido
    /// y la administración de categorías y proveedores se hacen en el panel de la derecha.
    /// Los controles están en VistaInventario.Designer.cs (se editan con el diseñador de Visual Studio);
    /// el panel de la derecha y lo que se abre en él se agregan acá porque los arma el programa.
    /// </summary>
    public partial class VistaInventario : Vista
    {
        private readonly InventarioServicio _inventario;
        private readonly CatalogoServicio<Categoria> _categorias;
        private readonly CatalogoServicio<Proveedor> _proveedores;

        private readonly PanelLateral panelLateral = new(ancho: 540);
        private readonly EditorProducto editor;
        private readonly PanelAjustePrecios ajustePrecios;
        private readonly PanelCategorias panelCategorias;
        private readonly PanelProveedores panelProveedores;
        private readonly PanelPedido panelPedido;
        private readonly PanelImportacion panelImportacion;

        // Productos marcados con ✓ para cambiarles el precio juntos. Las marcas se conservan al buscar o filtrar,
        // así se pueden juntar productos que no aparecen en una misma búsqueda.
        private readonly HashSet<long> _marcados = [];

        // Nombres para mostrar en la grilla en lugar de los Id.
        private Dictionary<long, string> _nombreCategoria = [];
        private Dictionary<long, string> _nombreProveedor = [];
        private bool _cargandoFiltros;

        public VistaInventario(InventarioServicio inventario,
            CatalogoServicio<Categoria> categorias, CatalogoServicio<Proveedor> proveedores,
            EditorProducto editor, PanelAjustePrecios ajustePrecios,
            PanelCategorias panelCategorias, PanelProveedores panelProveedores, PanelPedido panelPedido,
            PanelImportacion panelImportacion)
        {
            this.panelImportacion = panelImportacion;
            _inventario = inventario;
            _categorias = categorias;
            _proveedores = proveedores;
            this.editor = editor;
            this.ajustePrecios = ajustePrecios;
            this.panelCategorias = panelCategorias;
            this.panelProveedores = panelProveedores;
            this.panelPedido = panelPedido;

            InitializeComponent();
            Controls.Add(panelLateral);

            // Lo que el diseñador no puede hacer: la grilla y las listas de filtros (se llenan al mostrar la vista).
            Estilos.ConfigurarGrilla(grilla);
            _cargandoFiltros = true;
            Estilos.PrepararOpciones<Opcion<FiltroClasificacion>>(cmbCategoria, [], o => o.Texto);
            Estilos.PrepararOpciones<Opcion<FiltroClasificacion>>(cmbProveedor, [], o => o.Texto);
            _cargandoFiltros = false;

            editor.Guardado += (_, producto) => { CerrarPanel(); Cargar(producto.Id); };
            editor.Cancelado += (_, _) => CerrarPanel();
            ajustePrecios.Aplicado += (_, cantidad) =>
            {
                CerrarPanel();
                _marcados.Clear(); // ya se usaron
                Cargar(Seleccionado?.Id);
                MostrarEstado($"Se actualizaron los precios de {Estilos.Productos(cantidad)}.", Estilos.Exito);
            };
            ajustePrecios.Cancelado += (_, _) => CerrarPanel();
            panelPedido.Cerrado += (_, _) => CerrarPanel();
            panelImportacion.Cerrado += (_, _) => CerrarPanel();
            panelImportacion.Importado += (_, resultado) =>
            {
                CerrarPanel();
                RecargarCatalogos(); // pudo haber rubros y proveedores nuevos
                MostrarEstado($"Importación lista: {resultado.Descripcion}", Estilos.Exito);
            };
            // Al cambiar el catálogo se actualizan los filtros y los nombres de la grilla.
            panelCategorias.Cambiado += (_, _) => RecargarCatalogos();
            panelCategorias.Cerrado += (_, _) => CerrarPanel();
            panelProveedores.Cambiado += (_, _) => RecargarCatalogos();
            panelProveedores.Cerrado += (_, _) => CerrarPanel();
        }

        public override string Titulo => "Inventario";
        public override Keys Atajo => Keys.F2;

        public override void AlMostrar()
        {
            RecargarCatalogos();
            switch (panelLateral.Contenido)
            {
                case EditorProducto: editor.Enfocar(); break;
                // Los productos pudieron cambiar en otra sección: se recalcula la vista previa.
                case PanelAjustePrecios: ajustePrecios.CambiarProductos(ProductosParaAjustar(), QueProductosSeAjustan()); break;
                case null: txtBuscar.Focus(); break;
            }
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            // Si hay un panel abierto, Enter y Esc los maneja él (le llegan primero).
            switch (keyData)
            {
                case Keys.Insert: Nuevo(); return true;
                case Keys.Enter: Editar(); return true;
                case Keys.Delete when grilla.Focused: Eliminar(); return true;
                case Keys.F6: AbrirAjustePrecios(); return true;
                case Keys.F7: AbrirPedido(); return true;
                case Keys.F8: AbrirCatalogo(panelCategorias); return true;
                case Keys.F9: AbrirCatalogo(panelProveedores); return true;
                case Keys.Down when txtBuscar.Focused && grilla.Rows.Count > 0: grilla.Focus(); return true;
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }

        // ---------- Lo que pasa al tocar cada cosa (conectado en el diseñador) ----------

        private void TxtBuscar_TextChanged(object? sender, EventArgs e) => Cargar();

        private void ChkStockBajo_CheckedChanged(object? sender, EventArgs e) => Cargar();

        private void CmbFiltro_SelectedIndexChanged(object? sender, EventArgs e)
        {
            if (!_cargandoFiltros)
                Cargar();
        }

        private void Grilla_CellFormatting(object? sender, DataGridViewCellFormattingEventArgs e)
        {
            MostrarNombres(sender, e);
            if (e.RowIndex < 0)
                return;
            var producto = grilla.Rows[e.RowIndex].DataBoundItem as Producto;
            // Los productos con poco stock se pintan de rojo.
            if (producto is { StockBajo: true })
                e.CellStyle!.BackColor = Estilos.FondoStockBajo;
            // La columna ✓ no está enlazada a los datos: muestra si el producto está en _marcados.
            if (e.ColumnIndex == colMarca.Index)
            {
                e.Value = producto is not null && _marcados.Contains(producto.Id);
                e.FormattingApplied = true;
            }
        }

        // Solo el clic del mouse: la grilla también "hace clic" en la casilla cuando se aprieta espacio sobre ella,
        // y el espacio ya se atiende abajo (si se atendiera dos veces, marcaría y desmarcaría).
        private void Grilla_CellMouseClick(object? sender, DataGridViewCellMouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left && e.ColumnIndex == colMarca.Index && e.RowIndex >= 0)
                AlternarMarca(e.RowIndex);
        }

        // La barra espaciadora marca o desmarca el producto elegido. Se toma en PreviewKeyDown porque
        // la grilla no deja llegar el espacio a KeyDown.
        private void Grilla_PreviewKeyDown(object? sender, PreviewKeyDownEventArgs e)
        {
            if (e.KeyCode == Keys.Space && e.Modifiers == Keys.None && FilaElegida is { } fila)
                AlternarMarca(fila.Index);
        }

        private void Grilla_CellDoubleClick(object? sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0 && e.ColumnIndex != colMarca.Index)
                Editar();
        }

        private void BtnNuevo_Click(object? sender, EventArgs e) => Nuevo();

        private void BtnEditar_Click(object? sender, EventArgs e) => Editar();

        private void BtnEliminar_Click(object? sender, EventArgs e) => Eliminar();

        private void BtnActualizarPrecios_Click(object? sender, EventArgs e) => AbrirAjustePrecios();

        private void BtnArmarPedido_Click(object? sender, EventArgs e) => AbrirPedido();

        private void BtnCategorias_Click(object? sender, EventArgs e) => AbrirCatalogo(panelCategorias);

        private void BtnProveedores_Click(object? sender, EventArgs e) => AbrirCatalogo(panelProveedores);

        private void BtnImportar_Click(object? sender, EventArgs e) => AbrirImportacion();

        private void BtnDesmarcar_Click(object? sender, EventArgs e) => Desmarcar();

        private Producto? Seleccionado => grilla.CurrentRow?.DataBoundItem as Producto;

        // La fila actual o, si todavía no se hizo clic en ninguna, la que se ve resaltada.
        private DataGridViewRow? FilaElegida => grilla.CurrentRow ?? grilla.SelectedRows.Cast<DataGridViewRow>().FirstOrDefault();

        private IReadOnlyList<Producto> ProductosVisibles => grilla.DataSource as List<Producto> ?? [];

        private static FiltroClasificacion Elegido(ComboBox combo) =>
            (combo.SelectedItem as Opcion<FiltroClasificacion>)?.Valor ?? FiltroClasificacion.Todos;

        /// <summary>Vuelve a leer categorías y proveedores (filtros y nombres) y recarga la lista.</summary>
        private void RecargarCatalogos()
        {
            var categorias = _categorias.Listar();
            var proveedores = _proveedores.Listar();
            _nombreCategoria = categorias.ToDictionary(c => c.Id, c => c.Nombre);
            _nombreProveedor = proveedores.ToDictionary(p => p.Id, p => p.Nombre);

            // Se conserva lo elegido en cada filtro (si todavía existe).
            var categoriaElegida = Elegido(cmbCategoria);
            var proveedorElegido = Elegido(cmbProveedor);
            _cargandoFiltros = true;
            Estilos.CambiarOpciones(cmbCategoria, OpcionesDeCatalogo.ParaFiltrar(categorias, "Todas", "Sin categoría"));
            Estilos.CambiarOpciones(cmbProveedor, OpcionesDeCatalogo.ParaFiltrar(proveedores, "Todos", "Sin proveedor"));
            Estilos.Elegir<Opcion<FiltroClasificacion>>(cmbCategoria, o => o.Valor == categoriaElegida);
            Estilos.Elegir<Opcion<FiltroClasificacion>>(cmbProveedor, o => o.Valor == proveedorElegido);
            _cargandoFiltros = false;

            Cargar(Seleccionado?.Id);
        }

        private void Cargar(long? seleccionarId = null)
        {
            var visibles = _inventario.Buscar(new FiltroProductos
            {
                Texto = txtBuscar.Text,
                SoloStockBajo = chkStockBajo.Checked,
                Categoria = Elegido(cmbCategoria),
                Proveedor = Elegido(cmbProveedor)
            }).ToList();

            grilla.DataSource = visibles;
            lblEstado.Text = "";
            MostrarMarcados();
            lblResumen.Text = $"{Estilos.Productos(visibles.Count)}  ·{visibles.Count(p => p.StockBajo)} con stock bajo";

            var indice = visibles.FindIndex(p => p.Id == seleccionarId);
            if (indice >= 0)
                grilla.CurrentCell = grilla.Rows[indice].Cells[0];
        }

        private void MostrarNombres(object? sender, DataGridViewCellFormattingEventArgs e)
        {
            var columna = grilla.Columns[e.ColumnIndex].DataPropertyName;
            var nombres = columna switch
            {
                nameof(Producto.CategoriaId) => _nombreCategoria,
                nameof(Producto.ProveedorId) => _nombreProveedor,
                _ => null
            };
            if (nombres == null)
                return;

            e.Value = e.Value is long id && nombres.TryGetValue(id, out var nombre) ? nombre : "";
            e.FormattingApplied = true;
        }

        private void Nuevo()
        {
            editor.NuevoProducto();
            AbrirEditor();
        }

        private void Editar()
        {
            if (Seleccionado is not { } producto)
                return;
            editor.Editar(producto);
            AbrirEditor();
        }

        private void AbrirEditor()
        {
            panelLateral.Mostrar(editor);
            editor.Enfocar();
        }

        // ---------- Productos marcados ----------

        private void AlternarMarca(int fila)
        {
            if (grilla.Rows[fila].DataBoundItem is not Producto producto)
                return;
            if (!_marcados.Remove(producto.Id))
                _marcados.Add(producto.Id);
            grilla.InvalidateRow(fila);
            MostrarMarcados();
        }

        private void Desmarcar()
        {
            _marcados.Clear();
            grilla.Invalidate();
            MostrarMarcados();
            txtBuscar.Focus();
        }

        /// <summary>Cuántos hay marcados (o cómo marcar) y, si el panel de precios está abierto, a qué productos se aplica.</summary>
        private void MostrarMarcados()
        {
            var hay = _marcados.Count > 0;
            lblMarcados.Text = hay
                ? $"✓ {(_marcados.Count == 1 ? "1 marcado" : $"{_marcados.Count} marcados")}"
                : "Tip: marcá ✓ varios productos para cambiarles el precio juntos";
            lblMarcados.ForeColor = hay ? Estilos.Exito : SystemColors.GrayText;
            btnDesmarcar.Visible = hay;
            if (panelLateral.Contenido == ajustePrecios)
                ajustePrecios.CambiarProductos(ProductosParaAjustar(), QueProductosSeAjustan());
        }

        /// <summary>
        /// A qué productos se les cambia el precio: a los marcados con ✓ si hay alguno; si no, a los que se ven
        /// en la lista (filtrando por una categoría o un proveedor, o buscando "coca", se actualizan solo esos).
        /// </summary>
        private IReadOnlyList<Producto> ProductosParaAjustar() => _marcados.Count > 0
            ? _marcados.Select(_inventario.ObtenerPorId).OfType<Producto>().OrderBy(p => p.Nombre).ToList()
            : ProductosVisibles;

        private string QueProductosSeAjustan() => _marcados.Count switch
        {
            0 => FiltrosEnPalabras(),
            1 => "el que marcaste",
            var n => $"los {n} que marcaste"
        };

        private void AbrirAjustePrecios()
        {
            var productos = ProductosParaAjustar();
            if (productos.Count == 0)
            {
                MostrarEstado("No hay productos en la lista para actualizar.", Estilos.Advertencia);
                return;
            }
            panelLateral.Mostrar(ajustePrecios);
            // Cuando se marcan productos uno por uno suele ser para dejarlos al mismo precio.
            ajustePrecios.Iniciar(productos, QueProductosSeAjustan(), precioFijo: _marcados.Count > 0);
        }

        /// <summary>Los filtros de la lista en palabras, para que quede claro a qué productos se aplica algo.</summary>
        private string FiltrosEnPalabras()
        {
            var partes = new List<string>();
            if (Elegido(cmbCategoria) != FiltroClasificacion.Todos)
                partes.Add($"categoría {((Opcion<FiltroClasificacion>)cmbCategoria.SelectedItem!).Texto}");
            if (Elegido(cmbProveedor) != FiltroClasificacion.Todos)
                partes.Add($"proveedor {((Opcion<FiltroClasificacion>)cmbProveedor.SelectedItem!).Texto}");
            if (txtBuscar.Text.Trim() != "")
                partes.Add($"que dicen \"{txtBuscar.Text.Trim()}\"");
            if (chkStockBajo.Checked)
                partes.Add("solo los de stock bajo");
            return partes.Count == 0 ? "todos los productos" : string.Join(", ", partes);
        }

        private void AbrirPedido()
        {
            panelLateral.Mostrar(panelPedido);
            panelPedido.Activar(Elegido(cmbProveedor).Id);
        }

        private void AbrirImportacion()
        {
            panelLateral.Mostrar(panelImportacion, PanelImportacion.AnchoSugerido);
            panelImportacion.Iniciar();
        }

        private void AbrirCatalogo<T>(PanelCatalogo<T> panel) where T : ElementoDeCatalogo
        {
            panelLateral.Mostrar(panel);
            panel.Activar();
        }

        private void CerrarPanel()
        {
            panelLateral.Ocultar();
            grilla.Focus();
        }

        private void MostrarEstado(string texto, Color color)
        {
            lblEstado.Text = texto;
            lblEstado.ForeColor = color;
        }

        private void Eliminar()
        {
            if (Seleccionado is not { } producto || !Mensajes.Confirmar(this, $"¿Eliminar \"{producto.Nombre}\"?"))
                return;

            try
            {
                _inventario.Eliminar(producto.Id);
                _marcados.Remove(producto.Id);
                Cargar();
            }
            catch (ReglaDeNegocioException ex)
            {
                Mensajes.Informacion(this, ex.Message);
            }
        }
    }
}

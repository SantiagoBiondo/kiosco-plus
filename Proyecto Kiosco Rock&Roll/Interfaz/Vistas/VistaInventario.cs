using KioscoRockAndRoll.Dominio;
using KioscoRockAndRoll.Dominio.Entidades;
using KioscoRockAndRoll.Dominio.Servicios;
using Proyecto_Kiosco_Rock_Roll.Interfaz.Controles;

namespace Proyecto_Kiosco_Rock_Roll.Interfaz.Vistas
{
    /// <summary>
    /// Listado de productos con filtros. El alta, la edición, el ajuste de precios, el pedido
    /// y la administración de categorías y proveedores se hacen en el panel de la derecha.
    /// </summary>
    public class VistaInventario : Vista
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

        private readonly TextBox txtBuscar = new() { Width = 320, Font = Estilos.Grande, PlaceholderText = "Buscar por nombre o código" };
        private readonly ComboBox cmbCategoria = Estilos.Opciones<Opcion<FiltroClasificacion>>([], o => o.Texto, 200);
        private readonly ComboBox cmbProveedor = Estilos.Opciones<Opcion<FiltroClasificacion>>([], o => o.Texto, 200);
        private readonly CheckBox chkStockBajo = new() { Text = "Solo stock bajo", AutoSize = true, Margin = new Padding(12, 10, 3, 3) };
        private readonly DataGridView grilla = new();
        private readonly Label lblResumen = new() { AutoSize = true, Margin = new Padding(12, 14, 3, 3) };
        private readonly Label lblEstado = new() { AutoSize = true, Margin = new Padding(12, 14, 3, 3) };

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

            var arriba = new FlowLayoutPanel { Dock = DockStyle.Top, AutoSize = true, Padding = new Padding(6) };
            arriba.Controls.Add(txtBuscar);
            arriba.Controls.Add(new Label { Text = "Categoría:", AutoSize = true, Margin = new Padding(12, 10, 3, 3) });
            arriba.Controls.Add(cmbCategoria);
            arriba.Controls.Add(new Label { Text = "Proveedor:", AutoSize = true, Margin = new Padding(12, 10, 3, 3) });
            arriba.Controls.Add(cmbProveedor);
            arriba.Controls.Add(chkStockBajo);
            arriba.Controls.Add(lblResumen);
            foreach (var combo in new[] { cmbCategoria, cmbProveedor })
                combo.Margin = new Padding(3, 6, 3, 3);

            Estilos.ConfigurarGrilla(grilla);
            grilla.ReadOnly = true;
            Estilos.Columna(grilla, nameof(Producto.CodigoBarras), "Código", ancho: 150);
            Estilos.Columna(grilla, nameof(Producto.Nombre), "Producto", llenar: true);
            Estilos.Columna(grilla, nameof(Producto.CategoriaId), "Categoría", ancho: 150);
            Estilos.Columna(grilla, nameof(Producto.ProveedorId), "Proveedor", ancho: 150);
            Estilos.Columna(grilla, nameof(Producto.PrecioCosto), "Costo", "C2", 110);
            Estilos.Columna(grilla, nameof(Producto.PrecioVenta), "Venta", "C2", 110);
            Estilos.Columna(grilla, nameof(Producto.Stock), "Stock", "N0", 75);
            Estilos.Columna(grilla, nameof(Producto.StockMinimo), "Mínimo", "N0", 75);

            grilla.CellFormatting += MostrarNombres;
            // Los productos con poco stock se pintan de rojo.
            grilla.CellFormatting += (_, e) =>
            {
                if (e.RowIndex >= 0 && grilla.Rows[e.RowIndex].DataBoundItem is Producto { StockBajo: true })
                    e.CellStyle!.BackColor = Estilos.FondoStockBajo;
            };
            grilla.CellDoubleClick += (_, e) => { if (e.RowIndex >= 0) Editar(); };

            var abajo = new FlowLayoutPanel { Dock = DockStyle.Bottom, AutoSize = true, Padding = new Padding(6) };
            abajo.Controls.Add(Estilos.Boton("Nuevo (Insert)", (_, _) => Nuevo(), 150));
            abajo.Controls.Add(Estilos.Boton("Editar (Enter)", (_, _) => Editar(), 150));
            abajo.Controls.Add(Estilos.Boton("Eliminar (Supr)", (_, _) => Eliminar(), 150));
            abajo.Controls.Add(Estilos.Boton("Actualizar precios (F6)", (_, _) => AbrirAjustePrecios(), 210));
            abajo.Controls.Add(Estilos.Boton("Armar pedido (F7)", (_, _) => AbrirPedido(), 180));
            abajo.Controls.Add(Estilos.Boton("Categorías (F8)", (_, _) => AbrirCatalogo(panelCategorias), 160));
            abajo.Controls.Add(Estilos.Boton("Proveedores (F9)", (_, _) => AbrirCatalogo(panelProveedores), 170));
            abajo.Controls.Add(Estilos.Boton("Importar de Excel…", (_, _) => AbrirImportacion(), 190));
            abajo.Controls.Add(lblEstado);

            var principal = new Panel { Dock = DockStyle.Fill };
            principal.Controls.Add(grilla);
            principal.Controls.Add(abajo);
            principal.Controls.Add(arriba);
            Controls.Add(principal);
            Controls.Add(panelLateral);

            txtBuscar.TextChanged += (_, _) => Cargar();
            chkStockBajo.CheckedChanged += (_, _) => Cargar();
            cmbCategoria.SelectedIndexChanged += (_, _) => { if (!_cargandoFiltros) Cargar(); };
            cmbProveedor.SelectedIndexChanged += (_, _) => { if (!_cargandoFiltros) Cargar(); };

            editor.Guardado += (_, producto) => { CerrarPanel(); Cargar(producto.Id); };
            editor.Cancelado += (_, _) => CerrarPanel();
            ajustePrecios.Aplicado += (_, cantidad) =>
            {
                CerrarPanel();
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
                case PanelAjustePrecios: ajustePrecios.CambiarProductos(ProductosVisibles, FiltrosEnPalabras()); break;
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

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                // Los paneles que no están en pantalla no son hijos de la vista: se liberan a mano.
                editor.Dispose();
                ajustePrecios.Dispose();
                panelCategorias.Dispose();
                panelProveedores.Dispose();
                panelPedido.Dispose();
                panelImportacion.Dispose();
            }
            base.Dispose(disposing);
        }

        private Producto? Seleccionado => grilla.CurrentRow?.DataBoundItem as Producto;

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
            if (panelLateral.Contenido == ajustePrecios)
                ajustePrecios.CambiarProductos(visibles, FiltrosEnPalabras());
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

        /// <summary>
        /// Se aplica a los productos que se ven en la lista: filtrando por una categoría
        /// o un proveedor (o buscando "coca") se actualizan solo esos; sin filtros, todos.
        /// </summary>
        private void AbrirAjustePrecios()
        {
            if (ProductosVisibles.Count == 0)
            {
                MostrarEstado("No hay productos en la lista para actualizar.", Estilos.Advertencia);
                return;
            }
            panelLateral.Mostrar(ajustePrecios);
            ajustePrecios.Iniciar(ProductosVisibles, FiltrosEnPalabras());
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
                Cargar();
            }
            catch (ReglaDeNegocioException ex)
            {
                Mensajes.Informacion(this, ex.Message);
            }
        }
    }
}

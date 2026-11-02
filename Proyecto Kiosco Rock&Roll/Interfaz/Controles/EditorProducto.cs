using KioscoRockAndRoll.Dominio;
using KioscoRockAndRoll.Dominio.Entidades;
using KioscoRockAndRoll.Dominio.Servicios;

namespace Proyecto_Kiosco_Rock_Roll.Interfaz.Controles
{
    /// <summary>
    /// Alta o edición de un producto, embebible en cualquier pantalla.
    /// Enter guarda, Esc cancela. Las validaciones las hace el dominio.
    /// </summary>
    public class EditorProducto : UserControl
    {
        private readonly InventarioServicio _inventario;
        private readonly CatalogoServicio<Categoria> _categorias;
        private readonly CatalogoServicio<Proveedor> _proveedores;
        private long? _idEnEdicion;

        private readonly Label lblTitulo = Estilos.Etiqueta("", Estilos.Titulo);
        private readonly TextBox txtCodigo = new() { Width = 220 };
        private readonly TextBox txtNombre = new() { Width = 220 };
        private readonly NumericUpDown numCosto = Estilos.CampoNumero(2);
        private readonly NumericUpDown numVenta = Estilos.CampoNumero(2);
        private readonly NumericUpDown numStock = Estilos.CampoNumero(0, 1_000_000);
        private readonly NumericUpDown numMinimo = Estilos.CampoNumero(0, 1_000_000);
        private readonly ComboBox cmbCategoria = Estilos.Opciones<Opcion<long?>>([], o => o.Texto, 220);
        private readonly ComboBox cmbProveedor = Estilos.Opciones<Opcion<long?>>([], o => o.Texto, 220);
        private readonly Label lblMargen = Estilos.Etiqueta("");

        // Productos que se venden sueltos, sacados de un paquete (ej.: cigarrillo suelto de un atado).
        private readonly CheckBox chkSuelto = new()
        {
            Text = "Se vende suelto, sacado de un paquete (ej.: cigarrillo suelto de un atado)",
            AutoSize = true,
            Margin = new Padding(3, 12, 3, 3)
        };
        private readonly TextBox txtPaquete = new() { Width = 220, PlaceholderText = "Escaneá el paquete" };
        private readonly NumericUpDown numUnidades = Estilos.CampoNumero(0, 1000);
        private readonly Label lblPaquete = new() { AutoSize = true, MaximumSize = new Size(400, 0), Margin = new Padding(3, 0, 3, 6) };
        private readonly List<Control> filasDePaquete = [];

        // Productos que se cobran según lo que se lleva (ej.: caramelos por peso).
        private readonly CheckBox chkPrecioAlVender = new()
        {
            Text = "El precio se pone al vender (ej.: caramelos por peso)",
            AutoSize = true,
            Margin = new Padding(3, 8, 3, 3)
        };
        private readonly Label lblAyudaPrecioAlVender = new()
        {
            AutoSize = true,
            MaximumSize = new Size(440, 0),
            ForeColor = SystemColors.GrayText,
            Margin = new Padding(3, 0, 3, 6),
            Text = "Al venderlo se pregunta cuánto cobrar. El costo y la venta de arriba son de referencia " +
                   "(ej.: 100 g: costo $1.200, venta $1.800) y sirven para calcular la ganancia. No se controla el stock."
        };
        private readonly Label lblError = new() { AutoSize = true, ForeColor = Estilos.Error, MaximumSize = new Size(400, 0) };

        /// <summary>Se guardó el producto (nuevo o editado).</summary>
        public event EventHandler<Producto>? Guardado;

        public event EventHandler? Cancelado;

        public EditorProducto(InventarioServicio inventario,
            CatalogoServicio<Categoria> categorias, CatalogoServicio<Proveedor> proveedores)
        {
            _inventario = inventario;
            _categorias = categorias;
            _proveedores = proveedores;
            Font = Estilos.Normal;

            var tabla = new TableLayoutPanel { ColumnCount = 2, AutoSize = true, Dock = DockStyle.Top };
            tabla.Controls.Add(lblTitulo);
            tabla.SetColumnSpan(lblTitulo, 2);
            AgregarFila(tabla, "Código de barras", txtCodigo);
            AgregarFila(tabla, "", new Label
            {
                AutoSize = true,
                MaximumSize = new Size(260, 0),
                ForeColor = SystemColors.GrayText,
                Margin = new Padding(3, 0, 3, 6),
                Text = "¿No tiene código? Inventá uno corto (por ejemplo 101) y anotalo para tipearlo al vender."
            });
            AgregarFila(tabla, "Nombre", txtNombre);
            AgregarFila(tabla, "Precio de costo", numCosto);
            AgregarFila(tabla, "Precio de venta", numVenta);
            AgregarFila(tabla, "", lblMargen);
            tabla.Controls.Add(chkPrecioAlVender);
            tabla.SetColumnSpan(chkPrecioAlVender, 2);
            tabla.Controls.Add(lblAyudaPrecioAlVender);
            tabla.SetColumnSpan(lblAyudaPrecioAlVender, 2);
            chkPrecioAlVender.CheckedChanged += (_, _) => lblAyudaPrecioAlVender.Visible = chkPrecioAlVender.Checked;
            AgregarFila(tabla, "Stock actual", numStock);
            AgregarFila(tabla, "Stock mínimo", numMinimo);
            AgregarFila(tabla, "Categoría", cmbCategoria);
            AgregarFila(tabla, "Proveedor", cmbProveedor);

            tabla.Controls.Add(chkSuelto);
            tabla.SetColumnSpan(chkSuelto, 2);
            filasDePaquete.AddRange(AgregarFila(tabla, "Código del paquete", txtPaquete));
            filasDePaquete.AddRange(AgregarFila(tabla, "", lblPaquete));
            filasDePaquete.AddRange(AgregarFila(tabla, "Unidades por paquete", numUnidades));
            chkSuelto.CheckedChanged += (_, _) => MostrarPaquete();
            txtPaquete.TextChanged += (_, _) => DescribirPaquete();

            var botones = new FlowLayoutPanel { AutoSize = true, Margin = new Padding(0, 12, 0, 0) };
            botones.Controls.Add(Estilos.Boton("Guardar (Enter)", (_, _) => Guardar(), 160));
            botones.Controls.Add(Estilos.Boton("Cancelar (Esc)", (_, _) => Cancelado?.Invoke(this, EventArgs.Empty), 160));
            tabla.Controls.Add(botones);
            tabla.SetColumnSpan(botones, 2);
            tabla.Controls.Add(lblError);
            tabla.SetColumnSpan(lblError, 2);

            Controls.Add(tabla);

            foreach (var campo in new[] { numCosto, numVenta })
            {
                campo.ValueChanged += (_, _) => ActualizarMargen();
                campo.KeyUp += (_, _) => ActualizarMargen();
            }
        }

        public void NuevoProducto(string codigoBarras = "", int stockInicial = 0)
        {
            _idEnEdicion = null;
            lblTitulo.Text = "Nuevo producto";
            Cargar("", "", 0, 0, stockInicial, 0, null, null);
            txtCodigo.Text = codigoBarras;
            CargarPaquete(null, 0);
            CargarPrecioAlVender(false);
        }

        public void Editar(Producto producto)
        {
            _idEnEdicion = producto.Id;
            lblTitulo.Text = "Editar producto";
            Cargar(producto.CodigoBarras, producto.Nombre, producto.PrecioCosto, producto.PrecioVenta,
                Math.Max(0, producto.Stock), producto.StockMinimo, producto.CategoriaId, producto.ProveedorId);
            var paquete = producto.PaqueteId is { } paqueteId ? _inventario.ObtenerPorId(paqueteId) : null;
            CargarPaquete(paquete?.CodigoBarras, producto.UnidadesPorPaquete);
            CargarPrecioAlVender(producto.PrecioAlVender);
        }

        private void CargarPrecioAlVender(bool alVender)
        {
            chkPrecioAlVender.Checked = alVender;
            lblAyudaPrecioAlVender.Visible = alVender;
        }

        private void CargarPaquete(string? codigoPaquete, int unidades)
        {
            txtPaquete.Text = codigoPaquete ?? "";
            numUnidades.Value = unidades;
            chkSuelto.Checked = codigoPaquete != null;
            MostrarPaquete();
        }

        private void MostrarPaquete()
        {
            foreach (var control in filasDePaquete)
                control.Visible = chkSuelto.Checked;
            DescribirPaquete();
        }

        /// <summary>Muestra de qué paquete se trata a medida que se escribe o escanea el código.</summary>
        private void DescribirPaquete()
        {
            var codigo = txtPaquete.Text.Trim();
            if (!chkSuelto.Checked || codigo == "")
            {
                lblPaquete.Text = "";
                return;
            }

            var paquete = _inventario.ObtenerPorCodigo(codigo);
            lblPaquete.Text = paquete == null
                ? "No hay ningún producto con ese código. Cargá primero el paquete."
                : $"{paquete.Nombre} (quedan {paquete.Stock} cerrados)";
            lblPaquete.ForeColor = paquete == null ? Estilos.Error : Estilos.Exito;
        }

        /// <summary>Pone el cursor en el primer campo que falta completar.</summary>
        public void Enfocar()
        {
            var campo = txtCodigo.Text == "" ? txtCodigo : txtNombre;
            campo.Focus();
            campo.SelectAll();
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            // El lector de códigos manda un Enter al final: en el campo del paquete solo pasa al siguiente.
            if (keyData == Keys.Enter && txtPaquete.Focused)
            {
                numUnidades.Focus();
                numUnidades.Select(0, numUnidades.Text.Length);
                return true;
            }
            switch (keyData)
            {
                case Keys.Enter: Guardar(); return true;
                case Keys.Escape: Cancelado?.Invoke(this, EventArgs.Empty); return true;
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }

        private void Cargar(string codigo, string nombre, decimal costo, decimal venta, int stock, int minimo,
            long? categoriaId, long? proveedorId)
        {
            txtCodigo.Text = codigo;
            txtNombre.Text = nombre;
            numCosto.Value = costo;
            numVenta.Value = venta;
            numStock.Value = Math.Min(stock, (int)numStock.Maximum);
            numMinimo.Value = minimo;

            // Las listas se recargan cada vez, por si se agregaron categorías o proveedores.
            Estilos.CambiarOpciones(cmbCategoria, OpcionesDeCatalogo.ParaAsignar(_categorias.Listar(), "(sin categoría)"));
            Estilos.CambiarOpciones(cmbProveedor, OpcionesDeCatalogo.ParaAsignar(_proveedores.Listar(), "(sin proveedor)"));
            Estilos.Elegir<Opcion<long?>>(cmbCategoria, o => o.Valor == categoriaId);
            Estilos.Elegir<Opcion<long?>>(cmbProveedor, o => o.Valor == proveedorId);

            lblError.Text = "";
            ActualizarMargen();
        }

        /// <returns>La etiqueta y el campo, por si hay que mostrarlos u ocultarlos juntos.</returns>
        private static Control[] AgregarFila(TableLayoutPanel tabla, string etiqueta, Control campo)
        {
            var label = Estilos.Etiqueta(etiqueta);
            tabla.Controls.Add(label);
            tabla.Controls.Add(campo);
            return [label, campo];
        }

        private void ActualizarMargen()
        {
            var costo = numCosto.ValorTipeado();
            var venta = numVenta.ValorTipeado();
            if (costo <= 0 || venta <= 0)
            {
                lblMargen.Text = "";
                return;
            }

            var margen = (venta - costo) / costo;
            lblMargen.Text = margen < 0 ? $"¡Vende a pérdida! ({margen:P0})" : $"Ganancia: {margen:P0}";
            lblMargen.ForeColor = margen < 0 ? Estilos.Error : Estilos.Exito;
        }

        private void Guardar()
        {
            var datos = new DatosProducto(
                txtCodigo.Text, txtNombre.Text, numCosto.Value, numVenta.Value, (int)numStock.Value, (int)numMinimo.Value,
                ((Opcion<long?>)cmbCategoria.SelectedItem!).Valor, ((Opcion<long?>)cmbProveedor.SelectedItem!).Valor,
                chkSuelto.Checked ? txtPaquete.Text : null, (int)numUnidades.Value,
                PrecioAlVender: chkPrecioAlVender.Checked);

            try
            {
                var producto = _idEnEdicion is { } id
                    ? _inventario.Modificar(id, datos)
                    : _inventario.Crear(datos);
                lblError.Text = "";
                Guardado?.Invoke(this, producto);
            }
            catch (ReglaDeNegocioException ex)
            {
                lblError.Text = ex.Message;
            }
        }
    }
}

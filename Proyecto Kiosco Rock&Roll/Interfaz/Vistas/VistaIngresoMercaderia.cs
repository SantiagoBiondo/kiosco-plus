using KioscoRockAndRoll.Dominio.Servicios;
using Proyecto_Kiosco_Rock_Roll.Interfaz.Controles;

namespace Proyecto_Kiosco_Rock_Roll.Interfaz.Vistas
{
    /// <summary>
    /// Para cuando llega un pedido: se escanea cada producto y se suma al stock.
    /// Si el código no existe, se da de alta en el panel de la derecha con el código y la cantidad ya cargados.
    /// Los controles están en VistaIngresoMercaderia.Designer.cs (se editan con el diseñador de Visual Studio);
    /// el panel de la derecha y el editor se agregan acá porque los arma el programa.
    /// </summary>
    public partial class VistaIngresoMercaderia : Vista
    {
        private readonly InventarioServicio _inventario;

        private readonly PanelLateral panelLateral = new();
        private readonly EditorProducto editor;

        public VistaIngresoMercaderia(InventarioServicio inventario, EditorProducto editor)
        {
            _inventario = inventario;
            this.editor = editor;

            InitializeComponent();
            Controls.Add(panelLateral);

            editor.Guardado += (_, nuevo) =>
            {
                CerrarEditor();
                Registrar($"NUEVO  {nuevo.Nombre}  (stock: {nuevo.Stock})");
            };
            editor.Cancelado += (_, _) =>
            {
                CerrarEditor();
                MostrarEstado("No se cargó el producto nuevo.", Estilos.Advertencia);
            };
        }

        public override string Titulo => "Ingreso mercadería";
        public override Keys Atajo => Keys.F3;

        public override void AlMostrar()
        {
            if (panelLateral.Abierto)
                editor.Enfocar();
            else
                txtCodigo.Focus();
        }

        private void TxtCodigo_KeyDown(object? sender, KeyEventArgs e)
        {
            if (e.KeyCode != Keys.Enter)
                return;
            e.SuppressKeyPress = true;

            var codigo = txtCodigo.Text.Trim();
            txtCodigo.Clear();
            if (codigo == "")
                return;

            var cantidad = (int)numCantidad.Value;
            if (_inventario.IngresarMercaderia(codigo, cantidad) is { } existente)
            {
                Registrar($"+{cantidad}  {existente.Nombre}  (stock: {existente.Stock})");
                return;
            }

            editor.NuevoProducto(codigo, cantidad);
            panelLateral.Mostrar(editor);
            txtCodigo.Enabled = false;
            editor.Enfocar();
            MostrarEstado($"El código {codigo} no está registrado: completá los datos a la derecha.", Estilos.Advertencia);
        }

        private void CerrarEditor()
        {
            panelLateral.Ocultar();
            txtCodigo.Enabled = true;
            txtCodigo.Focus();
        }

        private void Registrar(string texto)
        {
            lstIngresos.Items.Insert(0, $"{DateTime.Now:HH:mm}   {texto}");
            MostrarEstado(texto, Estilos.Exito);
            numCantidad.Value = 1;
            txtCodigo.Focus();
        }

        private void MostrarEstado(string texto, Color color)
        {
            lblEstado.Text = texto;
            lblEstado.ForeColor = color;
        }
    }
}

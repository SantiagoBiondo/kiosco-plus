namespace Proyecto_Kiosco_Rock_Roll.Interfaz.Vistas
{
    /// <summary>
    /// Una sección de la ventana principal (Venta, Inventario, etc.).
    /// Las vistas se crean una sola vez y se conservan al cambiar de sección,
    /// así no se pierde, por ejemplo, una venta a medio cargar.
    /// <para>
    /// No es abstracta a propósito: el diseñador de Visual Studio crea una instancia de la clase base
    /// para poder mostrar y editar las vistas que tienen su archivo .Designer.cs.
    /// </para>
    /// </summary>
    public class Vista : UserControl
    {
        public Vista()
        {
            Dock = DockStyle.Fill;
            Font = Estilos.Normal;
        }

        /// <summary>Texto del botón en la barra de secciones. Cada vista pone el suyo.</summary>
        public virtual string Titulo => "";

        /// <summary>Tecla que lleva a esta sección desde cualquier lugar (Keys.None si no tiene).</summary>
        public virtual Keys Atajo => Keys.None;

        /// <summary>Se llama cada vez que la vista pasa a primer plano: refrescar datos y ubicar el cursor.</summary>
        public virtual void AlMostrar() { }
    }
}

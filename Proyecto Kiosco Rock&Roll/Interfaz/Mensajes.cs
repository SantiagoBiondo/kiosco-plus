namespace Proyecto_Kiosco_Rock_Roll.Interfaz
{
    /// <summary>Cuadros de diálogo comunes, para no repetir MessageBox.Show en todos lados.</summary>
    public static class Mensajes
    {
        private const string Titulo = "Kiosco+";

        public static void Advertencia(IWin32Window? duenio, string texto) =>
            MessageBox.Show(duenio, texto, Titulo, MessageBoxButtons.OK, MessageBoxIcon.Warning);

        public static void Informacion(IWin32Window? duenio, string texto) =>
            MessageBox.Show(duenio, texto, Titulo, MessageBoxButtons.OK, MessageBoxIcon.Information);

        public static void Error(IWin32Window? duenio, string texto) =>
            MessageBox.Show(duenio, texto, Titulo, MessageBoxButtons.OK, MessageBoxIcon.Error);

        public static bool Confirmar(IWin32Window? duenio, string pregunta) =>
            MessageBox.Show(duenio, pregunta, Titulo, MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes;
    }
}

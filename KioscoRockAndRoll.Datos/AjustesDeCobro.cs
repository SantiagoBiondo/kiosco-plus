using System.Globalization;
using KioscoRockAndRoll.Dominio.Entidades;
using KioscoRockAndRoll.Dominio.Repositorios;

namespace KioscoRockAndRoll.Datos;

/// <summary>
/// Las reglas para cobrar por transferencia (recargo por paquete de un rubro y monto mínimo).
/// Mientras no se cambien en Configuración, vale lo que pidió el kiosco: $200 por paquete de
/// la categoría "Cigarrillos" (si existe) y transferencias desde $500.
/// </summary>
public sealed class AjustesDeCobro(Ajustes ajustes, ICatalogoRepositorio<Categoria> categorias)
{
    private const string ClaveCategoria = "Cobro.CategoriaConRecargo";
    private const string ClaveRecargo = "Cobro.RecargoPorPaquete";
    private const string ClaveMinimo = "Cobro.MinimoTransferencia";
    private const string SinCategoria = "ninguna";

    public const string CategoriaPorDefecto = "Cigarrillos";
    public const decimal RecargoPorDefecto = 200;
    public const decimal MinimoPorDefecto = 500;

    public ReglasDeCobro Leer() => new(
        LeerCategoria(),
        LeerMonto(ClaveRecargo, RecargoPorDefecto),
        LeerMonto(ClaveMinimo, MinimoPorDefecto));

    public void Guardar(ReglasDeCobro reglas)
    {
        if (reglas.RecargoPorPaquete < 0 || reglas.MinimoTransferencia < 0)
            throw new ArgumentOutOfRangeException(nameof(reglas), "Los montos no pueden ser negativos.");
        ajustes.Guardar(ClaveCategoria, reglas.CategoriaConRecargo?.ToString(CultureInfo.InvariantCulture) ?? SinCategoria);
        ajustes.Guardar(ClaveRecargo, reglas.RecargoPorPaquete.ToString(CultureInfo.InvariantCulture));
        ajustes.Guardar(ClaveMinimo, reglas.MinimoTransferencia.ToString(CultureInfo.InvariantCulture));
    }

    private long? LeerCategoria()
    {
        var guardada = ajustes.Leer(ClaveCategoria);
        if (guardada == SinCategoria)
            return null;
        if (long.TryParse(guardada, NumberStyles.Integer, CultureInfo.InvariantCulture, out var id))
            return id;
        // Nunca se configuró: se usa la categoría "Cigarrillos" si existe.
        return categorias.Listar()
            .FirstOrDefault(c => string.Equals(c.Nombre, CategoriaPorDefecto, StringComparison.CurrentCultureIgnoreCase))?.Id;
    }

    private decimal LeerMonto(string clave, decimal porDefecto) =>
        decimal.TryParse(ajustes.Leer(clave), NumberStyles.Number, CultureInfo.InvariantCulture, out var monto) ? monto : porDefecto;
}

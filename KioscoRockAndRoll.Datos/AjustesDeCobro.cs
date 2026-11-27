using System.Globalization;
using KioscoRockAndRoll.Dominio.Entidades;
using KioscoRockAndRoll.Dominio.Repositorios;

namespace KioscoRockAndRoll.Datos;

/// <summary>
/// Las reglas para cobrar por transferencia (recargo por paquete de cada rubro y monto mínimo).
/// Mientras no se cambien en Configuración, vale lo que pidió el kiosco: $200 por paquete de
/// la categoría "Cigarrillos" (si existe) y transferencias desde $500.
/// </summary>
public sealed class AjustesDeCobro(Ajustes ajustes, ICatalogoRepositorio<Categoria> categorias)
{
    // Los rubros con recargo, como "3:200;7:300" (rubro:monto). Vacío = ninguno.
    private const string ClaveRecargos = "Cobro.Recargos";
    private const string ClaveMinimo = "Cobro.MinimoTransferencia";

    // Hasta la versión 1.8 había un solo rubro con recargo, guardado así.
    private const string ClaveCategoriaAnterior = "Cobro.CategoriaConRecargo";
    private const string ClaveRecargoAnterior = "Cobro.RecargoPorPaquete";
    private const string SinCategoriaAnterior = "ninguna";

    public const string CategoriaPorDefecto = "Cigarrillos";
    public const decimal RecargoPorDefecto = 200;
    public const decimal MinimoPorDefecto = 500;

    public ReglasDeCobro Leer() => new(LeerRecargos(), LeerMonto(ClaveMinimo, MinimoPorDefecto));

    public void Guardar(ReglasDeCobro reglas)
    {
        var recargos = string.Join(";", reglas.Recargos.Select(r =>
            $"{r.CategoriaId.ToString(CultureInfo.InvariantCulture)}:{r.PorPaquete.ToString(CultureInfo.InvariantCulture)}"));
        ajustes.Guardar(ClaveRecargos, recargos);
        ajustes.Guardar(ClaveMinimo, reglas.MinimoTransferencia.ToString(CultureInfo.InvariantCulture));
        // Lo de antes ya quedó pasado al formato nuevo.
        ajustes.Guardar(ClaveCategoriaAnterior, null);
        ajustes.Guardar(ClaveRecargoAnterior, null);
    }

    private IEnumerable<RecargoDeRubro> LeerRecargos()
    {
        if (ajustes.Leer(ClaveRecargos) is { } guardados)
        {
            foreach (var par in guardados.Split(';', StringSplitOptions.RemoveEmptyEntries))
            {
                var partes = par.Split(':');
                if (partes.Length == 2
                    && long.TryParse(partes[0], NumberStyles.Integer, CultureInfo.InvariantCulture, out var categoria)
                    && decimal.TryParse(partes[1], NumberStyles.Number, CultureInfo.InvariantCulture, out var monto))
                    yield return new RecargoDeRubro(categoria, monto);
            }
            yield break;
        }

        // Todavía no se guardó con el formato nuevo: lo de la versión anterior, o lo que pidió el kiosco.
        var recargo = LeerMonto(ClaveRecargoAnterior, RecargoPorDefecto);
        var categoriaAnterior = ajustes.Leer(ClaveCategoriaAnterior);
        if (categoriaAnterior == SinCategoriaAnterior)
            yield break;
        if (long.TryParse(categoriaAnterior, NumberStyles.Integer, CultureInfo.InvariantCulture, out var id))
        {
            yield return new RecargoDeRubro(id, recargo);
            yield break;
        }
        if (categorias.Listar().FirstOrDefault(c =>
                string.Equals(c.Nombre, CategoriaPorDefecto, StringComparison.CurrentCultureIgnoreCase)) is { } cigarrillos)
            yield return new RecargoDeRubro(cigarrillos.Id, recargo);
    }

    private decimal LeerMonto(string clave, decimal porDefecto) =>
        decimal.TryParse(ajustes.Leer(clave), NumberStyles.Number, CultureInfo.InvariantCulture, out var monto) ? monto : porDefecto;
}

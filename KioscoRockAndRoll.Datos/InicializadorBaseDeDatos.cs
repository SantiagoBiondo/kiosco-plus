namespace KioscoRockAndRoll.Datos;

/// <summary>
/// Crea la base si no existe y la pone al día aplicando las migraciones que le falten.
/// La versión del esquema se guarda en la base misma (PRAGMA user_version), así una base
/// vieja se actualiza sola al abrir el programa, sin perder datos.
/// </summary>
/// <param name="backups">Si se indica, antes de migrar una base existente se guarda una copia.</param>
public sealed class InicializadorBaseDeDatos(ConexionSqlite conexion, GestorDeBackups? backups = null)
{
    /// <summary>
    /// Cada migración lleva la base de la versión N-1 a la N. Nunca se modifica una que ya
    /// se usó: los cambios nuevos van en una migración nueva al final.
    /// </summary>
    internal static readonly IReadOnlyList<string> Migraciones =
    [
        // 1: esquema inicial. Los montos se guardan en centavos (enteros) para no tener errores de redondeo.
        """
        CREATE TABLE IF NOT EXISTS Productos (
            Id                  INTEGER PRIMARY KEY AUTOINCREMENT,
            CodigoBarras        TEXT    NOT NULL UNIQUE,
            Nombre              TEXT    NOT NULL,
            PrecioCostoCentavos INTEGER NOT NULL,
            PrecioVentaCentavos INTEGER NOT NULL,
            Stock               INTEGER NOT NULL,
            StockMinimo         INTEGER NOT NULL
        );

        CREATE TABLE IF NOT EXISTS Ventas (
            Id        INTEGER PRIMARY KEY AUTOINCREMENT,
            Fecha     TEXT    NOT NULL,
            MedioPago INTEGER NOT NULL
        );

        CREATE TABLE IF NOT EXISTS VentaLineas (
            Id                     INTEGER PRIMARY KEY AUTOINCREMENT,
            VentaId                INTEGER NOT NULL REFERENCES Ventas(Id),
            ProductoId             INTEGER NOT NULL REFERENCES Productos(Id),
            NombreProducto         TEXT    NOT NULL,
            Cantidad               INTEGER NOT NULL,
            PrecioUnitarioCentavos INTEGER NOT NULL
        );

        CREATE INDEX IF NOT EXISTS IX_Ventas_Fecha ON Ventas(Fecha);
        CREATE INDEX IF NOT EXISTS IX_VentaLineas_VentaId ON VentaLineas(VentaId);
        CREATE INDEX IF NOT EXISTS IX_VentaLineas_ProductoId ON VentaLineas(ProductoId);
        """,

        // 2: categorías y proveedores. Al borrar uno, sus productos quedan sin asignar (no se borran).
        """
        CREATE TABLE Categorias (
            Id     INTEGER PRIMARY KEY AUTOINCREMENT,
            Nombre TEXT    NOT NULL UNIQUE COLLATE NOCASE
        );

        CREATE TABLE Proveedores (
            Id       INTEGER PRIMARY KEY AUTOINCREMENT,
            Nombre   TEXT    NOT NULL UNIQUE COLLATE NOCASE,
            Telefono TEXT    NOT NULL DEFAULT ''
        );

        ALTER TABLE Productos ADD COLUMN CategoriaId INTEGER REFERENCES Categorias(Id) ON DELETE SET NULL;
        ALTER TABLE Productos ADD COLUMN ProveedorId INTEGER REFERENCES Proveedores(Id) ON DELETE SET NULL;

        CREATE INDEX IX_Productos_CategoriaId ON Productos(CategoriaId);
        CREATE INDEX IX_Productos_ProveedorId ON Productos(ProveedorId);
        """,

        // 3: ajustes del programa (por ejemplo, la carpeta extra de backups).
        """
        CREATE TABLE Ajustes (
            Clave TEXT PRIMARY KEY,
            Valor TEXT NOT NULL
        );
        """,

        // 4: turnos de caja y movimientos de efectivo. Cada venta nueva queda asociada a su turno
        // (las ventas anteriores a esta versión quedan sin turno).
        """
        CREATE TABLE Turnos (
            Id                      INTEGER PRIMARY KEY AUTOINCREMENT,
            Apertura                TEXT    NOT NULL,
            EfectivoInicialCentavos INTEGER NOT NULL,
            Cierre                  TEXT,
            EfectivoContadoCentavos INTEGER
        );

        -- Solo puede haber un turno abierto a la vez.
        CREATE UNIQUE INDEX UX_Turnos_UnoAbierto ON Turnos((Cierre IS NULL)) WHERE Cierre IS NULL;

        CREATE TABLE MovimientosCaja (
            Id            INTEGER PRIMARY KEY AUTOINCREMENT,
            TurnoId       INTEGER NOT NULL REFERENCES Turnos(Id),
            Fecha         TEXT    NOT NULL,
            Tipo          INTEGER NOT NULL,
            MontoCentavos INTEGER NOT NULL,
            Motivo        TEXT    NOT NULL
        );

        CREATE INDEX IX_MovimientosCaja_TurnoId ON MovimientosCaja(TurnoId);

        ALTER TABLE Ventas ADD COLUMN TurnoId INTEGER REFERENCES Turnos(Id);
        CREATE INDEX IX_Ventas_TurnoId ON Ventas(TurnoId);
        """,

        // 5: anulación de ventas. La venta no se borra: queda marcada con la fecha y el motivo.
        """
        ALTER TABLE Ventas ADD COLUMN AnuladaEn TEXT;
        ALTER TABLE Ventas ADD COLUMN MotivoAnulacion TEXT;
        """,

        // 6: costo de cada renglón al momento de vender, para calcular la ganancia real.
        // Para las ventas anteriores no se sabe: se usa el costo actual del producto como estimación.
        """
        ALTER TABLE VentaLineas ADD COLUMN CostoUnitarioCentavos INTEGER NOT NULL DEFAULT 0;

        UPDATE VentaLineas
        SET CostoUnitarioCentavos = COALESCE(
            (SELECT p.PrecioCostoCentavos FROM Productos p WHERE p.Id = VentaLineas.ProductoId), 0);
        """,

        // 7: una venta se puede pagar con varios medios. Cada parte va en VentaPagos;
        // Ventas.MedioPago queda como el medio principal (el de mayor monto).
        // Las ventas anteriores pasan a tener un solo pago por el total.
        """
        CREATE TABLE VentaPagos (
            Id            INTEGER PRIMARY KEY AUTOINCREMENT,
            VentaId       INTEGER NOT NULL REFERENCES Ventas(Id),
            Medio         INTEGER NOT NULL,
            MontoCentavos INTEGER NOT NULL
        );

        CREATE INDEX IX_VentaPagos_VentaId ON VentaPagos(VentaId);

        INSERT INTO VentaPagos (VentaId, Medio, MontoCentavos)
        SELECT v.Id, v.MedioPago,
               COALESCE((SELECT SUM(l.Cantidad * l.PrecioUnitarioCentavos) FROM VentaLineas l WHERE l.VentaId = v.Id), 0)
        FROM Ventas v;
        """,

        // 8: productos que se venden sueltos, sacados de un paquete (ej.: cigarrillo suelto de un atado).
        """
        ALTER TABLE Productos ADD COLUMN PaqueteId INTEGER REFERENCES Productos(Id) ON DELETE SET NULL;
        ALTER TABLE Productos ADD COLUMN UnidadesPorPaquete INTEGER NOT NULL DEFAULT 0;
        """,

        // 9: productos con el precio puesto al vender (ej.: caramelos por peso). No llevan control de stock.
        """
        ALTER TABLE Productos ADD COLUMN PrecioAlVender INTEGER NOT NULL DEFAULT 0;
        """
    ];

    public static int VersionActual => Migraciones.Count;

    public void Inicializar()
    {
        var carpeta = Path.GetDirectoryName(conexion.RutaArchivo);
        if (!string.IsNullOrEmpty(carpeta))
            Directory.CreateDirectory(carpeta);

        using var db = conexion.Abrir();
        var version = LeerVersion(db);
        if (version > VersionActual)
            throw new InvalidOperationException(
                "La base de datos es de una versión más nueva del programa. Actualizá el programa para abrirla.");

        // Una base con datos que se va a actualizar: primero, una copia por las dudas.
        if (version > 0 && version < VersionActual)
            backups?.HacerCopiaEspecial($"antes-de-migrar-v{version}");

        // Cada migración va en su propia transacción junto con el cambio de versión:
        // si falla, la base queda como estaba.
        for (var siguiente = version + 1; siguiente <= VersionActual; siguiente++)
        {
            using var transaccion = db.BeginTransaction();
            using var comando = db.CreateCommand();
            comando.CommandText = $"{Migraciones[siguiente - 1]};\nPRAGMA user_version = {siguiente};";
            comando.ExecuteNonQuery();
            transaccion.Commit();
        }
    }

    internal static int LeerVersion(Microsoft.Data.Sqlite.SqliteConnection db)
    {
        using var comando = db.CreateCommand();
        comando.CommandText = "PRAGMA user_version";
        return Convert.ToInt32(comando.ExecuteScalar());
    }
}

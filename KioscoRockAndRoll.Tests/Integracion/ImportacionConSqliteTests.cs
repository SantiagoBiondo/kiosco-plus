using System.IO.Compression;
using System.Text;
using KioscoRockAndRoll.Datos;
using KioscoRockAndRoll.Dominio;
using KioscoRockAndRoll.Dominio.Entidades;
using KioscoRockAndRoll.Dominio.Servicios;

namespace KioscoRockAndRoll.Tests.Integracion;

public sealed class ImportacionConSqliteTests : IDisposable
{
    private readonly BaseDeDatosDePrueba _base = new();
    private readonly string _carpeta = Path.Combine(Path.GetTempPath(), "kiosco-importacion-" + Guid.NewGuid().ToString("N"));
    private readonly InventarioServicio _inventario;
    private readonly CatalogoServicio<Categoria> _categorias;
    private readonly CatalogoServicio<Proveedor> _proveedores;
    private readonly ImportadorDeProductos _importador;

    public ImportacionConSqliteTests()
    {
        Directory.CreateDirectory(_carpeta);
        var productos = new SqliteProductoRepositorio(_base.Conexion);
        _inventario = new InventarioServicio(productos);
        _categorias = new CatalogoServicio<Categoria>(new SqliteCategoriaRepositorio(_base.Conexion));
        _proveedores = new CatalogoServicio<Proveedor>(new SqliteProveedorRepositorio(_base.Conexion));
        _importador = new ImportadorDeProductos(productos, _categorias, _proveedores);
    }

    public void Dispose()
    {
        _base.Dispose();
        Directory.Delete(_carpeta, recursive: true);
    }

    private string Archivo(string nombre, string contenido, Encoding? codificacion = null)
    {
        var ruta = Path.Combine(_carpeta, nombre);
        File.WriteAllText(ruta, contenido, codificacion ?? new UTF8Encoding(true));
        return ruta;
    }

    private VistaPreviaImportacion Previa(TablaImportada tabla, SiElCodigoYaExiste siYaExiste = SiElCodigoYaExiste.Actualizar) =>
        _importador.Previsualizar(tabla, MapeoDeColumnas.Detectar(tabla.Encabezados), siYaExiste);

    // ---------- Lectura de archivos ----------

    [Fact]
    public void Csv_ConPuntoYComaYComillas()
    {
        var tabla = LectorDePlanillas.Leer(Archivo("productos.csv",
            "Código;Descripción;Precio\r\n779001;\"Alfajor \"\"triple\"\"; chocolate\";880\r\n779002;Coca 500;1.650,00\r\n"));

        Assert.Equal(["Código", "Descripción", "Precio"], tabla.Encabezados);
        Assert.Equal(2, tabla.Filas.Count);
        Assert.Equal("Alfajor \"triple\"; chocolate", tabla.Filas[0][1]);
        Assert.Equal("1.650,00", tabla.Filas[1][2]);
    }

    [Fact]
    public void Csv_ConComasYLineaSepDeExcel()
    {
        var tabla = LectorDePlanillas.Leer(Archivo("productos.csv", "sep=,\ncodigo,nombre,venta\n1,Chicle,100\n"));

        Assert.Equal(["codigo", "nombre", "venta"], tabla.Encabezados);
        Assert.Equal(["1", "Chicle", "100"], tabla.Filas[0]);
    }

    [Fact]
    public void Csv_GuardadoPorExcelEnCastellano_RespetaTildesYEñes()
    {
        var tabla = LectorDePlanillas.Leer(Archivo("productos.csv", "Código;Nombre\n1;Ñoquis de papá\n", Encoding.Latin1));

        Assert.Equal("Código", tabla.Encabezados[0]);
        Assert.Equal("Ñoquis de papá", tabla.Filas[0][1]);
    }

    [Fact]
    public void Xls_PideGuardarloComoXlsxOCsv()
    {
        var ex = Assert.Throws<ReglaDeNegocioException>(() => LectorDePlanillas.Leer(Archivo("viejo.xls", "x")));
        Assert.Contains(".xlsx", ex.Message);
    }

    [Fact]
    public void Xlsx_LeeTextosNumerosYCodigosLargos()
    {
        var ruta = Path.Combine(_carpeta, "productos.xlsx");
        CrearXlsx(ruta);

        var tabla = LectorDePlanillas.Leer(ruta);

        Assert.Equal(["Código", "Descripción", "Precio", "Stock"], tabla.Encabezados);
        Assert.Equal(["7790895000997", "Coca-Cola 500", "1650,5", "12"], tabla.Filas[0]);
        // La fila 3 no tiene descripción (Excel no guarda la celda): queda vacía en su lugar.
        Assert.Equal(["779002", "", "880", ""], tabla.Filas[1]);
    }

    /// <summary>Un .xlsx mínimo como el que guarda Excel: textos compartidos, un número en notación científica y una celda salteada.</summary>
    private static void CrearXlsx(string ruta)
    {
        const string ns = "http://schemas.openxmlformats.org/spreadsheetml/2006/main";
        using var zip = ZipFile.Open(ruta, ZipArchiveMode.Create);
        void Escribir(string nombre, string xml)
        {
            using var escritor = new StreamWriter(zip.CreateEntry(nombre).Open(), new UTF8Encoding(false));
            escritor.Write(xml);
        }
        Escribir("xl/workbook.xml", $"""
            <workbook xmlns="{ns}" xmlns:r="http://schemas.openxmlformats.org/officeDocument/2006/relationships">
              <sheets><sheet name="Productos" sheetId="1" r:id="rId1"/></sheets>
            </workbook>
            """);
        Escribir("xl/_rels/workbook.xml.rels", """
            <Relationships xmlns="http://schemas.openxmlformats.org/package/2006/relationships">
              <Relationship Id="rId1" Type="worksheet" Target="worksheets/hoja1.xml"/>
            </Relationships>
            """);
        Escribir("xl/sharedStrings.xml", $"""
            <sst xmlns="{ns}"><si><t>Código</t></si><si><t>Descripción</t></si><si><t>Precio</t></si>
              <si><r><t>Coca-Cola</t></r><r><t xml:space="preserve"> 500</t></r></si><si><t>Stock</t></si></sst>
            """);
        Escribir("xl/worksheets/hoja1.xml", $"""
            <worksheet xmlns="{ns}"><sheetData>
              <row r="1"><c r="A1" t="s"><v>0</v></c><c r="B1" t="s"><v>1</v></c><c r="C1" t="s"><v>2</v></c><c r="D1" t="s"><v>4</v></c></row>
              <row r="2"><c r="A2"><v>7.790895000997E+12</v></c><c r="B2" t="s"><v>3</v></c><c r="C2"><v>1650.5</v></c><c r="D2"><v>12</v></c></row>
              <row r="3"><c r="A3" t="inlineStr"><is><t>779002</t></is></c><c r="C3"><v>880</v></c></row>
            </sheetData></worksheet>
            """);
    }

    // ---------- Vista previa ----------

    [Fact]
    public void VistaPrevia_DiceQuePasaConCadaFila()
    {
        _inventario.Crear(new DatosProducto("779002", "Coca vieja", 900, 1500, 10, 0));
        var tabla = LectorDePlanillas.Leer(Archivo("p.csv",
            "Código;Nombre;Costo;Precio;Stock\n" +
            "779001;Alfajor;500;880;20\n" +       // 2: nuevo
            "779002;Coca 500;1000;1650;\n" +      // 3: ya existe → se actualiza
            ";Sin código;1;2;3\n" +               // 4: error
            "779001;Repetido;1;2;3\n" +           // 5: error (ya está en la fila 2)
            "779003;Caro;;mucho;1\n" +            // 6: error de precio
            "779004;Negativo;1;2;-5\n" +          // 7: error de stock
            ";;;;\n" +                            // fila vacía: se ignora
            "779005;Sin costo;;300;\n"));         // 9: nuevo, sin costo ni stock

        var previa = Previa(tabla);

        Assert.Equal(
            [EstadoFilaImportada.Nuevo, EstadoFilaImportada.Actualiza, EstadoFilaImportada.Error,
             EstadoFilaImportada.Error, EstadoFilaImportada.Error, EstadoFilaImportada.Error, EstadoFilaImportada.Nuevo],
            previa.Filas.Select(f => f.Estado));
        Assert.Equal([2, 3, 4, 5, 6, 7, 9], previa.Filas.Select(f => f.NumeroDeFila));
        Assert.Contains("fila 2", previa.Filas[3].Error);
        Assert.Equal(2, previa.Nuevos);
        Assert.Equal(1, previa.Actualizados);
        Assert.Equal(4, previa.ConError);
    }

    [Fact]
    public void VistaPrevia_SiSeEligeNoTocarLosExistentes()
    {
        _inventario.Crear(new DatosProducto("779002", "Coca", 900, 1500, 10, 0));
        var tabla = new TablaImportada(["codigo", "nombre", "precio"], [["779002", "Coca nueva", "2000"]]);

        var previa = Previa(tabla, SiElCodigoYaExiste.DejarComoEsta);

        Assert.Equal(EstadoFilaImportada.YaExiste, Assert.Single(previa.Filas).Estado);
        Assert.False(previa.HayAlgoParaImportar);
    }

    [Fact]
    public void VistaPrevia_SinLasColumnasObligatorias_Avisa()
    {
        var tabla = new TablaImportada(["codigo", "nombre"], [["1", "Chicle"]]);

        var ex = Assert.Throws<ReglaDeNegocioException>(() => Previa(tabla));
        Assert.Contains("Precio de venta", ex.Message);
    }

    // ---------- Importar ----------

    [Fact]
    public void Importar_CreaProductosRubrosYProveedores()
    {
        _categorias.Guardar(new Categoria("Golosinas"));
        var tabla = new TablaImportada(["Código", "Nombre", "Costo", "Precio", "Stock", "Rubro", "Proveedor"],
        [
            ["779001", "Alfajor", "500", "880", "20", "GOLOSINAS", "Arcor"],
            ["779002", "Chicle", "50", "100", "", "golosinas", "arcor"],
            ["779003", "Marlboro", "4000", "5000", "8", "Cigarrillos", ""]
        ]);

        var resultado = _importador.Importar(Previa(tabla));

        Assert.Equal(new ResultadoImportacion(3, 0, CategoriasCreadas: 1, ProveedoresCreados: 1), resultado);
        Assert.Equal(["Cigarrillos", "Golosinas"], _categorias.Listar().Select(c => c.Nombre));
        Assert.Equal(["Arcor"], _proveedores.Listar().Select(p => p.Nombre));

        var alfajor = _inventario.ObtenerPorCodigo("779001")!;
        Assert.Equal(("Alfajor", 500m, 880m, 20), (alfajor.Nombre, alfajor.PrecioCosto, alfajor.PrecioVenta, alfajor.Stock));
        var golosinas = _categorias.Listar().Single(c => c.Nombre == "Golosinas").Id;
        Assert.Equal(golosinas, alfajor.CategoriaId);
        Assert.Equal(golosinas, _inventario.ObtenerPorCodigo("779002")!.CategoriaId);
        Assert.Equal(0, _inventario.ObtenerPorCodigo("779002")!.Stock);
        Assert.Null(_inventario.ObtenerPorCodigo("779003")!.ProveedorId);
    }

    [Fact]
    public void Importar_AlActualizar_LoQueLaPlanillaNoTraeSeConserva()
    {
        var existente = _inventario.Crear(new DatosProducto("779002", "Coca", 900, 1500, 10, 3));
        var tabla = new TablaImportada(["Código", "Nombre", "Precio"], [["779002", "Coca-Cola 500", "1650"]]);

        var resultado = _importador.Importar(Previa(tabla));

        Assert.Equal(1, resultado.Actualizados);
        var coca = _inventario.ObtenerPorId(existente.Id)!;
        Assert.Equal(("Coca-Cola 500", 1650m), (coca.Nombre, coca.PrecioVenta));
        Assert.Equal((900m, 10, 3), (coca.PrecioCosto, coca.Stock, coca.StockMinimo));
    }

    [Fact]
    public void Importar_LasFilasConErrorNoSeImportan()
    {
        var tabla = new TablaImportada(["Código", "Nombre", "Precio"], [["1", "Chicle", "100"], ["2", "", "50"]]);

        var resultado = _importador.Importar(Previa(tabla));

        Assert.Equal(1, resultado.Nuevos);
        Assert.Null(_inventario.ObtenerPorCodigo("2"));
    }

    [Fact]
    public void Importar_SinNadaParaImportar_Avisa()
    {
        var tabla = new TablaImportada(["Código", "Nombre", "Precio"], [["", "Chicle", "100"]]);

        Assert.Throws<ReglaDeNegocioException>(() => _importador.Importar(Previa(tabla)));
    }

    [Fact]
    public void Importar_DosMilProductos_EnUnaSolaOperacion()
    {
        var filas = Enumerable.Range(1, 2000)
            .Select(i => (IReadOnlyList<string>)[$"77900{i:D5}", $"Producto {i}", "100", "150", "5"]).ToList();
        var tabla = new TablaImportada(["Código", "Nombre", "Costo", "Precio", "Stock"], filas);

        var resultado = _importador.Importar(Previa(tabla));

        Assert.Equal(2000, resultado.Nuevos);
        Assert.Equal(2000, _inventario.Buscar("").Count);
    }
}

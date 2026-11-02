# Kiosco+ — guía de instalación y uso

Programa de ventas, inventario y caja del Kiosco Rock & Roll. (Para una presentación del proyecto, ver [README.md](README.md).)

## Instalar en la computadora del kiosco

**Requisitos:** Windows 10 u 11 de 64 bits. No hace falta instalar nada más: el programa trae todo adentro.

1. Generá el ejecutable (ver [Generar el ejecutable](#generar-el-ejecutable)) o usá el que ya está en la carpeta `publicar`.
2. Copiá `KioscoPlus.exe` a la computadora del kiosco, por ejemplo a `C:\Kiosco\`.
3. Clic derecho sobre el archivo → **Enviar a** → **Escritorio (crear acceso directo)**.
4. La primera vez, Windows puede mostrar *"Windows protegió su PC"* porque el programa no está firmado digitalmente.
   Hacé clic en **Más información** → **Ejecutar de todas formas**. Solo pasa la primera vez.

### El lector de códigos de barras

Funciona con cualquier lector USB que trabaje "como teclado" (la gran mayoría viene así de fábrica):
el lector escribe el código y aprieta Enter. Para probarlo, abrí el Bloc de notas y escaneá algo:
tiene que aparecer el número y pasar a la línea siguiente.

## Dónde quedan los datos

| Qué | Dónde |
|---|---|
| Base de datos (productos, ventas, caja) | `%LOCALAPPDATA%\KioscoRockAndRoll\kiosco.db` |
| Backups automáticos | `%LOCALAPPDATA%\KioscoRockAndRoll\backups\` (y la carpeta extra si la configurás) |
| PDF exportados | `Documentos\Kiosco Rock & Roll\Ventas\` |

Para abrir `%LOCALAPPDATA%`: tecla Windows + R, escribí `%LOCALAPPDATA%` y Enter.

**Recomendado:** en la sección **Backups** elegí una carpeta extra dentro de Google Drive u OneDrive,
así una copia de los datos queda fuera de la computadora del kiosco.

## Actualizar a una versión nueva

1. Cerrá el programa.
2. Reemplazá `KioscoPlus.exe` por el nuevo.

> **Si venías usando `KioscoRockAndRoll.exe`:** desde que el programa se llama Kiosco+, el archivo es
> `KioscoPlus.exe`. Borrá el `.exe` viejo y su acceso directo del escritorio, copiá el nuevo y creale
> un acceso directo como en la instalación. Los datos siguen donde estaban.

Los datos no se tocan, porque están en otra carpeta. Si la versión nueva necesita actualizar la base,
lo hace sola al abrir y antes guarda una copia de seguridad.

## Pasar todo a otra computadora

1. Cerrá el programa en la computadora vieja.
2. Copiá la carpeta `%LOCALAPPDATA%\KioscoRockAndRoll` completa a la misma ubicación en la computadora nueva.
3. Instalá el programa en la nueva como se explica arriba.

## Teclas rápidas

| Tecla | Dónde | Qué hace |
|---|---|---|
| F1 / F2 / F3 / F4 | Siempre | Venta / Inventario / Ingreso de mercadería / Caja |
| Enter | Venta | Agrega el producto escaneado (`3*código` agrega 3 unidades) |
| F5 | Venta | Buscar un producto por nombre |
| F12 | Venta | Cobrar |
| Supr | Venta | Quitar el renglón elegido |
| Insert | Inventario | Nuevo producto |
| F6 | Inventario | Actualizar precios por porcentaje (de lo que se ve en la lista) |
| F7 | Inventario | Armar pedido a un proveedor |
| F8 / F9 | Inventario | Categorías / Proveedores |
| Enter / Esc | Paneles de la derecha | Confirmar / Cancelar |

## Para el programador

### Generar el ejecutable

- **Visual Studio:** clic derecho en el proyecto *Proyecto Kiosco Rock&Roll* → **Publicar** → perfil **Kiosco** → **Publicar**.
- **Terminal**, en la carpeta de la solución:

  ```
  dotnet publish "Proyecto Kiosco Rock&Roll" -p:PublishProfile=Kiosco
  ```

El resultado es un único `publicar\KioscoPlus.exe`. Antes de publicar una versión nueva,
subí el número de `<Version>` en `Proyecto Kiosco Rock&Roll.csproj`.

### Probar sin tocar los datos reales

```
KioscoPlus.exe --base practica.db
```

Usa (o crea) otra base de datos, ideal para practicar o mostrar el programa.

### Estructura de la solución

| Proyecto | Qué tiene |
|---|---|
| `KioscoRockAndRoll.Dominio` | Reglas del negocio: entidades, interfaces de repositorios y servicios. No depende de nada. |
| `KioscoRockAndRoll.Datos` | Repositorios con SQLite, migraciones de la base y backups. |
| `KioscoRockAndRoll.Reportes` | PDF de ventas y reportes (QuestPDF). |
| `Proyecto Kiosco Rock&Roll` | La aplicación de escritorio (WinForms). |
| `KioscoRockAndRoll.Tests` | Tests unitarios y de integración (xUnit). Se corren con `dotnet test`. |

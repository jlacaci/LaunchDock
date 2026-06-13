# Changelog — LaunchDock

Todos los cambios relevantes del proyecto se documentan aquí.  
Formato: `[versión] — fecha`

---

## [1.7.0] — 2025-07

### 🐛 Bugs arreglados

- **Soporte multiidioma (es, en, fr, it)**: los archivos de idioma XAML tenían el carácter de reemplazo Unicode `U+FFFD` (rombo con interrogante) en lugar de ñ, acentos y otros caracteres especiales. Corregida la codificación de los cuatro archivos (`es.xaml`, `en.xaml`, `fr.xaml`, `it.xaml`) a UTF-8 sin BOM con declaración `<?xml encoding="utf-8"?>`.
- **Emojis en la ventana de Configuración aparecían como `??`**: al rescribir los archivos de idioma, los emojis fuera del BMP (📦 🗑️ 🧹 🎨 📤 📥…) se almacenaban como dos signos de interrogación ASCII. Sustituidos por XML character references (`&#x1F4E6;`, `&#x1F5D1;`, etc.) que son independientes de la codificación de la terminal y garantizan representación correcta en cualquier entorno.

---

## [1.6.0] — 2025

### ✨ Nuevas funciones

- **Multi-barra (barras simultáneas)**: botón `＋` en la barra principal para crear barras adicionales independientes. Cada barra tiene su propia configuración, tema y colores. Las barras secundarias se restauran automáticamente al iniciar LaunchDock. Se pueden cerrar desde Configuración → `🗑 Cerrar esta barra` con mensaje de confirmación.
- **Color fondo menú desplegable configurable**: nuevo campo en Configuración → Colores para personalizar el fondo de los menús de categorías. Los temas predefinidos incluyen su valor correspondiente.
- **Esquinas redondeadas realmente transparentes**: los menús desplegables y la ventana de Configuración ya no muestran píxeles de fondo visibles en las esquinas redondeadas. Se aplica un recorte geométrico preciso (`Clip`) acorde al `CornerRadius`.
- **Botones de acción en modo vertical reubicados**: en orientación vertical (Left/Right) los botones ✏ ⚙ ＋ se muestran debajo de las categorías en lugar de al lado, aprovechando mejor el espacio de la barra estrecha.
- **Versión visible en Configuración**: la versión actual del programa aparece en la esquina inferior izquierda de la ventana de Configuración, sincronizada automáticamente con el número de versión del ensamblado.
- **Panel del sistema se abre al pasar el ratón**: el menú del panel del sistema ahora se despliega al pasar el ratón por encima, igual que los menús de categorías, y se cierra al salir.

### 🐛 Bugs arreglados

- **Botón ⚙ horizontal visible en modo vertical**: el botón de configuración del panel horizontal no se ocultaba al cambiar a orientación vertical. Corregido asignándole nombre `SettingsBtn` y controlando su visibilidad desde `ApplyOrientation`.

---

## [1.5.2] — 2025

### 🐛 Bugs arreglados / Mejoras visuales

- **Icono del Panel del sistema ahora visible en todos los temas**: el icono `⊞` Unicode no se veía correctamente en fondos oscuros. Reemplazado por un icono del logo de Windows dibujado en XAML puro con 4 rectángulos. Usa el mismo `Foreground` que el lápiz y el engranaje, cambia al color de acento al hacer hover y es independiente de archivos externos.

---

## [1.5.1] — 2025

### 🐛 Bugs arreglados

- **Transmission (y apps similares) abría el `.ico` en lugar del programa**: `ResolveShortcut` aceptaba cualquier archivo que existiera en disco como ejecutable válido, incluyendo archivos `.ico` referenciados en el `.lnk`. Ahora se valida que el `TargetPath` sea realmente un ejecutable (`.exe`, `.bat`, `.cmd`, `.com`, `.msi`); si no lo es, se usa el `.lnk` original para que Windows lo resuelva correctamente con `UseShellExecute`.

---

## [1.5.0] — 2025

### 🐛 Bugs arreglados

- **Accesos directos UWP/Store se rompen al borrar el .lnk de origen**: al añadir apps como WhatsApp, Telegram, YouTube, Copilot u Outlook desde un acceso directo del escritorio, si ese acceso se borraba el dock dejaba de abrirlas. Ahora, si el `.lnk` no tiene ejecutable real (apps UWP), el archivo se copia automáticamente a `%AppData%\LaunchDock\shortcuts\` y se guarda esa ruta estable e independiente de la ubicación original.
- **Transmission abría el `.ico` en lugar del programa**: `ResolveShortcut` estaba leyendo la ruta del icono en vez del ejecutable real. Corregida la resolución del target para estos casos.

### ✨ Mejoras

- **Panel del sistema** (`⊞`): nuevo botón en la barra principal que despliega un menú con accesos rápidos del sistema:
  - Panel de control, Ejecutar, Buscar, Configuración de Windows
  - Administrador de tareas, Servicios
  - CMD, PowerShell
  - Apagar, Reiniciar, Suspender

---

## [1.4.0] — 2025

### ✨ Mejoras

- **Backup / Restore de configuraci
  - `?? Exportar` — guarda el `config.json` completo (categorías, accesos directos, colores, fuente, posición…) en la ruta que elija el usuario.
  - `?? Importar` — carga un `config.json` previamente exportado y lo aplica al reiniciar LaunchDock. Incluye confirmación antes de sobreescribir.

---



### ? Mejoras

- **5 temas de color predefinidos**: nueva sección TEMA en Configuración con botones de preset. Los temas disponibles son `Oscuro` (el clásico), `Drácula` (púrpura), `Nord` (azules fríos), `Claro` (fondo blanco) y `Neumórfico Oscuro` (violeta suave). Al seleccionar un tema se rellenan automáticamente los 3 campos de color; el usuario puede seguir ajustándolos manualmente.
- **Radio de esquinas configurable**: nuevo slider en Configuración (0–20 px). El radio se aplica inteligentemente según la posición de la barra: las esquinas pegadas al borde del monitor siempre quedan en 0 y solo se redondean las esquinas libres.
- **Indicador de tema activo**: el botón del tema seleccionado se resalta con el color de acento y negrita.

---



### ?? Bugs arreglados

- **Script de publicación se detenía si LaunchDock no estaba en ejecución**: `taskkill` devolvía error y `$ErrorActionPreference = "Stop"` abortaba el script antes de compilar. Reemplazado por `Stop-Process -ErrorAction SilentlyContinue`.
- **Versión del instalador siempre mostraba 1.0.0**: el parámetro `$Version` del script tenía `"1.0.0"` hardcodeado. Ahora se lee automáticamente del `<Version>` en `LaunchDock.csproj`.
- **Instaladores antiguos acumulados en `publish\Installer\`**: el script ahora borra los `.exe` anteriores antes de generar el nuevo.

### ? Mejoras

- **Categorías por defecto en instalación nueva**: los usuarios que instalan LaunchDock por primera vez ven 5 categorías de ejemplo: `OFIMÁTICA`, `INTERNET`, `FOTOGRAFÍA`, `VIDEO`, `AUDIO`.
- **Color de acento aplica a la línea de hover**: la línea de color que aparece al pasar el ratón sobre una categoría ahora respeta el color de acento configurado por el usuario en tiempo real (`DynamicResource`).

---



### ?? Bugs arreglados

- **Error catastrófico al añadir accesos directos UWP/Store**: al pulsar `+ Añadir acceso directo` y seleccionar un `.lnk` de apps como Clipchamp, el `OpenFileDialog` de WPF intentaba resolver el enlace automáticamente y el shell lanzaba `E_UNEXPECTED`. Reemplazado por el `OpenFileDialog` de WinForms con `DereferenceLinks = false`.

### ? Mejoras

- **Botón ? Modo edición en la barra**: añadido botón de lápiz junto al de configuración (?) para activar/desactivar el modo edición sin necesidad de menú contextual.
- **Modo edición limitado al ancho del monitor**: al activar el modo edición la ventana nunca se desborda fuera de los límites del monitor. Compatible con configuraciones multi-monitor gracias a detección DPI-aware con `Screen.FromPoint()`.
- **Tabs compactos en modo edición**: los botones de categoría se reducen (padding, fuente y tamaño de botones ???) al entrar en modo edición para maximizar el número de categorías visibles sin scroll.

---



### ?? Bugs arreglados

- **Iconos con flecha de acceso directo**: los iconos extraídos de accesos directos `.lnk` ya no muestran la flecha de overlay de Windows.
- **Fusion 360 icono incorrecto**: mostraba el icono de `FusionLauncher.exe` (cohete) en lugar del icono naranja con "F". Ahora se prioriza el icono personalizado definido en el `.lnk`.
- **Copilot icono con fondo negro**: al extraer el icono de apps UWP/Store, el fondo aparecía negro. Corregido leyendo el canal alpha premultiplicado (PARGB) directamente con `GetDIBits`.
- **Copilot icono con flecha**: las apps UWP no tienen `.exe` accesible, por lo que los métodos clásicos fallaban. Implementado `IShellItemImageFactory` con flag `SIIGBF_ICONONLY` para extraer sin overlay.
- **Menú contextual "Eliminar" no funcionaba**: el `MessageBox` de confirmación aparecía oculto detrás de otras ventanas en apps de bandeja del sistema. Eliminado el diálogo de confirmación, ahora elimina directamente.
- **Modo edición cortado a la derecha**: al tener muchas categorías, el botón `? Categoría` quedaba fuera de pantalla. Rediseñado el layout con `Grid` + `ScrollViewer` horizontal para que los botones de acción siempre sean visibles.
- **Cambio de orientación no se revertía correctamente**: al pasar de vertical a horizontal, la ventana mantenía el ancho estrecho del modo vertical. Corregido reseteando `Width` antes de que `PositionWindow` recalcule.
- **Script de publicación no incluía últimos cambios**: `dotnet publish` usaba artefactos en caché de `obj\`. Añadido `dotnet clean` antes de publicar.
- **Instalador Inno Setup no generaba el `.exe`**: la variable `{#MyAppExeName}` dentro de un `#define` no se expandía. Corregida la referencia directamente en la sección `[Files]`.

### ? Mejoras

- **Caché de iconos**: sistema de caché PNG en `%AppData%\LaunchDock\IconCache\` para guardar iconos sin flecha y reutilizarlos en arranques posteriores.
- **Botón "Limpiar caché" en Configuración**: permite regenerar todos los iconos desde la ventana de ajustes sin tocar archivos manualmente.
- **Scroll horizontal en la barra**: cuando hay muchas categorías, se pueden desplazar con la rueda del ratón.
- **Script de publicación mejorado** (`Scripts/Publish.bat`): limpia, compila, publica y genera el instalador de Inno Setup en un solo paso.

---

## [1.0.0] — 2025

### ?? Lanzamiento inicial

- Barra de lanzamiento rápido personalizable para Windows.
- Soporte para categorías con accesos directos.
- Iconos extraídos automáticamente de los ejecutables.
- Configuración de colores, fuente, tamaño de iconos y posición.
- Modos de orientación horizontal y vertical.
- Inicio automático con Windows (opcional).
- Publicación como ejecutable único autocontenido (Self-Contained .NET 8).

---

## Esquema de versiones

`Mayor.Menor.Parche`

| Número | Cuándo sube |
|--------|-------------|
| **Mayor** | Cambio grande o rediseño |
| **Menor** | Nuevas funcionalidades |
| **Parche** | Corrección de bugs |

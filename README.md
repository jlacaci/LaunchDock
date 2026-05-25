# LaunchDock 🚀

Barra de accesos directos categorizados para Windows — estilo barra de menús flotante.

---

## ✅ Requisitos

- Windows 10 / 11
- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) (para compilar)
- Visual Studio 2022+ **o** compilación por línea de comandos

---

## 🔨 Compilar y ejecutar

### Opción A — Visual Studio 2022
1. Abre `LaunchDock.csproj` en Visual Studio
2. Selecciona configuración **Release**
3. Pulsa `Ctrl+Shift+B` para compilar
4. El ejecutable quedará en `bin\Release\net8.0-windows\LaunchDock.exe`

### Opción B — Línea de comandos (PowerShell / CMD)
```powershell
cd LaunchDock
dotnet restore
dotnet build -c Release
dotnet run
```

### Opción C — Publicar como .exe único portátil
```powershell
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true
```
El resultado estará en `bin\Release\net8.0-windows\win-x64\publish\LaunchDock.exe`

---

## 🎯 Características

### Barra principal
- Se ancla en la **parte superior, inferior** de la pantalla o **flotante** (arrastrable)
- **Auto-ocultación**: se esconde dejando solo 4px visibles; reaparece al acercar el ratón
- Siempre encima del resto de ventanas (**always on top**)
- Iconos de los programas extraídos automáticamente

### Menús desplegables
- Se abren automáticamente al **pasar el ratón** sobre la categoría
- Se cierran cuando el ratón sale del desplegable (con pequeño retardo para comodidad)
- Scroll automático si hay muchos accesos directos

### Drag & Drop
- **Arrastra** cualquier archivo, carpeta o acceso directo (.lnk) del escritorio encima de una categoría
- El icono y nombre se añaden automáticamente
- Los accesos directos de Windows (.lnk) se resuelven al programa real

### Modo Edición (✏)
- Actívalo desde el **icono de bandeja del sistema** o doble clic en él
- **Renombrar categorías**: haz clic en el nombre
- **Reordenar categorías**: botones ◀ ▶ en modo edición
- **Reordenar accesos directos**: botones ▲ ▼ en cada ítem
- **Eliminar** categorías o accesos directos
- **Añadir accesos directos** con el botón ＋ o arrastrando desde el escritorio
- Los cambios se guardan automáticamente al salir del modo edición

### Configuración (⚙)
- Posición: Arriba / Abajo / Flotante
- Auto-ocultar: sí / no
- Tamaño de iconos: 16 / 32 / 48 px

---

## 📁 Archivos de configuración

La configuración y tus categorías se guardan en:
```
C:\Users\TuUsuario\AppData\Roaming\LaunchDock\config.json
```
Puedes editar este archivo a mano si lo necesitas.

---

## 🚀 Arrancar con Windows

Para que LaunchDock se inicie automáticamente con Windows:
1. Crea un acceso directo de `LaunchDock.exe`
2. Pulsa `Win + R` → escribe `shell:startup`
3. Mueve el acceso directo a esa carpeta

---

## 📦 Estructura del proyecto

```
LaunchDock/
├── App.xaml / App.xaml.cs          ← Punto de entrada + bandeja del sistema
├── Models/
│   └── Models.cs                   ← Modelos de datos (categorías, accesos)
├── Helpers/
│   ├── ConfigManager.cs            ← Carga/guarda config en JSON
│   └── IconHelper.cs               ← Extrae iconos de .exe, carpetas, .lnk
├── Views/
│   ├── MainBarWindow.xaml/.cs      ← Ventana principal de la barra
│   ├── CategoryControl.cs          ← Control de categoría + desplegable
│   └── SettingsWindow.xaml/.cs     ← Ventana de configuración
└── Resources/
    └── Styles.xaml                 ← Tema visual oscuro
```

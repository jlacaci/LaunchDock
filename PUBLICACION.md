# LaunchDock - Guía de Publicación e Instalación

## ?? Cómo crear el instalador de LaunchDock

### Opción 1: Publicación Rápida (Archivo Ejecutable Portable)

Esta es la forma más rápida de probar tu aplicación como un programa terminado.

#### Pasos:

1. **Abrir PowerShell en la carpeta del proyecto**
   - Haz clic derecho en la carpeta del proyecto ? "Abrir en Terminal"

2. **Ejecutar el script de publicación**
   ```powershell
   .\publish.ps1
   ```

3. **Seleccionar la opción 3** (Self-contained con archivo único) - Recomendado
   - Esta opción crea un solo archivo .exe que incluye todo lo necesario
   - No requiere instalar .NET en otras computadoras
   - Tamaño aproximado: 60-80 MB

4. **Ubicación del ejecutable**
   - Se creará en: `.\publish\LaunchDock-SingleFile\LaunchDock.exe`
   - Puedes copiar este archivo a cualquier computadora Windows y ejecutarlo directamente

#### Ventajas de la Opción 3:
- ? Un solo archivo .exe
- ? No requiere instalación
- ? Portable (puedes llevarlo en USB)
- ? Incluye todo lo necesario (.NET Runtime incluido)

---

### Opción 2: Crear Instalador Profesional con Inno Setup

Para crear un instalador tipo "Setup.exe" profesional:

#### Requisitos:
1. **Descargar e instalar Inno Setup**
   - Descarga desde: https://jrsoftware.org/isdl.php
   - Instala la versión más reciente (6.x)

#### Pasos:

1. **Primero, publicar la aplicación**
   ```powershell
   .\publish.ps1
   ```
   Selecciona la opción 3 (Self-contained con archivo único)

2. **Abrir Inno Setup Compiler**
   - Abre el archivo `LaunchDock-Setup.iss` con Inno Setup Compiler
   - O arrastra el archivo .iss al icono de Inno Setup

3. **Compilar el instalador**
   - En Inno Setup: Build ? Compile (F9)
   - O simplemente presiona F9

4. **Encontrar el instalador**
   - Se creará en: `.\publish\Installer\LaunchDock-Setup-1.0.0.exe`

#### Características del instalador:
- ? Instalador profesional de Windows
- ? Opción para iniciar con Windows
- ? Crear acceso directo en el escritorio
- ? Desinstalador incluido
- ? Instalación en Archivos de Programa
- ? Multiidioma (Español/Inglés)

---

## ?? Cómo usar el ejecutable

### Método 1: Archivo Portable (Sin instalación)

1. Navega a `.\publish\LaunchDock-SingleFile\`
2. Haz doble clic en `LaunchDock.exe`
3. ¡Listo! La aplicación se ejecutará

**Para que inicie con Windows (Portable):**
1. Presiona `Win + R`
2. Escribe: `shell:startup`
3. Crea un acceso directo de `LaunchDock.exe` en esa carpeta

### Método 2: Usando el Instalador

1. Ejecuta `LaunchDock-Setup-1.0.0.exe`
2. Sigue el asistente de instalación
3. Marca "Iniciar con Windows" si lo deseas
4. ¡Listo!

---

## ?? Configuración de LaunchDock

La aplicación guarda su configuración en:
- **Windows 10/11**: `C:\Users\[TuUsuario]\AppData\Roaming\LaunchDock\config.json`

Puedes respaldar este archivo para guardar tu configuración.

---

## ?? Comparación de Opciones de Publicación

| Característica | Framework-Dependent | Self-Contained | Single File |
|----------------|---------------------|----------------|-------------|
| Tamaño | ~10 MB | ~150 MB | ~70 MB |
| Requiere .NET instalado | ? Sí | ? No | ? No |
| Archivos | Múltiples | Múltiples | 1 solo .exe |
| Portabilidad | Baja | Media | Alta |
| Recomendado para | Desarrollo | Distribución | **Distribución** |

---

## ?? Solución de Problemas

### Error: "No se puede ejecutar porque falta .NET"
- Usa la publicación Self-Contained (Opción 2 o 3)

### El instalador no se crea
- Verifica que Inno Setup esté instalado correctamente
- Asegúrate de haber publicado primero con `publish.ps1`

### La aplicación no inicia
- Verifica que Windows Defender no esté bloqueando la aplicación
- Haz clic derecho ? Propiedades ? Desbloquear

---

## ?? Personalización

### Cambiar la versión:
1. Edita `LaunchDock.csproj` ? Cambia `<Version>1.0.0</Version>`
2. Edita `LaunchDock-Setup.iss` ? Cambia `#define MyAppVersion "1.0.0"`

### Cambiar el icono:
1. Reemplaza `LaunchDock.ico` con tu propio icono
2. Republica la aplicación

---

## ? Lista de Verificación Pre-Distribución

Antes de distribuir LaunchDock, verifica:

- [ ] La aplicación se ejecuta correctamente
- [ ] Todas las funcionalidades funcionan
- [ ] El icono se muestra correctamente
- [ ] La configuración se guarda y carga correctamente
- [ ] El auto-ocultar funciona
- [ ] Las posiciones (Top, Bottom, Left, Right, Floating) funcionan
- [ ] El modo de edición funciona
- [ ] Los atajos se abren correctamente
- [ ] La aplicación se cierra correctamente desde la bandeja del sistema

---

## ?? Soporte

Si encuentras algún problema, revisa:
1. Los logs de la aplicación (si los tienes implementados)
2. El Visor de Eventos de Windows
3. Verifica la configuración en `AppData\Roaming\LaunchDock\`

---

¡Buena suerte con tu distribución de LaunchDock! ??

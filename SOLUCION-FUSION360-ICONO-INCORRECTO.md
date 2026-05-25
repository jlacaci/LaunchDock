# ?? Solución: Icono Incorrecto de Fusion 360

## ? Problema

Fusion 360 aparece con el icono de **FusionLauncher.exe** (cohete) en lugar del icono típico de Fusion 360 (naranja con "F").

---

## ?? Causa del Problema

El acceso directo de Fusion 360 en Windows apunta a `FusionLauncher.exe` en lugar de `Fusion360.exe`. Anteriormente, el código extraía el icono del ejecutable resuelto (FusionLauncher.exe), en lugar del icono personalizado definido en el acceso directo (.lnk).

---

## ? Solución Implementada

He modificado el código para que:

1. **Priorice el icono personalizado del .lnk** antes que el del ejecutable resuelto
2. **Soporte archivos .ico** directamente (muchos accesos directos apuntan a archivos .ico separados)
3. **Expanda variables de entorno** como `%ProgramFiles%` en las rutas de iconos
4. **Maneje correctamente índices negativos** en los recursos de iconos

### Archivos Modificados:

- **`Helpers/IconCacheHelper.cs`**:
  - `ExtractCleanIcon()`: Ahora prioriza el icono del .lnk antes que el del ejecutable
  - `ExtractIconFromShortcut()`: Mejorado para soportar .ico y variables de entorno

---

## ?? Cómo Arreglar Fusion 360 (y otras aplicaciones con icono incorrecto)

### Método 1: Eliminar de la Caché (Recomendado)

1. **Abre el explorador de archivos** y navega a:
   ```
   %AppData%\LaunchDock\IconCache\
   ```

2. **Elimina TODOS los archivos PNG** de esta carpeta
   - Esto forzará a LaunchDock a volver a extraer todos los iconos con el nuevo código

3. **Elimina Fusion 360** de LaunchDock:
   - Entra en modo edición (clic derecho en bandeja ? Editar)
   - Elimina el acceso directo de Fusion 360

4. **Vuelve a agregar Fusion 360**:
   - Arrastra el acceso directo de Fusion 360 desde el menú de inicio o escritorio
   - El icono debería aparecer correctamente ahora

### Método 2: Eliminar Solo el Icono de Fusion 360

1. Abre la carpeta de caché de iconos:
   ```
   %AppData%\LaunchDock\IconCache\
   ```

2. Los archivos PNG tienen nombres como: `a3f4b9c2d1e5f6a7b8c9d0e1f2a3b4c5_32.png`
   - No es fácil identificar cuál es de Fusion 360 por el nombre

3. **Opción fácil**: Ordena por fecha y elimina los iconos más recientes

4. **Elimina y vuelve a agregar Fusion 360** en LaunchDock

---

## ?? Cómo Probar que Funciona

1. **Ejecuta la aplicación**:
   ```bash
   dotnet run
   ```

2. **Entra en modo edición** (clic derecho en bandeja)

3. **Arrastra un acceso directo** de Fusion 360 desde:
   - Menú de inicio de Windows
   - Escritorio
   - Carpeta de Autodesk en AppData

4. **Verifica que el icono es correcto**:
   - ? Debería mostrar el icono naranja con "F" de Fusion 360
   - ? NO debería mostrar el cohete de FusionLauncher

---

## ?? Debug: Ver Qué Icono Está Usando

Si quieres ver qué archivo está usando para el icono:

1. Abre el archivo de configuración:
   ```
   %AppData%\LaunchDock\config.json
   ```

2. Busca el acceso directo de Fusion 360

3. Verifica el campo `IconPath`:
   ```json
   {
     "Name": "Fusion 360",
     "TargetPath": "C:\\Users\\...\\FusionLauncher.exe",
     "IconPath": "C:\\Users\\...\\AppData\\Roaming\\LaunchDock\\IconCache\\abc123_32.png"
   }
   ```

4. Abre ese archivo PNG para ver si es el icono correcto

---

## ?? Por Qué Pasaba Esto

### Comportamiento Anterior:
```
.lnk ? Resuelve TargetPath ? FusionLauncher.exe ? Extrae icono del .exe (cohete)
```

### Comportamiento Nuevo:
```
.lnk ? Lee GetIconLocation() ? Fusion360.ico ? Extrae icono del .ico (correcto)
       ? (si no hay icono personalizado)
     Resuelve TargetPath ? Fusion360.exe ? Extrae icono del .exe
```

---

## ?? Notas Importantes

### Variables de Entorno

Muchos accesos directos usan variables como:
- `%ProgramFiles%\Autodesk\Fusion360\Fusion360.ico`
- `%LocalAppData%\Programs\...`

El código ahora **expande estas variables** automáticamente usando:
```csharp
iconLocation = Environment.ExpandEnvironmentVariables(iconLocation);
```

### Archivos .ico vs .exe

- **`.ico`**: Archivo de icono independiente ? Se carga directamente
- **`.exe` / `.dll`**: Contienen iconos incrustados ? Se extraen con `ExtractIconEx`

### Índices de Iconos

Los archivos .exe pueden tener múltiples iconos:
- Índice `0`: Primer icono
- Índice `-1`: ID de recurso específico (negativo)

El código maneja ambos casos correctamente.

---

## ? Mejoras Adicionales

Si después de esto sigues viendo iconos incorrectos, puede ser que:

1. **El acceso directo no tiene icono personalizado definido**
   - Solución: Haz clic derecho en el .lnk ? Propiedades ? Cambiar icono

2. **El archivo .ico no existe o está corrupto**
   - El código fallará silenciosamente y usará el del ejecutable

3. **El ejecutable tiene múltiples iconos y se está usando el incorrecto**
   - Esto es raro, pero puede pasar con algunos instaladores

---

## ?? Prueba con Otras Aplicaciones

Esta mejora también debería funcionar mejor con:
- ? **Adobe Creative Cloud** (iconos personalizados)
- ? **Microsoft Office** (Word, Excel, PowerPoint)
- ? **Steam Games** (iconos de juegos)
- ? **Aplicaciones UWP** (Microsoft Store)

---

¿Todo listo? Prueba eliminando la caché y volviendo a agregar Fusion 360. ¡Deberías ver el icono correcto ahora! ??

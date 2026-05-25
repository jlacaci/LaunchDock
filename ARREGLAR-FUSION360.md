# ?? Instrucciones Rápidas - Arreglar Fusion 360

## ? Solución Rápida (3 pasos)

### 1?? Limpia la caché de iconos
```bash
# Desde la raíz del proyecto, ejecuta:
.\Scripts\ClearIconCache.bat
```

O manualmente:
- Presiona `Win + R`
- Escribe: `%AppData%\LaunchDock\IconCache\`
- Elimina todos los archivos `.png`

### 2?? Ejecuta LaunchDock
```bash
dotnet run
```

### 3?? Elimina y vuelve a agregar Fusion 360
1. Clic derecho en la bandeja del sistema ? **Editar categorías**
2. Elimina el acceso directo de Fusion 360 (botón ?)
3. Arrastra Fusion 360 desde el menú de inicio
4. ¡Listo! Ahora debería tener el icono naranja con "F" ?

---

## ?? ¿Por qué pasaba esto?

El código anterior extraía el icono del **ejecutable resuelto** (FusionLauncher.exe) en lugar del **icono personalizado del acceso directo** (Fusion360.ico).

## ? ¿Qué se arregló?

Ahora el código:
1. **Prioriza el icono personalizado** del .lnk
2. **Soporta archivos .ico** independientes
3. **Expande variables de entorno** como `%ProgramFiles%`
4. **Solo usa el icono del ejecutable** si no hay uno personalizado

---

## ?? Más Información

- **`RESUMEN-CORRECCION-ICONOS.md`** - Explicación técnica completa
- **`SOLUCION-FUSION360-ICONO-INCORRECTO.md`** - Guía detallada del problema
- **`SOLUCION-ICONOS-SIN-FLECHA.md`** - Documentación original del sistema de caché

---

## ?? Si aún no funciona

1. Verifica que el acceso directo de Fusion 360 tiene un icono personalizado:
   - Clic derecho en el .lnk ? Propiedades ? Ver icono

2. Verifica la configuración en `%AppData%\LaunchDock\config.json`:
   - Busca `"Name": "Fusion 360"`
   - Verifica que `IconPath` apunte a un archivo PNG válido

3. Revisa los documentos de troubleshooting en la raíz del proyecto

---

¡Eso es todo! ??

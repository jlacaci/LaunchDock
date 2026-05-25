# ? INSTRUCCIONES RÁPIDAS - Arreglar Copilot

## ?? Problema
Copilot aparece con flecha de acceso directo superpuesta.

## ? Solución (3 pasos)

### 1?? Limpia la caché
```bash
.\Scripts\ClearIconCache.bat
```

### 2?? Ejecuta LaunchDock
```bash
dotnet run
```

### 3?? Elimina y vuelve a agregar Copilot
1. Entra en modo edición (clic derecho ? Editar)
2. Elimina Copilot (botón ?)
3. Arrastra Copilot desde el menú de inicio
4. ? ¡Sin flecha!

---

## ?? Qué se Arregló

- ? **Fusion 360**: Icono personalizado del .lnk (icono naranja con "F")
- ? **Copilot y apps UWP**: API `IShellItemImageFactory` sin overlays
- ? **Todas las apps**: Mejor extracción de iconos sin flecha

---

## ?? Más Info

- **`SOLUCION-COPILOT-FLECHA.md`** - Explicación técnica completa
- **`ARREGLAR-FUSION360.md`** - Solución para Fusion 360
- **`RESUMEN-CORRECCION-ICONOS.md`** - Todos los cambios

---

**¿Funciona?** ¡Perfecto! ??  
**¿Sigue con problemas?** Revisa `SOLUCION-COPILOT-FLECHA.md`

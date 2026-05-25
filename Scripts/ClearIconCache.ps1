# Script para limpiar la caché de iconos de LaunchDock
# Ejecuta este script si ves iconos incorrectos después de actualizar

$cachePath = "$env:APPDATA\LaunchDock\IconCache"

Write-Host "================================================" -ForegroundColor Cyan
Write-Host "  Limpiador de Caché de Iconos - LaunchDock" -ForegroundColor Cyan
Write-Host "================================================" -ForegroundColor Cyan
Write-Host ""

if (Test-Path $cachePath) {
    Write-Host "?? Ubicación de caché: $cachePath" -ForegroundColor Yellow

    $files = Get-ChildItem -Path $cachePath -Filter "*.png"
    $count = $files.Count

    if ($count -eq 0) {
        Write-Host "? La caché ya está vacía. No hay nada que eliminar." -ForegroundColor Green
    } else {
        Write-Host "?? Se encontraron $count iconos en caché" -ForegroundColor Yellow
        Write-Host ""

        $confirm = Read-Host "¿Deseas eliminar todos los iconos en caché? (S/N)"

        if ($confirm -eq "S" -or $confirm -eq "s") {
            Write-Host ""
            Write-Host "???  Eliminando iconos..." -ForegroundColor Yellow

            $deleted = 0
            $failed = 0

            foreach ($file in $files) {
                try {
                    Remove-Item $file.FullName -Force
                    $deleted++
                } catch {
                    $failed++
                    Write-Host "  ??  No se pudo eliminar: $($file.Name)" -ForegroundColor Red
                }
            }

            Write-Host ""
            Write-Host "? Completado:" -ForegroundColor Green
            Write-Host "  • Eliminados: $deleted archivos" -ForegroundColor Green

            if ($failed -gt 0) {
                Write-Host "  • Fallidos: $failed archivos (pueden estar en uso)" -ForegroundColor Yellow
                Write-Host ""
                Write-Host "?? Cierra LaunchDock e intenta de nuevo si algunos archivos no se pudieron eliminar." -ForegroundColor Cyan
            }

            Write-Host ""
            Write-Host "?? Próximos pasos:" -ForegroundColor Cyan
            Write-Host "  1. Inicia LaunchDock" -ForegroundColor White
            Write-Host "  2. Los iconos se regenerarán automáticamente" -ForegroundColor White
            Write-Host "  3. Si un icono sigue siendo incorrecto:" -ForegroundColor White
            Write-Host "     - Elimina el acceso directo de LaunchDock" -ForegroundColor White
            Write-Host "     - Vuelve a agregarlo arrastrándolo" -ForegroundColor White

        } else {
            Write-Host ""
            Write-Host "? Operación cancelada." -ForegroundColor Red
        }
    }
} else {
    Write-Host "??  La carpeta de caché no existe todavía." -ForegroundColor Cyan
    Write-Host "   Se creará automáticamente cuando agregues accesos directos." -ForegroundColor Gray
}

Write-Host ""
Write-Host "================================================" -ForegroundColor Cyan
Write-Host "Presiona cualquier tecla para salir..."
$null = $Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")

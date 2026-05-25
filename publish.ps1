# Script de publicación de LaunchDock
# Este script crea diferentes versiones de la aplicación para distribución

Write-Host "=====================================" -ForegroundColor Cyan
Write-Host "  LaunchDock - Script de Publicación" -ForegroundColor Cyan
Write-Host "=====================================" -ForegroundColor Cyan
Write-Host ""

# Limpiar publicaciones anteriores
Write-Host "Limpiando publicaciones anteriores..." -ForegroundColor Yellow
if (Test-Path ".\publish") {
    Remove-Item ".\publish" -Recurse -Force
}

# Crear carpeta de publicación
New-Item -ItemType Directory -Path ".\publish" -Force | Out-Null

Write-Host ""
Write-Host "Selecciona el tipo de publicación:" -ForegroundColor Green
Write-Host "1. Framework-dependent (Requiere .NET 8.0 instalado - Tamaño pequeño ~10MB)"
Write-Host "2. Self-contained (Incluye .NET - Tamaño grande ~150MB pero no requiere instalación de .NET)"
Write-Host "3. Self-contained con archivo único (Todo en un .exe - Recomendado para distribución)"
Write-Host ""

$choice = Read-Host "Ingresa tu opción (1, 2 o 3)"

switch ($choice) {
    "1" {
        Write-Host ""
        Write-Host "Publicando versión Framework-dependent..." -ForegroundColor Yellow
        dotnet publish -c Release -r win-x64 --self-contained false -o .\publish\LaunchDock-FrameworkDependent

        Write-Host ""
        Write-Host "? Publicación completada!" -ForegroundColor Green
        Write-Host "Ubicación: .\publish\LaunchDock-FrameworkDependent\" -ForegroundColor Cyan
        Write-Host "NOTA: Esta versión requiere .NET 8.0 Desktop Runtime instalado" -ForegroundColor Yellow
    }
    "2" {
        Write-Host ""
        Write-Host "Publicando versión Self-contained..." -ForegroundColor Yellow
        dotnet publish -c Release -r win-x64 --self-contained true -o .\publish\LaunchDock-SelfContained

        Write-Host ""
        Write-Host "? Publicación completada!" -ForegroundColor Green
        Write-Host "Ubicación: .\publish\LaunchDock-SelfContained\" -ForegroundColor Cyan
        Write-Host "Esta versión incluye todos los archivos necesarios" -ForegroundColor Yellow
    }
    "3" {
        Write-Host ""
        Write-Host "Publicando versión Self-contained con archivo único..." -ForegroundColor Yellow
        dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -o .\publish\LaunchDock-SingleFile

        Write-Host ""
        Write-Host "? Publicación completada!" -ForegroundColor Green
        Write-Host "Ubicación: .\publish\LaunchDock-SingleFile\" -ForegroundColor Cyan
        Write-Host "Esta versión es un solo archivo .exe portable" -ForegroundColor Yellow
    }
    default {
        Write-Host "Opción inválida. Saliendo..." -ForegroundColor Red
        exit
    }
}

Write-Host ""
Write-Host "=====================================" -ForegroundColor Cyan
Write-Host "¿Deseas crear un instalador con Inno Setup?" -ForegroundColor Green
Write-Host "NOTA: Necesitas tener Inno Setup instalado" -ForegroundColor Yellow
Write-Host "(Descárgalo de: https://jrsoftware.org/isdl.php)" -ForegroundColor Yellow
Write-Host "=====================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "Presiona cualquier tecla para abrir la carpeta de publicación..." -ForegroundColor Cyan
$null = $Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")

# Abrir carpeta de publicación
Start-Process explorer.exe -ArgumentList ".\publish\"

Write-Host ""
Write-Host "? ¡Listo! Puedes probar la aplicación desde la carpeta publish" -ForegroundColor Green

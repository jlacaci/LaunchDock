# ============================================================
#  LaunchDock - Script de Publicación
#  Genera el .exe y (opcionalmente) el instalador con Inno Setup
# ============================================================

param(
    [string]$Version       = "",
    [string]$InnoSetupPath = "C:\Program Files (x86)\Inno Setup 6\ISCC.exe"
)

# Si no se pasa versión, leerla automáticamente del .csproj
if ([string]::IsNullOrEmpty($Version)) {
    $csproj = Join-Path (Split-Path $PSScriptRoot -Parent) "LaunchDock.csproj"
    $xml = [xml](Get-Content $csproj)
    $Version = $xml.Project.PropertyGroup.Version | Where-Object { $_ } | Select-Object -First 1
    if ([string]::IsNullOrEmpty($Version)) { $Version = "1.0.0" }
}

$ErrorActionPreference = "Stop"
$Root   = Split-Path $PSScriptRoot -Parent
$Output = Join-Path $Root "publish\win-x64"

Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  LaunchDock - Publicación v$Version"    -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# ?? 1. Cerrar LaunchDock si está en ejecución ??????????????
Write-Host "  Cerrando LaunchDock si está en ejecución..." -ForegroundColor Yellow
Stop-Process -Name "LaunchDock" -Force -ErrorAction SilentlyContinue
Start-Sleep -Milliseconds 600

# ?? 2. Limpiar carpeta de salida ???????????????????????????
if (Test-Path $Output) {
    Write-Host "??  Limpiando carpeta anterior..." -ForegroundColor Yellow
    Remove-Item $Output -Recurse -Force
}

# ?? 3. Publicar como único .exe (Self-Contained) ??????????
Write-Host "?? Compilando y publicando..." -ForegroundColor Yellow
Set-Location $Root

dotnet clean "$Root\LaunchDock.csproj" --configuration Release --runtime win-x64 | Out-Null

dotnet publish "$Root\LaunchDock.csproj" `
    --configuration Release `
    --runtime win-x64 `
    --self-contained true `
    -p:PublishSingleFile=true `
    -p:EnableCompressionInSingleFile=true `
    -p:IncludeNativeLibrariesForSelfExtract=true `
    -p:PublishTrimmed=false `
    -p:DebugType=none `
    -p:DebugSymbols=false `
    --output $Output

if ($LASTEXITCODE -ne 0) {
    Write-Host ""
    Write-Host "? Error en la compilación. Revisa los errores arriba." -ForegroundColor Red
    exit 1
}

# ?? 4. Verificar que el .exe existe ???????????????????????
$exe = Join-Path $Output "LaunchDock.exe"
if (-not (Test-Path $exe)) {
    Write-Host "? No se encontró LaunchDock.exe en $Output" -ForegroundColor Red
    exit 1
}

$sizeMB = [math]::Round((Get-Item $exe).Length / 1MB, 1)
Write-Host ""
Write-Host "? Ejecutable generado:" -ForegroundColor Green
Write-Host "   $exe ($sizeMB MB)" -ForegroundColor White

# ?? 5. Actualizar versión en el .iss ?????????????????????
$issFile = Join-Path $Root "LaunchDock-Setup.iss"
if (Test-Path $issFile) {
    (Get-Content $issFile) `
        -replace '#define MyAppVersion\s+"[^"]+"', "#define MyAppVersion   `"$Version`"" |
        Set-Content $issFile
    Write-Host "?? Versión actualizada en LaunchDock-Setup.iss" -ForegroundColor Cyan
}

# ?? 6. Crear instalador con Inno Setup (opcional) ?????????
Write-Host ""
if (Test-Path $InnoSetupPath) {
    Write-Host "?? Creando instalador con Inno Setup..." -ForegroundColor Yellow

    $installerOut = Join-Path $Root "publish\Installer"
    New-Item -ItemType Directory -Force -Path $installerOut | Out-Null

    # Limpiar instaladores antiguos para que no queden varias versiones
    Get-ChildItem $installerOut -Filter "LaunchDock-Setup-*.exe" | Remove-Item -Force

    & $InnoSetupPath $issFile
    if ($LASTEXITCODE -eq 0) {
        $installer = Get-ChildItem $installerOut -Filter "*.exe" | Sort-Object LastWriteTime -Descending | Select-Object -First 1
        if ($installer) {
            $iSizeMB = [math]::Round($installer.Length / 1MB, 1)
            Write-Host ""
            Write-Host "? Instalador creado:" -ForegroundColor Green
            Write-Host "   $($installer.FullName) ($iSizeMB MB)" -ForegroundColor White
        }
    } else {
        Write-Host "??  Inno Setup terminó con errores." -ForegroundColor Yellow
    }
} else {
    Write-Host "??  Inno Setup no encontrado en:" -ForegroundColor Cyan
    Write-Host "   $InnoSetupPath" -ForegroundColor Gray
    Write-Host "   ? Solo se generó el .exe. Para crear el instalador instala" -ForegroundColor Gray
    Write-Host "     Inno Setup 6 desde https://jrsoftware.org/isinfo.php" -ForegroundColor Gray
    Write-Host "     y vuelve a ejecutar este script." -ForegroundColor Gray
}

# ?? 7. Resumen ????????????????????????????????????????????
Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  Listo " -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "?? Carpeta de salida:" -ForegroundColor White
Write-Host "   $(Join-Path $Root 'publish')" -ForegroundColor Gray
Write-Host ""
Write-Host "Presiona cualquier tecla para salir..."
$null = $Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")

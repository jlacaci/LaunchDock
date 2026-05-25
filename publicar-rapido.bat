@echo off
chcp 65001 >nul
echo =====================================
echo   LaunchDock - Publicación Rápida
echo   (Versión Optimizada)
echo =====================================
echo.

echo Limpiando publicaciones anteriores...
if exist "publish" rmdir /s /q "publish"
mkdir "publish"

echo.
echo Publicando LaunchDock (Archivo único optimizado)...
echo Por favor espera, esto puede tomar unos minutos...
echo.
echo Optimizaciones aplicadas:
echo - Trimming: Elimina código no usado
echo - Compresión: Reduce el tamaño del ejecutable
echo - Sin símbolos de depuración
echo.

dotnet publish -c Release --self-contained true -o ".\publish\LaunchDock-SingleFile"

if %ERRORLEVEL% EQU 0 (
    echo.
    echo =====================================
    echo ? Publicación completada con éxito!
    echo =====================================
    echo.
    echo Tu aplicación está en:
    echo .\publish\LaunchDock-SingleFile\LaunchDock.exe
    echo.
    echo Abriendo carpeta...
    start explorer.exe ".\publish\LaunchDock-SingleFile"
) else (
    echo.
    echo ? Error durante la publicación
    echo Verifica que tengas el SDK de .NET 8.0 instalado
    echo.
)

echo.
echo Presiona cualquier tecla para salir...
pause >nul

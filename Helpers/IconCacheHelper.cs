using System.Drawing;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace LaunchDock.Helpers;

public static class IconCacheHelper
{
    private static readonly string CacheFolder = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "LaunchDock",
        "IconCache"
    );

    static IconCacheHelper()
    {
        // Crear carpeta de caché si no existe
        if (!Directory.Exists(CacheFolder))
        {
            Directory.CreateDirectory(CacheFolder);
        }
    }

    /// <summary>
    /// Obtiene un icono y lo guarda como PNG sin flecha de acceso directo
    /// </summary>
    public static string? SaveIconAsPng(string sourcePath, int size = 32)
    {
        try
        {
            // Generar nombre único para el icono basado en el path
            string iconHash = GetHashForPath(sourcePath);
            string pngFileName = $"{iconHash}_{size}.png";
            string pngPath = Path.Combine(CacheFolder, pngFileName);

            // Si ya existe en caché, devolverlo
            if (File.Exists(pngPath))
            {
                return pngPath;
            }

            // Para .lnk (incluyendo apps UWP como Copilot) intentar primero
            // IShellItemImageFactory que devuelve el bitmap con alpha real y SIN flecha
            if (Path.GetExtension(sourcePath).ToLower() == ".lnk")
            {
                Bitmap? uwpBmp = ExtractBitmapUwp(sourcePath, size);
                if (uwpBmp != null)
                {
                    using (uwpBmp)
                    {
                        uwpBmp.Save(pngPath, ImageFormat.Png);
                        return pngPath;
                    }
                }
            }

            // Ruta clásica: extraer Icon y convertir a PNG
            Icon? icon = ExtractCleanIcon(sourcePath, size);
            if (icon == null) return null;

            using (var bitmap = icon.ToBitmap())
            using (var pngBitmap = new Bitmap(size, size, System.Drawing.Imaging.PixelFormat.Format32bppArgb))
            {
                using (var g = Graphics.FromImage(pngBitmap))
                {
                    g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                    g.DrawImage(bitmap, 0, 0, size, size);
                }
                pngBitmap.Save(pngPath, ImageFormat.Png);
            }

            icon.Dispose();
            return pngPath;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error guardando icono: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Carga un icono PNG desde el caché
    /// </summary>
    public static ImageSource? LoadIconFromCache(string pngPath)
    {
        try
        {
            if (!File.Exists(pngPath))
            {
                return null;
            }

            var bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.UriSource = new Uri(pngPath, UriKind.Absolute);
            bitmap.CacheOption = BitmapCacheOption.OnLoad;
            bitmap.EndInit();
            bitmap.Freeze();

            return bitmap;
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Extrae un icono limpio sin flecha de acceso directo
    /// </summary>
    private static Icon? ExtractCleanIcon(string path, int size)
    {
        // Si es un .lnk, PRIMERO intentar obtener el icono personalizado del .lnk
        if (Path.GetExtension(path).ToLower() == ".lnk")
        {
            // Intentar extraer el icono personalizado del .lnk usando IShellLink
            var iconFromLnk = ExtractIconFromShortcut(path, size);
            if (iconFromLnk != null) return iconFromLnk;

            // Fallback: resolver el .lnk y usar el icono del ejecutable
            string? resolved = IconHelper.ResolveShortcut(path);
            if (!string.IsNullOrEmpty(resolved) && resolved != path && File.Exists(resolved))
            {
                path = resolved;
            }
        }

        // Extraer icono del archivo ejecutable
        return ExtractIconFromFile(path, size);
    }

    /// <summary>
    /// Extrae el bitmap con alpha correcto usando IShellItemImageFactory.
    /// Funciona con apps UWP/Store (Copilot, Calculator, etc.).
    /// SIIGBF_ICONONLY = 0x4 ? sin flecha superpuesta.
    /// Usa GetDIBits para leer el canal alpha premultiplicado (PARGB) correctamente.
    /// </summary>
    private static Bitmap? ExtractBitmapUwp(string lnkPath, int size)
    {
        try
        {
            Guid shellItem2Guid = new Guid("7E9FB0D3-919F-4307-AB2E-9B1860310C93");
            int hr = NativeMethods.SHCreateItemFromParsingName(lnkPath, IntPtr.Zero, ref shellItem2Guid, out IntPtr shellItemPtr);
            if (hr != 0 || shellItemPtr == IntPtr.Zero) return null;

            var imageFactory = Marshal.GetObjectForIUnknown(shellItemPtr) as NativeMethods.IShellItemImageFactory;
            Marshal.Release(shellItemPtr);
            if (imageFactory == null) return null;

            var iconSize = new NativeMethods.SIZE { cx = size, cy = size };
            // SIIGBF_ICONONLY = 0x4 ? solo icono, sin overlays (sin flecha)
            hr = imageFactory.GetImage(iconSize, 0x4, out IntPtr hBitmap);
            Marshal.ReleaseComObject(imageFactory);
            if (hr != 0 || hBitmap == IntPtr.Zero) return null;

            try
            {
                return HBitmapToArgb(hBitmap, size);
            }
            finally
            {
                NativeMethods.DeleteObject(hBitmap);
            }
        }
        catch { }
        return null;
    }

    /// <summary>
    /// Convierte un HBITMAP con alpha premultiplicado (PARGB) a un Bitmap ARGB con
    /// transparencia correcta, usando GetDIBits para leer los bytes raw.
    /// Image.FromHbitmap NO sirve porque ignora el canal alpha.
    /// </summary>
    private static unsafe Bitmap? HBitmapToArgb(IntPtr hBitmap, int size)
    {
        IntPtr hdc = NativeMethods.CreateCompatibleDC(IntPtr.Zero);
        if (hdc == IntPtr.Zero) return null;
        try
        {
            var bmi = new NativeMethods.BITMAPINFOHEADER
            {
                biSize        = (uint)Marshal.SizeOf<NativeMethods.BITMAPINFOHEADER>(),
                biWidth       = size,
                biHeight      = -size, // negativo = top-down
                biPlanes      = 1,
                biBitCount    = 32,
                biCompression = 0      // BI_RGB
            };

            int[] rawPixels = new int[size * size];
            fixed (int* pPixels = rawPixels)
            {
                // Lee los bits del HBITMAP incluyendo el canal alpha
                NativeMethods.GetDIBits(hdc, hBitmap, 0, (uint)size,
                    (IntPtr)pPixels, ref bmi, 0);
            }

            var result = new Bitmap(size, size, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            var bmpData = result.LockBits(
                new Rectangle(0, 0, size, size),
                System.Drawing.Imaging.ImageLockMode.WriteOnly,
                System.Drawing.Imaging.PixelFormat.Format32bppArgb);

            byte* dst = (byte*)bmpData.Scan0;
            for (int i = 0; i < rawPixels.Length; i++)
            {
                int pixel = rawPixels[i];
                byte b = (byte)( pixel        & 0xFF);
                byte g = (byte)((pixel >>  8) & 0xFF);
                byte r = (byte)((pixel >> 16) & 0xFF);
                byte a = (byte)((pixel >> 24) & 0xFF);

                // Des-premultiplicar alpha: canal * 255 / alpha
                if (a > 0 && a < 255)
                {
                    b = (byte)Math.Min(255, b * 255 / a);
                    g = (byte)Math.Min(255, g * 255 / a);
                    r = (byte)Math.Min(255, r * 255 / a);
                }

                int offset = i * 4;
                dst[offset]     = b;
                dst[offset + 1] = g;
                dst[offset + 2] = r;
                dst[offset + 3] = a;
            }

            result.UnlockBits(bmpData);
            return result;
        }
        finally
        {
            NativeMethods.DeleteDC(hdc);
        }
    }

    private static Icon? ExtractIconFromFile(string filePath, int size)
    {
        try
        {
            IntPtr[] largeIcons = new IntPtr[1];
            IntPtr[] smallIcons = new IntPtr[1];

            int count = LaunchDock.Helpers.NativeMethods.ExtractIconEx(filePath, 0, largeIcons, smallIcons, 1);

            if (count > 0)
            {
                IntPtr hIcon = size >= 32 ? largeIcons[0] : smallIcons[0];

                if (hIcon != IntPtr.Zero)
                {
                    Icon icon = (Icon)Icon.FromHandle(hIcon).Clone();

                    // Limpiar recursos
                    if (largeIcons[0] != IntPtr.Zero) LaunchDock.Helpers.NativeMethods.DestroyIcon(largeIcons[0]);
                    if (smallIcons[0] != IntPtr.Zero) LaunchDock.Helpers.NativeMethods.DestroyIcon(smallIcons[0]);

                    return icon;
                }
            }
        }
        catch { }

        // Método alternativo
        try
        {
            return Icon.ExtractAssociatedIcon(filePath);
        }
        catch { }

        return null;
    }

    private static Icon? ExtractIconFromShortcut(string lnkPath, int size)
    {
        try
        {
            var link = (IShellLink)new ShellLink();
            ((IPersistFile)link).Load(lnkPath, 0);

            var sb = new StringBuilder(260);
            int iconIndex = 0;

            // Obtener ubicación del icono personalizado
            link.GetIconLocation(sb, sb.Capacity, out iconIndex);
            string? iconLocation = sb.ToString();

            // Si hay icono personalizado, usarlo
            if (!string.IsNullOrEmpty(iconLocation))
            {
                // Expandir variables de entorno
                iconLocation = Environment.ExpandEnvironmentVariables(iconLocation);

                Marshal.ReleaseComObject(link);

                // Si es un archivo .ico, cargarlo directamente
                if (iconLocation.EndsWith(".ico", StringComparison.OrdinalIgnoreCase) && File.Exists(iconLocation))
                {
                    try
                    {
                        using (var fs = new FileStream(iconLocation, FileMode.Open, FileAccess.Read))
                        {
                            return new Icon(fs, size, size);
                        }
                    }
                    catch 
                    { 
                        // Si falla, intentar cargar sin especificar tamaño
                        try
                        {
                            return new Icon(iconLocation);
                        }
                        catch { }
                    }
                }

                // Si es un .exe, .dll u otro archivo con recursos, extraer con ExtractIconEx
                if (File.Exists(iconLocation))
                {
                    IntPtr[] large = new IntPtr[1];
                    IntPtr[] small = new IntPtr[1];

                    // Para índices negativos, ExtractIconEx usa el valor absoluto
                    int extractIndex = iconIndex < 0 ? -iconIndex : iconIndex;
                    int count = LaunchDock.Helpers.NativeMethods.ExtractIconEx(iconLocation, extractIndex, large, small, 1);

                    if (count > 0)
                    {
                        IntPtr hIcon = size >= 32 ? large[0] : small[0];

                        if (hIcon != IntPtr.Zero)
                        {
                            Icon icon = (Icon)Icon.FromHandle(hIcon).Clone();

                            if (large[0] != IntPtr.Zero) LaunchDock.Helpers.NativeMethods.DestroyIcon(large[0]);
                            if (small[0] != IntPtr.Zero) LaunchDock.Helpers.NativeMethods.DestroyIcon(small[0]);

                            return icon;
                        }
                    }
                }
            }
            else
            {
                // No hay icono personalizado, obtener del target
                sb.Clear();
                link.GetPath(sb, sb.Capacity, IntPtr.Zero, 0);
                iconLocation = sb.ToString();

                Marshal.ReleaseComObject(link);

                if (!string.IsNullOrEmpty(iconLocation) && File.Exists(iconLocation))
                {
                    IntPtr[] large = new IntPtr[1];
                    IntPtr[] small = new IntPtr[1];

                    int count = LaunchDock.Helpers.NativeMethods.ExtractIconEx(iconLocation, 0, large, small, 1);

                    if (count > 0)
                    {
                        IntPtr hIcon = size >= 32 ? large[0] : small[0];

                        if (hIcon != IntPtr.Zero)
                        {
                            Icon icon = (Icon)Icon.FromHandle(hIcon).Clone();

                            if (large[0] != IntPtr.Zero) LaunchDock.Helpers.NativeMethods.DestroyIcon(large[0]);
                            if (small[0] != IntPtr.Zero) LaunchDock.Helpers.NativeMethods.DestroyIcon(small[0]);

                            return icon;
                        }
                    }
                }
            }
        }
        catch 
        { 
            // Ignorar errores y devolver null
        }

        return null;
    }

    /// <summary>
    /// Genera un hash único para un path
    /// </summary>
    private static string GetHashForPath(string path)
    {
        using (var md5 = MD5.Create())
        {
            byte[] hash = md5.ComputeHash(Encoding.UTF8.GetBytes(path.ToLowerInvariant()));
            return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
        }
    }

    /// <summary>
    /// Limpia iconos antiguos no usados del caché
    /// </summary>
    public static void CleanupOldIcons(int daysOld = 30)
    {
        try
        {
            var cutoffDate = DateTime.Now.AddDays(-daysOld);
            var files = Directory.GetFiles(CacheFolder, "*.png");

            foreach (var file in files)
            {
                var fileInfo = new FileInfo(file);
                if (fileInfo.LastAccessTime < cutoffDate)
                {
                    File.Delete(file);
                }
            }
        }
        catch { }
    }

    /// <summary>
    /// Limpia COMPLETAMENTE la caché de iconos (todos los archivos PNG)
    /// Útil cuando se han corregido bugs y se necesita regenerar todos los iconos
    /// </summary>
    public static void ClearAllCache()
    {
        try
        {
            var files = Directory.GetFiles(CacheFolder, "*.png");
            foreach (var file in files)
            {
                try
                {
                    File.Delete(file);
                }
                catch
                {
                    // Ignorar archivos que no se puedan eliminar (en uso, etc.)
                }
            }
        }
        catch { }
    }

    /// <summary>
    /// Obtiene la ruta de la carpeta de caché de iconos
    /// </summary>
    public static string GetCacheFolder() => CacheFolder;

    /// <summary>
    /// Devuelve el número de iconos actualmente en caché
    /// </summary>
    public static int CountCachedIcons()
    {
        try { return Directory.GetFiles(CacheFolder, "*.png").Length; }
        catch { return 0; }
    }

    // COM Interfaces
    [ComImport]
    [Guid("00021401-0000-0000-C000-000000000046")]
    private class ShellLink { }

    [ComImport]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    [Guid("000214F9-0000-0000-C000-000000000046")]
    private interface IShellLink
    {
        void GetPath([Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszFile, int cchMaxPath, IntPtr pfd, uint fFlags);
        void GetIDList(out IntPtr ppidl);
        void SetIDList(IntPtr pidl);
        void GetDescription([Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszName, int cchMaxName);
        void SetDescription([MarshalAs(UnmanagedType.LPWStr)] string pszName);
        void GetWorkingDirectory([Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszDir, int cchMaxPath);
        void SetWorkingDirectory([MarshalAs(UnmanagedType.LPWStr)] string pszDir);
        void GetArguments([Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszArgs, int cchMaxPath);
        void SetArguments([MarshalAs(UnmanagedType.LPWStr)] string pszArgs);
        void GetHotkey(out short pwHotkey);
        void SetHotkey(short wHotkey);
        void GetShowCmd(out int piShowCmd);
        void SetShowCmd(int iShowCmd);
        void GetIconLocation([Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszIconPath, int cchIconPath, out int piIcon);
        void SetIconLocation([MarshalAs(UnmanagedType.LPWStr)] string pszIconPath, int iIcon);
        void SetRelativePath([MarshalAs(UnmanagedType.LPWStr)] string pszPathRel, uint dwReserved);
        void Resolve(IntPtr hwnd, uint fFlags);
        void SetPath([MarshalAs(UnmanagedType.LPWStr)] string pszFile);
    }

    [ComImport]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    [Guid("0000010b-0000-0000-C000-000000000046")]
    private interface IPersistFile
    {
        void GetClassID(out Guid pClassID);
        void IsDirty();
        void Load([MarshalAs(UnmanagedType.LPWStr)] string pszFileName, uint dwMode);
        void Save([MarshalAs(UnmanagedType.LPWStr)] string pszFileName, [MarshalAs(UnmanagedType.Bool)] bool fRemember);
        void SaveCompleted([MarshalAs(UnmanagedType.LPWStr)] string pszFileName);
        void GetCurFile([MarshalAs(UnmanagedType.LPWStr)] out string ppszFileName);
    }
}

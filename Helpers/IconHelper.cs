using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace LaunchDock.Helpers;

public static class IconHelper
{
    public static ImageSource? GetIconForPath(string path, int size = 32)
    {
        try
        {
            Icon? icon = null;

            if (Directory.Exists(path))
            {
                // Folder icon
                icon = GetFolderIcon(size);
            }
            else if (File.Exists(path))
            {
                string ext = Path.GetExtension(path).ToLower();

                // Para archivos PNG en caché, cargar directamente
                if (ext == ".png" && path.Contains("IconCache"))
                {
                    return IconCacheHelper.LoadIconFromCache(path);
                }

                if (ext == ".lnk")
                {
                    // Para .lnk, resolver primero y usar el icono del ejecutable
                    string? resolved = ResolveShortcut(path);

                    if (!string.IsNullOrEmpty(resolved) && resolved != path && File.Exists(resolved))
                    {
                        // Es un acceso directo tradicional, usar icono del .exe
                        icon = ExtractIconClean(resolved, size);
                    }

                    // Si no se pudo obtener, intentar del .lnk con método especial
                    if (icon == null)
                    {
                        icon = GetCleanIconFromShortcut(path, size);
                    }
                }
                else if (ext == ".exe" || ext == ".dll")
                {
                    // Archivo ejecutable, extraer icono sin flecha
                    icon = ExtractIconClean(path, size);
                }
                else
                {
                    // Otros archivos
                    icon = Icon.ExtractAssociatedIcon(path);
                }
            }
            else
            {
                // Si no es un archivo físico, puede ser un protocolo UWP
                icon = SystemIcons.Application;
            }

            if (icon == null) return null;

            var bitmap = icon.ToBitmap();
            var hBitmap = bitmap.GetHbitmap();
            try
            {
                return Imaging.CreateBitmapSourceFromHBitmap(
                    hBitmap, IntPtr.Zero, Int32Rect.Empty,
                    BitmapSizeOptions.FromEmptyOptions());
            }
            finally
            {
                DeleteObject(hBitmap);
                bitmap.Dispose();
                icon.Dispose();
            }
        }
        catch
        {
            return null;
        }
    }

    private static Icon? ExtractIconClean(string filePath, int size)
    {
        try
        {
            IntPtr[] largeIcons = new IntPtr[1];
            IntPtr[] smallIcons = new IntPtr[1];

            int count = NativeMethods.ExtractIconEx(filePath, 0, largeIcons, smallIcons, 1);

            if (count > 0)
            {
                IntPtr hIcon = size >= 32 ? largeIcons[0] : smallIcons[0];

                if (hIcon != IntPtr.Zero)
                {
                    Icon icon = (Icon)Icon.FromHandle(hIcon).Clone();

                    // Limpiar recursos
                    if (largeIcons[0] != IntPtr.Zero) NativeMethods.DestroyIcon(largeIcons[0]);
                    if (smallIcons[0] != IntPtr.Zero) NativeMethods.DestroyIcon(smallIcons[0]);

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

    private static Icon? GetCleanIconFromShortcut(string lnkPath, int size)
    {
        // Método 1: Intentar con IShellLink para obtener el icono del ejecutable directo
        try
        {
            var link = (IShellLink)new ShellLink();
            ((IPersistFile)link).Load(lnkPath, 0);

            var sb = new StringBuilder(260);
            int iconIndex = 0;

            // Primero intentar obtener la ubicación del icono personalizado
            link.GetIconLocation(sb, sb.Capacity, out iconIndex);
            string? iconLocation = sb.ToString();

            // Si no hay icono personalizado, obtener del target
            if (string.IsNullOrEmpty(iconLocation))
            {
                sb.Clear();
                link.GetPath(sb, sb.Capacity, IntPtr.Zero, 0);
                iconLocation = sb.ToString();
                iconIndex = 0;
            }

            Marshal.ReleaseComObject(link);

            // Extraer icono usando ExtractIconEx (sin overlay de flecha)
            if (!string.IsNullOrEmpty(iconLocation) && File.Exists(iconLocation))
            {
                IntPtr[] large = new IntPtr[1];
                IntPtr[] small = new IntPtr[1];

                int count = NativeMethods.ExtractIconEx(iconLocation, iconIndex, large, small, 1);

                if (count > 0)
                {
                    IntPtr hIcon = size >= 32 ? large[0] : small[0];

                    if (hIcon != IntPtr.Zero)
                    {
                        Icon icon = (Icon)Icon.FromHandle(hIcon).Clone();

                        if (large[0] != IntPtr.Zero) NativeMethods.DestroyIcon(large[0]);
                        if (small[0] != IntPtr.Zero) NativeMethods.DestroyIcon(small[0]);

                        return icon;
                    }
                }
            }
        }
        catch { }

        // Método 2: Para apps UWP y otros casos especiales
        // Usar IShellItemImageFactory para obtener el icono sin overlay
        try
        {
            Guid shellItem2Guid = new Guid("7E9FB0D3-919F-4307-AB2E-9B1860310C93"); // IShellItem2
            Guid imageFactoryGuid = new Guid("bcc18b79-ba16-442f-80c4-8a59c30c463b"); // IShellItemImageFactory

            int hr = NativeMethods.SHCreateItemFromParsingName(lnkPath, IntPtr.Zero, ref shellItem2Guid, out IntPtr shellItemPtr);

            if (hr == 0 && shellItemPtr != IntPtr.Zero)
            {
                var imageFactory = Marshal.GetObjectForIUnknown(shellItemPtr) as NativeMethods.IShellItemImageFactory;

                if (imageFactory != null)
                {
                    NativeMethods.SIZE iconSize = new NativeMethods.SIZE { cx = size, cy = size };
                    IntPtr hBitmap;

                    // SIIGBF_ICONONLY = 0x4 - Solo el icono, sin overlays
                    hr = imageFactory.GetImage(iconSize, 0x4, out hBitmap);

                    if (hr == 0 && hBitmap != IntPtr.Zero)
                    {
                        using (var bitmap = System.Drawing.Image.FromHbitmap(hBitmap))
                        {
                            Icon icon = Icon.FromHandle(((Bitmap)bitmap).GetHicon());
                            NativeMethods.DeleteObject(hBitmap);
                            Marshal.ReleaseComObject(imageFactory);
                            return icon;
                        }
                    }

                    Marshal.ReleaseComObject(imageFactory);
                }
            }
        }
        catch { }

        // Último recurso: icono genérico
        return SystemIcons.Application;
    }

    private static Icon GetFolderIcon(int size)
    {
        var flags = NativeMethods.SHGFI_ICON | NativeMethods.SHGFI_USEFILEATTRIBUTES;
        flags |= size >= 32 ? NativeMethods.SHGFI_LARGEICON : NativeMethods.SHGFI_SMALLICON;

        var shfi = new NativeMethods.SHFILEINFO();
        NativeMethods.SHGetFileInfo("C:\\", NativeMethods.FILE_ATTRIBUTE_DIRECTORY,
            ref shfi, (uint)Marshal.SizeOf(shfi), flags);

        if (shfi.hIcon != IntPtr.Zero)
        {
            var icon = (Icon)Icon.FromHandle(shfi.hIcon).Clone();
            NativeMethods.DestroyIcon(shfi.hIcon);
            return icon;
        }
        return SystemIcons.Application;
    }

    public static string? ResolveShortcut(string lnkPath)
    {
        try
        {
            // Use WScript.Shell COM object to resolve .lnk
            Type? shellType = Type.GetTypeFromProgID("WScript.Shell");
            if (shellType == null) return null;
            dynamic shell = Activator.CreateInstance(shellType)!;
            dynamic shortcut = shell.CreateShortcut(lnkPath);
            string? targetPath = shortcut.TargetPath as string;

            // Si el TargetPath está vacío o no existe, devolver la ruta original del .lnk
            // para que pueda ser usado con UseShellExecute
            if (string.IsNullOrEmpty(targetPath) || !File.Exists(targetPath))
                return lnkPath;

            return targetPath;
        }
        catch { return lnkPath; }
    }

    [DllImport("gdi32.dll")]
    private static extern bool DeleteObject(IntPtr hObject);

    // COM Interfaces for IShellLink
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

public static class NativeMethods
{
    public const uint SHGFI_ICON = 0x100;
    public const uint SHGFI_LARGEICON = 0x0;
    public const uint SHGFI_SMALLICON = 0x1;
    public const uint SHGFI_USEFILEATTRIBUTES = 0x10;
    public const uint FILE_ATTRIBUTE_DIRECTORY = 0x10;

    [StructLayout(LayoutKind.Sequential)]
    public struct SHFILEINFO
    {
        public IntPtr hIcon;
        public int iIcon;
        public uint dwAttributes;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
        public string szDisplayName;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 80)]
        public string szTypeName;
    }

    [DllImport("shell32.dll", CharSet = CharSet.Unicode)]
    public static extern IntPtr SHGetFileInfo(string pszPath, uint dwFileAttributes,
        ref SHFILEINFO psfi, uint cbSizeFileInfo, uint uFlags);

    [DllImport("shell32.dll", CharSet = CharSet.Unicode)]
    public static extern int ExtractIconEx(string lpszFile, int nIconIndex, 
        [Out] IntPtr[] phiconLarge, [Out] IntPtr[] phiconSmall, int nIcons);

    [DllImport("user32.dll", SetLastError = true)]
    public static extern bool DestroyIcon(IntPtr hIcon);

    [DllImport("shell32.dll", CharSet = CharSet.Unicode, PreserveSig = false)]
    public static extern int SHCreateItemFromParsingName(
        [MarshalAs(UnmanagedType.LPWStr)] string pszPath,
        IntPtr pbc,
        ref Guid riid,
        out IntPtr ppv);

    [DllImport("gdi32.dll")]
    public static extern bool DeleteObject(IntPtr hObject);

    [DllImport("gdi32.dll")]
    public static extern IntPtr CreateCompatibleDC(IntPtr hdc);

    [DllImport("gdi32.dll")]
    public static extern bool DeleteDC(IntPtr hdc);

    [DllImport("gdi32.dll")]
    public static extern int GetDIBits(IntPtr hdc, IntPtr hbmp, uint uStartScan,
        uint cScanLines, IntPtr lpvBits, ref BITMAPINFOHEADER lpbi, uint uUsage);

    [StructLayout(LayoutKind.Sequential)]
    public struct BITMAPINFOHEADER
    {
        public uint  biSize;
        public int   biWidth;
        public int   biHeight;
        public ushort biPlanes;
        public ushort biBitCount;
        public uint  biCompression;
        public uint  biSizeImage;
        public int   biXPelsPerMeter;
        public int   biYPelsPerMeter;
        public uint  biClrUsed;
        public uint  biClrImportant;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct SIZE
    {
        public int cx;
        public int cy;
    }

    // IShellItemImageFactory interface
    [ComImport]
    [Guid("bcc18b79-ba16-442f-80c4-8a59c30c463b")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IShellItemImageFactory
    {
        [PreserveSig]
        int GetImage(SIZE size, int flags, out IntPtr phbm);
    }

    // IShellFolder interface
    [ComImport]
    [Guid("000214E6-0000-0000-C000-000000000046")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IShellFolder
    {
        [PreserveSig]
        int ParseDisplayName(IntPtr hwnd, IntPtr pbc, [MarshalAs(UnmanagedType.LPWStr)] string pszDisplayName,
            ref uint pchEaten, out IntPtr ppidl, ref uint pdwAttributes);

        [PreserveSig]
        int EnumObjects(IntPtr hwnd, uint grfFlags, out IntPtr ppenumIDList);

        [PreserveSig]
        int BindToObject(IntPtr pidl, IntPtr pbc, ref Guid riid, out IntPtr ppv);

        [PreserveSig]
        int BindToStorage(IntPtr pidl, IntPtr pbc, ref Guid riid, out IntPtr ppv);

        [PreserveSig]
        int CompareIDs(IntPtr lParam, IntPtr pidl1, IntPtr pidl2);

        [PreserveSig]
        int CreateViewObject(IntPtr hwndOwner, ref Guid riid, out IntPtr ppv);

        [PreserveSig]
        int GetAttributesOf(uint cidl, IntPtr[] apidl, ref uint rgfInOut);

        [PreserveSig]
        int GetUIObjectOf(IntPtr hwndOwner, uint cidl, IntPtr[] apidl,
            ref Guid riid, uint rgfReserved, out IntPtr ppv);

        [PreserveSig]
        int GetDisplayNameOf(IntPtr pidl, uint uFlags, out IntPtr pName);

        [PreserveSig]
        int SetNameOf(IntPtr hwnd, IntPtr pidl, [MarshalAs(UnmanagedType.LPWStr)] string pszName,
            uint uFlags, out IntPtr ppidlOut);
    }

    // IExtractIconW interface
    [ComImport]
    [Guid("000214FA-0000-0000-C000-000000000046")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IExtractIconW
    {
        int GetIconLocation(uint uFlags, [Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder szIconFile,
            int cchMax, out int piIndex, out uint pwFlags);

        int Extract([MarshalAs(UnmanagedType.LPWStr)] string pszFile, uint nIconIndex,
            out IntPtr phiconLarge, out IntPtr phiconSmall, uint nIconSize);
    }
}

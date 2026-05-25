using System.Drawing;
using System.Drawing.Drawing2D;

namespace LaunchDock.Helpers;

public static class TrayIconHelper
{
    public static Icon CreateSettingsIcon()
    {
        int size = 16;
        using var bitmap = new Bitmap(size, size);
        using var graphics = Graphics.FromImage(bitmap);

        graphics.SmoothingMode = SmoothingMode.AntiAlias;
        graphics.Clear(Color.Transparent);

        // Dibujar un círculo de fondo
        using var brush = new SolidBrush(Color.FromArgb(255, 233, 69, 96)); // Color #E94560
        graphics.FillEllipse(brush, 1, 1, size - 2, size - 2);

        // Dibujar el símbolo de engranaje simplificado
        using var pen = new Pen(Color.White, 1.5f);

        // Centro del engranaje (círculo pequeño)
        int centerX = size / 2;
        int centerY = size / 2;
        graphics.DrawEllipse(pen, centerX - 2, centerY - 2, 4, 4);

        // Líneas del engranaje (6 dientes)
        for (int i = 0; i < 6; i++)
        {
            double angle = i * Math.PI / 3; // 60 grados
            int x1 = (int)(centerX + Math.Cos(angle) * 3);
            int y1 = (int)(centerY + Math.Sin(angle) * 3);
            int x2 = (int)(centerX + Math.Cos(angle) * 6);
            int y2 = (int)(centerY + Math.Sin(angle) * 6);
            graphics.DrawLine(pen, x1, y1, x2, y2);
        }

        // Convertir bitmap a Icon
        IntPtr hIcon = bitmap.GetHicon();
        Icon icon = Icon.FromHandle(hIcon);
        return (Icon)icon.Clone();
    }
}

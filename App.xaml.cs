using System.Windows;
using System.Drawing;
using WinForms = System.Windows.Forms;
using LaunchDock.Views;
using LaunchDock.Helpers;

namespace LaunchDock;

public partial class App : System.Windows.Application
{
    private WinForms.NotifyIcon? _trayIcon;
    private MainBarWindow? _mainWindow;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        ConfigManager.Load();
        BarManager.Load();

        _mainWindow = new MainBarWindow();
        _mainWindow.Show();

        // Restaurar barras secundarias guardadas
        foreach (var barId in BarManager.BarIds)
        {
            var mgr = new ConfigManager(barId);
            mgr.LoadInstance();
            var bar = new MainBarWindow(mgr, isPrimary: false);
            bar.Show();
        }

        SetupTrayIcon();
    }

    private void SetupTrayIcon()
    {
        Icon? trayIcon = null;

        // Intentar cargar el icono desde el archivo
        try
        {
            string iconPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "LaunchDock.ico");
            if (System.IO.File.Exists(iconPath))
            {
                trayIcon = new Icon(iconPath);
            }
        }
        catch
        {
            // Si falla, usar el icono generado
        }

        _trayIcon = new WinForms.NotifyIcon
        {
            Text = "LaunchDock",
            Visible = true,
            Icon = trayIcon ?? TrayIconHelper.CreateSettingsIcon()
        };

        var menu = new WinForms.ContextMenuStrip();
        menu.Items.Add("⚙ Configuración", null, (s, e) => _mainWindow?.OpenSettings());
        menu.Items.Add("✏ Modo Edición", null, (s, e) => _mainWindow?.ToggleEditMode());
        menu.Items.Add(new WinForms.ToolStripSeparator());
        menu.Items.Add("✕ Cerrar LaunchDock", null, (s, e) => ExitApp());

        _trayIcon.ContextMenuStrip = menu;
        _trayIcon.DoubleClick += (s, e) => _mainWindow?.ToggleEditMode();
    }

    private void ExitApp()
    {
        ConfigManager.Save();
        _trayIcon?.Dispose();
        Shutdown();
    }

    protected override void OnExit(ExitEventArgs e)
    {
        _trayIcon?.Dispose();
        base.OnExit(e);
    }
}

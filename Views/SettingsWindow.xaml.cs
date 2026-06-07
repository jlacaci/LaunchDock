using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using LaunchDock.Helpers;
using LaunchDock.Models;
using WpfMessageBox = System.Windows.MessageBox;

namespace LaunchDock.Views;

public partial class SettingsWindow : Window
{
    private readonly ConfigManager _configManager;
    private AppConfig Cfg => _configManager.InstanceConfig;

    public SettingsWindow() : this(ConfigManager.Instance) { }

    public SettingsWindow(ConfigManager configManager)
    {
        InitializeComponent();
        _configManager = configManager;
        LoadCurrentConfig();
        MouseLeftButtonDown += (s, e) => DragMove();
        Loaded += (s, e) =>
        {
            ClipWindowToCornerRadius();
            var ver = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
            if (VersionText != null && ver != null)
                VersionText.Text = $"v{ver.Major}.{ver.Minor}.{ver.Build}";
        };

        // Mostrar botón "Cerrar barra" solo en barras secundarias
        if (CloseBarBtn != null)
            CloseBarBtn.Visibility = configManager.BarId == "main"
                ? Visibility.Collapsed : Visibility.Visible;
    }

    private void LoadCurrentConfig()
    {
        var cfg = Cfg;
        foreach (ComboBoxItem item in PositionCombo.Items)
            if (item.Tag?.ToString() == cfg.Position) { PositionCombo.SelectedItem = item; break; }
        if (PositionCombo.SelectedIndex < 0) PositionCombo.SelectedIndex = 1;
        AutoHideCheck.IsChecked = cfg.AutoHide;
        if (cfg.Orientation == "Vertical") OrientationVertical.IsChecked = true;
        else OrientationHorizontal.IsChecked = true;
        switch (cfg.IconSize)
        {
            case 16: Size16.IsChecked = true; break;
            case 48: Size48.IsChecked = true; break;
            default: Size32.IsChecked = true; break;
        }
        BackgroundColorText.Text = cfg.BackgroundColor ?? "#CC1A1A2E";
        AccentColorText.Text     = cfg.AccentColor     ?? "#E94560";
        TextColorText.Text       = cfg.TextColor       ?? "#EAEAEA";
        PopupMenuColorText.Text  = cfg.PopupMenuColor  ?? "#EE1E1E35";
        FontFamilyCombo.SelectedIndex = 0;
        foreach (ComboBoxItem item in FontFamilyCombo.Items)
            if (item.Tag?.ToString() == cfg.FontFamily) { FontFamilyCombo.SelectedItem = item; break; }
        FontSizeSlider.Value     = cfg.FontSize;
        OpacitySlider.Value      = cfg.Opacity;
        CornerRadiusSlider.Value = cfg.CornerRadiusValue;
        HighlightActiveTheme(cfg.ThemeName);
    }

    private void Save_Click(object sender, RoutedEventArgs e)
    {
        var cfg = Cfg;
        if (PositionCombo.SelectedItem is ComboBoxItem sel) cfg.Position = sel.Tag?.ToString() ?? "Bottom";
        cfg.AutoHide    = AutoHideCheck.IsChecked == true;
        cfg.Orientation = OrientationVertical.IsChecked == true ? "Vertical" : "Horizontal";
        if (Size16.IsChecked == true) cfg.IconSize = 16;
        else if (Size48.IsChecked == true) cfg.IconSize = 48;
        else cfg.IconSize = 32;
        cfg.BackgroundColor   = BackgroundColorText.Text;
        cfg.AccentColor       = AccentColorText.Text;
        cfg.TextColor         = TextColorText.Text;
        cfg.PopupMenuColor    = PopupMenuColorText.Text;
        if (FontFamilyCombo.SelectedItem is ComboBoxItem fi) cfg.FontFamily = fi.Tag?.ToString() ?? "Segoe UI";
        cfg.FontSize          = (int)FontSizeSlider.Value;
        cfg.Opacity           = (int)OpacitySlider.Value;
        cfg.CornerRadiusValue = (int)CornerRadiusSlider.Value;
        _configManager.SaveInstance();
        DialogResult = true;
        Close();
    }

    private void Cancel_Click(object sender, RoutedEventArgs e) { DialogResult = false; Close(); }
    private void Close_Click(object sender, RoutedEventArgs e)  { DialogResult = false; Close(); }

    private void ClearCache_Click(object sender, RoutedEventArgs e)
    {
        int count = IconCacheHelper.CountCachedIcons();
        if (count == 0) { CacheInfoText.Text = "La cache ya estaba vacia."; ClearCacheBtn.IsEnabled = false; return; }
        IconCacheHelper.ClearAllCache();
        CacheInfoText.Text = count + " iconos eliminados.";
        ClearCacheBtn.Content = "Limpiado";
        ClearCacheBtn.IsEnabled = false;
    }

    // BACKUP / RESTORE

    private void ExportConfig_Click(object sender, RoutedEventArgs e)
    {
        var dlg = new System.Windows.Forms.SaveFileDialog
        {
            Title = "Exportar configuracion",
            Filter = "JSON|*.json|Todos|*.*",
            FileName = "LaunchDock-config.json",
            DefaultExt = "json",
        };
        if (dlg.ShowDialog() != System.Windows.Forms.DialogResult.OK) return;
        try
        {
            string barFile = _configManager.BarId == "main" ? "config.json" : $"bar_{_configManager.BarId}.json";
            var src = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "LaunchDock", barFile);
            File.Copy(src, dlg.FileName, overwrite: true);
            WpfMessageBox.Show("Configuracion exportada correctamente.", "Exportar", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            WpfMessageBox.Show("Error al exportar: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void ImportConfig_Click(object sender, RoutedEventArgs e)
    {
        var dlg = new System.Windows.Forms.OpenFileDialog
        {
            Title = "Importar configuracion",
            Filter = "JSON|*.json|Todos|*.*",
            DereferenceLinks = false,
        };
        if (dlg.ShowDialog() != System.Windows.Forms.DialogResult.OK) return;
        if (WpfMessageBox.Show("Se reemplazara la configuracion actual. Continuar?", "Importar",
            MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes) return;
        try
        {
            var dest = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "LaunchDock", "config.json");
            File.Copy(dlg.FileName, dest, overwrite: true);
            _configManager.LoadInstance();
            WpfMessageBox.Show("Configuracion importada. Reinicia LaunchDock para aplicar los cambios.", "Importar", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            WpfMessageBox.Show("Error al importar: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    // TEMAS

    private static readonly Dictionary<string, (string Bg, string Accent, string Text, string Popup)> _themes = new()
    {
        //                     Fondo barra       Acento      Texto       Fondo popup (= fondo barra)
        ["Oscuro"]           = ("#CC1A1A2E", "#E94560", "#EAEAEA", "#CC1A1A2E"),
        ["Dracula"]          = ("#CC282A36", "#BD93F9", "#F8F8F2", "#CC282A36"),
        ["Nord"]             = ("#CC2E3440", "#88C0D0", "#ECEFF4", "#CC2E3440"),
        ["Claro"]            = ("#F0F0F0F0", "#0078D4", "#1A1A1A", "#F0F0F0F0"),
        ["NeumorficoOscuro"] = ("#CC2D2D3A", "#7C83FD", "#D0D0E8", "#CC2D2D3A"),
    };

    private void ThemePreset_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not System.Windows.Controls.Button btn) return;
        string tag = btn.Tag?.ToString() ?? "";
        if (!_themes.TryGetValue(tag, out var t)) return;
        BackgroundColorText.Text  = t.Bg;
        AccentColorText.Text      = t.Accent;
        TextColorText.Text        = t.Text;
        PopupMenuColorText.Text   = t.Popup;
        Cfg.ThemeName = tag;
        HighlightActiveTheme(tag);
    }

    private void HighlightActiveTheme(string themeName)
    {
        var buttons = new[] { ThemeOscuro, ThemeDracula, ThemeNord, ThemeClaro, ThemeNeuDark };
        foreach (var btn in buttons)
        {
            bool active = btn.Tag?.ToString() == themeName;
            btn.BorderBrush = active
                ? new System.Windows.Media.SolidColorBrush(
                    (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString(
                        Cfg.AccentColor))
                : new System.Windows.Media.SolidColorBrush(
                    System.Windows.Media.Color.FromRgb(0x44, 0x44, 0x66));
            btn.FontWeight = active ? FontWeights.Bold : FontWeights.Normal;
        }
    }

    private void CloseBar_Click(object sender, RoutedEventArgs e)
    {
        var result = WpfMessageBox.Show(
            "Si se cierra esta barra no se podrá recuperar su configuración ni la barra.\n\n¿Deseas continuar?",
            "Cerrar barra",
            MessageBoxButton.OKCancel,
            MessageBoxImage.Warning);

        if (result != MessageBoxResult.OK) return;

        DialogResult = false;
        Close();

        if (Owner is MainBarWindow bar)
            bar.CloseThisBar();
    }

    private void ClipWindowToCornerRadius()
    {
        if (Content is not Border border) return;
        double r = border.CornerRadius.TopLeft;
        var geo = new System.Windows.Media.RectangleGeometry(
            new System.Windows.Rect(0, 0, ActualWidth, ActualHeight), r, r);
        border.Clip = geo;
        SizeChanged += (s, e) =>
        {
            geo.Rect = new System.Windows.Rect(0, 0, ActualWidth, ActualHeight);
        };
    }
}

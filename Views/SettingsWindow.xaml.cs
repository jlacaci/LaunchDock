using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using LaunchDock.Helpers;
using WpfMessageBox = System.Windows.MessageBox;

namespace LaunchDock.Views;

public partial class SettingsWindow : Window
{
    public SettingsWindow()
    {
        InitializeComponent();
        LoadCurrentConfig();
        MouseLeftButtonDown += (s, e) => DragMove();
    }

    private void LoadCurrentConfig()
    {
        var cfg = ConfigManager.Config;
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
        var cfg = ConfigManager.Config;
        if (PositionCombo.SelectedItem is ComboBoxItem sel) cfg.Position = sel.Tag?.ToString() ?? "Bottom";
        cfg.AutoHide    = AutoHideCheck.IsChecked == true;
        cfg.Orientation = OrientationVertical.IsChecked == true ? "Vertical" : "Horizontal";
        if (Size16.IsChecked == true) cfg.IconSize = 16;
        else if (Size48.IsChecked == true) cfg.IconSize = 48;
        else cfg.IconSize = 32;
        cfg.BackgroundColor   = BackgroundColorText.Text;
        cfg.AccentColor       = AccentColorText.Text;
        cfg.TextColor         = TextColorText.Text;
        if (FontFamilyCombo.SelectedItem is ComboBoxItem fi) cfg.FontFamily = fi.Tag?.ToString() ?? "Segoe UI";
        cfg.FontSize          = (int)FontSizeSlider.Value;
        cfg.Opacity           = (int)OpacitySlider.Value;
        cfg.CornerRadiusValue = (int)CornerRadiusSlider.Value;
        ConfigManager.Save();
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
            var src = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "LaunchDock", "config.json");
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
            ConfigManager.Load();
            WpfMessageBox.Show("Configuracion importada. Reinicia LaunchDock para aplicar los cambios.", "Importar", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            WpfMessageBox.Show("Error al importar: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    // TEMAS

    private static readonly Dictionary<string, (string Bg, string Accent, string Text)> _themes = new()
    {
        ["Oscuro"]           = ("#CC1A1A2E", "#E94560", "#EAEAEA"),
        ["Dracula"]          = ("#CC282A36", "#BD93F9", "#F8F8F2"),
        ["Nord"]             = ("#CC2E3440", "#88C0D0", "#ECEFF4"),
        ["Claro"]            = ("#F2F2F2F2", "#0078D4", "#1A1A1A"),
        ["NeumorficoOscuro"] = ("#CC2D2D3A", "#7C83FD", "#D0D0E8"),
    };

    private void ThemePreset_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not System.Windows.Controls.Button btn) return;
        string tag = btn.Tag?.ToString() ?? "";
        if (!_themes.TryGetValue(tag, out var t)) return;
        BackgroundColorText.Text = t.Bg;
        AccentColorText.Text     = t.Accent;
        TextColorText.Text       = t.Text;
        ConfigManager.Config.ThemeName = tag;
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
                        ConfigManager.Config.AccentColor))
                : new System.Windows.Media.SolidColorBrush(
                    System.Windows.Media.Color.FromRgb(0x44, 0x44, 0x66));
            btn.FontWeight = active ? FontWeights.Bold : FontWeights.Normal;
        }
    }
}

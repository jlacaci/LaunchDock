using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using LaunchDock.Helpers;

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

        // Position
        foreach (ComboBoxItem item in PositionCombo.Items)
        {
            if (item.Tag?.ToString() == cfg.Position)
            {
                PositionCombo.SelectedItem = item;
                break;
            }
        }
        if (PositionCombo.SelectedIndex < 0) PositionCombo.SelectedIndex = 1;

        // Auto hide
        AutoHideCheck.IsChecked = cfg.AutoHide;

        // Orientation
        if (cfg.Orientation == "Vertical")
            OrientationVertical.IsChecked = true;
        else
            OrientationHorizontal.IsChecked = true;

        // Icon size
        switch (cfg.IconSize)
        {
            case 16: Size16.IsChecked = true; break;
            case 48: Size48.IsChecked = true; break;
            default: Size32.IsChecked = true; break;
        }

        // Personalización - Colores
        BackgroundColorText.Text = cfg.BackgroundColor ?? "#CC1A1A2E";
        AccentColorText.Text = cfg.AccentColor ?? "#E94560";
        TextColorText.Text = cfg.TextColor ?? "#EAEAEA";

        // Fuente
        FontFamilyCombo.SelectedIndex = 0;
        foreach (ComboBoxItem item in FontFamilyCombo.Items)
        {
            if (item.Tag?.ToString() == cfg.FontFamily)
            {
                FontFamilyCombo.SelectedItem = item;
                break;
            }
        }

        FontSizeSlider.Value = cfg.FontSize;
        OpacitySlider.Value = cfg.Opacity;
        CornerRadiusSlider.Value = cfg.CornerRadiusValue;
        HighlightActiveTheme(cfg.ThemeName);
    }

    private void Save_Click(object sender, RoutedEventArgs e)
    {
        var cfg = ConfigManager.Config;

        // Position
        if (PositionCombo.SelectedItem is ComboBoxItem selected)
            cfg.Position = selected.Tag?.ToString() ?? "Bottom";

        // Auto hide
        cfg.AutoHide = AutoHideCheck.IsChecked == true;

        // Orientation
        cfg.Orientation = OrientationVertical.IsChecked == true ? "Vertical" : "Horizontal";

        // Icon size
        if (Size16.IsChecked == true) cfg.IconSize = 16;
        else if (Size48.IsChecked == true) cfg.IconSize = 48;
        else cfg.IconSize = 32;

        // Personalización - Colores
        cfg.BackgroundColor = BackgroundColorText.Text;
        cfg.AccentColor = AccentColorText.Text;
        cfg.TextColor = TextColorText.Text;

        // Fuente
        if (FontFamilyCombo.SelectedItem is ComboBoxItem fontItem)
            cfg.FontFamily = fontItem.Tag?.ToString() ?? "Segoe UI";

        cfg.FontSize = (int)FontSizeSlider.Value;
        cfg.Opacity = (int)OpacitySlider.Value;
        cfg.CornerRadiusValue = (int)CornerRadiusSlider.Value;

        ConfigManager.Save();
        DialogResult = true;
        Close();
    }

    private void Cancel_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }

    private void Close_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }

    private void ClearCache_Click(object sender, RoutedEventArgs e)
    {
        int count = IconCacheHelper.CountCachedIcons();

        if (count == 0)
        {
            CacheInfoText.Text = "? La caché ya estaba vacía.";
            ClearCacheBtn.IsEnabled = false;
            return;
        }

        IconCacheHelper.ClearAllCache();

        CacheInfoText.Text = $"? {count} iconos eliminados. Vuelve a agregar los accesos directos para regenerarlos.";
        ClearCacheBtn.Content = "? Limpiado";
        ClearCacheBtn.IsEnabled = false;
    }

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

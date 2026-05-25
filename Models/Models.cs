using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.IO;

namespace LaunchDock.Models;

public class AppConfig
{
    public string Position { get; set; } = "Bottom";       // Top, Bottom, Left, Right, Floating
    public bool AutoHide { get; set; } = false;
    public string Theme { get; set; } = "Dark";            // Dark, Light
    public string Orientation { get; set; } = "Horizontal"; // Horizontal, Vertical
    public int IconSize { get; set; } = 32;
    public double FloatX { get; set; } = 100;
    public double FloatY { get; set; } = 100;

    // Personalización
    public string BackgroundColor { get; set; } = "#CC1A1A2E";
    public string AccentColor { get; set; } = "#E94560";
    public string TextColor { get; set; } = "#EAEAEA";
    public string FontFamily { get; set; } = "Segoe UI";
    public int FontSize { get; set; } = 13;
    public int Opacity { get; set; } = 95;  // 0-100%

    public List<CategoryModel> Categories { get; set; } = new();
}

public class CategoryModel : INotifyPropertyChanged
{
    private string _name = "Nueva Categoría";
    private ObservableCollection<ShortcutModel> _shortcuts = new();

    public string Id { get; set; } = Guid.NewGuid().ToString();

    public string Name
    {
        get => _name;
        set { _name = value; OnPropertyChanged(); }
    }

    public ObservableCollection<ShortcutModel> Shortcuts
    {
        get => _shortcuts;
        set { _shortcuts = value; OnPropertyChanged(); }
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}

public class ShortcutModel : INotifyPropertyChanged
{
    private string _name = "";
    private string _targetPath = "";
    private string? _iconPath;

    public string Id { get; set; } = Guid.NewGuid().ToString();

    public string Name
    {
        get => _name;
        set { _name = value; OnPropertyChanged(); }
    }

    public string TargetPath
    {
        get => _targetPath;
        set { _targetPath = value; OnPropertyChanged(); }
    }

    public string? IconPath
    {
        get => _iconPath;
        set { _iconPath = value; OnPropertyChanged(); }
    }

    // Runtime-only: used for display
    public bool IsFolder => Directory.Exists(TargetPath);

    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}

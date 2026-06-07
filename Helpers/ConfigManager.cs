using System.IO;
using Newtonsoft.Json;
using LaunchDock.Models;

namespace LaunchDock.Helpers;

public class ConfigManager
{
    private static readonly string AppDataDir = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "LaunchDock");

    // ── Instancia estática para la barra principal (compatibilidad con código existente) ──
    private static ConfigManager? _instance;
    public static ConfigManager Instance => _instance ??= new ConfigManager("main");

    // Acceso directo estático para no romper código existente
    public static AppConfig Config => Instance._config;
    public static void Load() => Instance.LoadInstance();
    public static void Save() => Instance.SaveInstance();

    // ── Estado de instancia ──
    private AppConfig _config = new();
    private readonly string _configPath;
    public string BarId { get; }
    public AppConfig InstanceConfig => _config;

    public ConfigManager(string barId)
    {
        BarId = barId;
        string fileName = barId == "main" ? "config.json" : $"bar_{barId}.json";
        _configPath = Path.Combine(AppDataDir, fileName);
    }

    public void LoadInstance()
    {
        try
        {
            if (File.Exists(_configPath))
            {
                var json = File.ReadAllText(_configPath);
                _config = JsonConvert.DeserializeObject<AppConfig>(json) ?? CreateDefaultConfig();
                _config.BarId = BarId;
            }
            else
            {
                _config = CreateDefaultConfig();
                _config.BarId = BarId;
                SaveInstance();
            }
        }
        catch
        {
            _config = CreateDefaultConfig();
            _config.BarId = BarId;
        }
    }

    public void SaveInstance()
    {
        try
        {
            Directory.CreateDirectory(AppDataDir);
            _config.BarId = BarId;
            var json = JsonConvert.SerializeObject(_config, Formatting.Indented);
            File.WriteAllText(_configPath, json);
        }
        catch { /* silent */ }
    }

    public void DeleteConfig()
    {
        try { if (File.Exists(_configPath)) File.Delete(_configPath); }
        catch { /* silent */ }
    }

    private static AppConfig CreateDefaultConfig()
    {
        return new AppConfig
        {
            Position = "Bottom",
            AutoHide = false,
            Theme = "Dark",
            IconSize = 32,
            ThemeName = "Oscuro",
            CornerRadiusValue = 12,
            Categories = new List<CategoryModel>
            {
                new() { Name = "OFIMÁTICA",  Shortcuts = new() },
                new() { Name = "INTERNET",   Shortcuts = new() },
                new() { Name = "FOTOGRAFÍA", Shortcuts = new() },
                new() { Name = "VIDEO",      Shortcuts = new() },
                new() { Name = "AUDIO",      Shortcuts = new() },
            }
        };
    }
}

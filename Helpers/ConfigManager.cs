using System.IO;
using Newtonsoft.Json;
using LaunchDock.Models;

namespace LaunchDock.Helpers;

public static class ConfigManager
{
    private static readonly string ConfigPath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "LaunchDock", "config.json");

    public static AppConfig Config { get; private set; } = new();

    public static void Load()
    {
        try
        {
            if (File.Exists(ConfigPath))
            {
                var json = File.ReadAllText(ConfigPath);
                Config = JsonConvert.DeserializeObject<AppConfig>(json) ?? new AppConfig();
            }
            else
            {
                Config = CreateDefaultConfig();
                Save();
            }
        }
        catch
        {
            Config = CreateDefaultConfig();
        }
    }

    public static void Save()
    {
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(ConfigPath)!);
            var json = JsonConvert.SerializeObject(Config, Formatting.Indented);
            File.WriteAllText(ConfigPath, json);
        }
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

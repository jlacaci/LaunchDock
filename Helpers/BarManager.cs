using System.IO;
using Newtonsoft.Json;

namespace LaunchDock.Helpers;

/// <summary>
/// Gestiona la lista de IDs de barras secundarias activas,
/// persistida en %AppData%\LaunchDock\bars.json
/// </summary>
public static class BarManager
{
    private static readonly string BarsPath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "LaunchDock", "bars.json");

    private static List<string> _barIds = new();

    public static IReadOnlyList<string> BarIds => _barIds;

    public static void Load()
    {
        try
        {
            if (File.Exists(BarsPath))
            {
                var json = File.ReadAllText(BarsPath);
                _barIds = JsonConvert.DeserializeObject<List<string>>(json) ?? new();
            }
        }
        catch { _barIds = new(); }
    }

    public static string AddBar()
    {
        string id = Guid.NewGuid().ToString("N")[..8];
        _barIds.Add(id);
        Save();
        return id;
    }

    public static void RemoveBar(string barId)
    {
        _barIds.Remove(barId);
        Save();
        // Borrar también el archivo de configuración de la barra
        var mgr = new ConfigManager(barId);
        mgr.DeleteConfig();
    }

    private static void Save()
    {
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(BarsPath)!);
            File.WriteAllText(BarsPath, JsonConvert.SerializeObject(_barIds, Formatting.Indented));
        }
        catch { }
    }
}

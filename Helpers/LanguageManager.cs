using System.Globalization;
using System.IO;
using System.Windows;
using WpfApp = System.Windows.Application;

namespace LaunchDock.Helpers;

public static class LanguageManager
{
    private static readonly string LangDir = Path.Combine(
        AppDomain.CurrentDomain.BaseDirectory, "Language");

    /// <summary>
    /// Devuelve la lista de idiomas disponibles escaneando la carpeta Language/.
    /// Cada entrada es (código, nombre legible).
    /// </summary>
    public static List<(string Code, string DisplayName)> GetAvailableLanguages()
    {
        var result = new List<(string, string)>();
        if (!Directory.Exists(LangDir)) return result;

        foreach (var file in Directory.GetFiles(LangDir, "*.xaml").OrderBy(f => f))
        {
            string code = Path.GetFileNameWithoutExtension(file);
            try
            {
                var rd = new ResourceDictionary { Source = new Uri(file, UriKind.Absolute) };
                string displayName = rd.Contains("Lang.Name")
                    ? rd["Lang.Name"]!.ToString()!
                    : code;
                result.Add((code, displayName));
            }
            catch
            {
                result.Add((code, code));
            }
        }
        return result;
    }

    /// <summary>
    /// Detecta el idioma del sistema y devuelve el código del archivo .xaml más cercano.
    /// Si no hay coincidencia devuelve "es" como fallback.
    /// </summary>
    public static string DetectSystemLanguage()
    {
        string culture = CultureInfo.CurrentUICulture.TwoLetterISOLanguageName.ToLower();
        string filePath = Path.Combine(LangDir, $"{culture}.xaml");
        if (File.Exists(filePath)) return culture;
        // Fallback a español
        return "es";
    }

    /// <summary>
    /// Carga el ResourceDictionary del idioma indicado y lo aplica a los recursos de la app.
    /// </summary>
    public static void Apply(string languageCode)
    {
        string filePath = Path.Combine(LangDir, $"{languageCode}.xaml");
        if (!File.Exists(filePath))
            filePath = Path.Combine(LangDir, "es.xaml");
        if (!File.Exists(filePath)) return;

        try
        {
            var rd = new ResourceDictionary { Source = new Uri(filePath, UriKind.Absolute) };

            // Eliminar el diccionario de idioma previo si existe
            var existing = WpfApp.Current.Resources.MergedDictionaries
                .FirstOrDefault(d => d.Source != null &&
                    d.Source.OriginalString.Contains("Language/") ||
                    d.Source != null && d.Source.OriginalString.Contains("Language\\"));
            if (existing != null)
                WpfApp.Current.Resources.MergedDictionaries.Remove(existing);

            WpfApp.Current.Resources.MergedDictionaries.Add(rd);
        }
        catch { /* silent */ }
    }

    /// <summary>
    /// Obtiene un string traducido del diccionario de recursos activo.
    /// </summary>
    public static string Get(string key, params object[] args)
    {
        if (WpfApp.Current.Resources.Contains(key))
        {
            string value = WpfApp.Current.Resources[key]?.ToString() ?? key;
            return args.Length > 0 ? string.Format(value, args) : value;
        }
        return key;
    }
}

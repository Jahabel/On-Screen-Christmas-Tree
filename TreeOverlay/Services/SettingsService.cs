using System.IO;
using System.Text.Json;
using TreeOverlay.Models;

namespace TreeOverlay.Services;

public class SettingsService
{
    private readonly string _settingsPath;
    private readonly JsonSerializerOptions _options = new() { WriteIndented = true };

    public SettingsService()
    {
        var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        var folder = Path.Combine(appData, "TreeOverlay");
        Directory.CreateDirectory(folder);
        _settingsPath = Path.Combine(folder, "settings.json");
    }

    public TreeOverlaySettings Load()
    {
        if (!File.Exists(_settingsPath))
        {
            return new TreeOverlaySettings();
        }

        try
        {
            var json = File.ReadAllText(_settingsPath);
            return JsonSerializer.Deserialize<TreeOverlaySettings>(json, _options) ?? new TreeOverlaySettings();
        }
        catch
        {
            return new TreeOverlaySettings();
        }
    }

    public void Save(TreeOverlaySettings settings)
    {
        var json = JsonSerializer.Serialize(settings, _options);
        File.WriteAllText(_settingsPath, json);
    }
}

using System.IO;

namespace TreeOverlay.Services;

public class StartupService
{
    private const string ShortcutName = "TreeOverlay-startup.cmd";

    public void SetRunOnStartup(bool enabled)
    {
        var shortcutPath = GetStartupShortcutPath();
        if (enabled)
        {
            var exePath = Path.Combine(AppContext.BaseDirectory, "TreeOverlay.exe");
            var content = $"@echo off{Environment.NewLine}\"{exePath}\"{Environment.NewLine}";
            File.WriteAllText(shortcutPath, content);
        }
        else if (File.Exists(shortcutPath))
        {
            File.Delete(shortcutPath);
        }
    }

    public bool IsRunOnStartupEnabled()
    {
        return File.Exists(GetStartupShortcutPath());
    }

    private static string GetStartupShortcutPath()
    {
        var startupFolder = Environment.GetFolderPath(Environment.SpecialFolder.Startup);
        return Path.Combine(startupFolder, ShortcutName);
    }
}

using System.Text.Json.Serialization;

namespace TreeOverlay.Models;

public class TreeOverlaySettings
{
    public string MonitorDeviceName { get; set; } = "Primary";
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public OverlayCorner Corner { get; set; } = OverlayCorner.BottomRight;
    public double Scale { get; set; } = 1.0;
    public double Opacity { get; set; } = 1.0;
    public bool AlwaysOnTop { get; set; } = true;
    public bool ClickThrough { get; set; } = true;
    public string GifPath { get; set; } = string.Empty;
    public List<string> RecentGifs { get; set; } = new();
    public bool OverlayRunning { get; set; } = true;
}

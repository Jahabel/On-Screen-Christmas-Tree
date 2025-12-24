using System.IO;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using TreeOverlay.Models;
using WpfAnimatedGif;
using Screen = System.Windows.Forms.Screen;

namespace TreeOverlay;

public partial class OverlayWindow : Window
{
    private const int GwlExstyle = -20;
    private const int WsExLayered = 0x00080000;
    private const int WsExTransparent = 0x00000020;

    private TreeOverlaySettings? _settings;
    private bool _applyClickThrough;

    public OverlayWindow()
    {
        InitializeComponent();
        Loaded += (_, _) => ApplySettings();
        SizeChanged += (_, _) => PositionWindow();
        SourceInitialized += (_, _) => ApplyClickThrough();
    }

    public void UpdateSettings(TreeOverlaySettings settings)
    {
        _settings = settings;
        ApplySettings();
    }

    private void ApplySettings()
    {
        if (_settings is null || !IsLoaded)
        {
            return;
        }

        Topmost = _settings.AlwaysOnTop;
        Opacity = _settings.Opacity;
        _applyClickThrough = _settings.ClickThrough;
        ApplyClickThrough();
        LoadGif(_settings.GifPath, _settings.Scale);
        PositionWindow();
    }

    private void LoadGif(string path, double scale)
    {
        if (!File.Exists(path))
        {
            return;
        }

        var image = new BitmapImage();
        image.BeginInit();
        image.CacheOption = BitmapCacheOption.OnLoad;
        image.UriSource = new Uri(path, UriKind.Absolute);
        image.EndInit();
        ImageBehavior.SetAnimatedSource(OverlayImage, image);
        OverlayImage.LayoutTransform = new ScaleTransform(scale, scale);
    }

    private void PositionWindow()
    {
        if (_settings is null || !IsLoaded)
        {
            return;
        }

        var screen = ResolveScreen(_settings.MonitorDeviceName);
        var dpi = VisualTreeHelper.GetDpi(this);
        var left = screen.Bounds.Left / dpi.DpiScaleX;
        var top = screen.Bounds.Top / dpi.DpiScaleY;
        var right = screen.Bounds.Right / dpi.DpiScaleX;
        var bottom = screen.Bounds.Bottom / dpi.DpiScaleY;

        var width = ActualWidth;
        var height = ActualHeight;

        switch (_settings.Corner)
        {
            case OverlayCorner.BottomRight:
                Left = right - width;
                Top = bottom - height;
                break;
            case OverlayCorner.BottomLeft:
                Left = left;
                Top = bottom - height;
                break;
            case OverlayCorner.TopRight:
                Left = right - width;
                Top = top;
                break;
            case OverlayCorner.TopLeft:
                Left = left;
                Top = top;
                break;
        }
    }

    private static Screen ResolveScreen(string deviceName)
    {
        if (string.Equals(deviceName, "Primary", StringComparison.OrdinalIgnoreCase))
        {
            return Screen.PrimaryScreen ?? Screen.AllScreens.First();
        }

        return Screen.AllScreens.FirstOrDefault(s => s.DeviceName == deviceName) ?? Screen.PrimaryScreen ?? Screen.AllScreens.First();
    }

    private void ApplyClickThrough()
    {
        if (!IsLoaded)
        {
            return;
        }

        var hwnd = new WindowInteropHelper(this).Handle;
        if (hwnd == IntPtr.Zero)
        {
            return;
        }

        var exStyle = GetWindowLong(hwnd, GwlExstyle);
        if (_applyClickThrough)
        {
            exStyle |= WsExLayered | WsExTransparent;
        }
        else
        {
            exStyle &= ~WsExTransparent;
        }

        SetWindowLong(hwnd, GwlExstyle, exStyle);
    }

    [DllImport("user32.dll", SetLastError = true)]
    private static extern int GetWindowLong(IntPtr hwnd, int index);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern int SetWindowLong(IntPtr hwnd, int index, int newStyle);
}

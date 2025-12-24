using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;
using TreeOverlay.Models;
using TreeOverlay.Services;
using TreeOverlay.Resources;
using Screen = System.Windows.Forms.Screen;
using Forms = System.Windows.Forms;

namespace TreeOverlay;

public partial class MainWindow : Window
{
    private readonly SettingsService _settingsService = new();
    private TreeOverlaySettings _settings;
    private OverlayWindow? _overlayWindow;
    private Forms.NotifyIcon? _notifyIcon;
    private bool _exitRequested;

    public MainWindow()
    {
        InitializeComponent();
        _settings = _settingsService.Load();
        Loaded += OnLoaded;
        Closing += OnClosing;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        InitializeTray();
        PopulateMonitors();
        PopulateCorners();
        BindSettingsToControls();
        HookEvents();
        EnsureDefaultGif();
        UpdateOverlayState();
        UpdateStartStopButton();
    }

    private void InitializeTray()
    {
        _notifyIcon = new Forms.NotifyIcon
        {
            Icon = System.Drawing.SystemIcons.Application,
            Visible = true,
            Text = "Tree Overlay"
        };

        var menu = new Forms.ContextMenuStrip();
        menu.Items.Add("Show Control Panel", null, (_, _) => ShowControlPanel());
        menu.Items.Add("Hide Control Panel", null, (_, _) => HideControlPanel());
        menu.Items.Add(new Forms.ToolStripSeparator());
        menu.Items.Add("Start Overlay", null, (_, _) => StartOverlay());
        menu.Items.Add("Stop Overlay", null, (_, _) => StopOverlay());
        menu.Items.Add(new Forms.ToolStripSeparator());
        menu.Items.Add("Previous GIF", null, (_, _) => CycleGif(-1));
        menu.Items.Add("Next GIF", null, (_, _) => CycleGif(1));
        menu.Items.Add(new Forms.ToolStripSeparator());
        menu.Items.Add("Quit", null, (_, _) => QuitApplication());
        _notifyIcon.ContextMenuStrip = menu;
        _notifyIcon.DoubleClick += (_, _) => ShowControlPanel();
    }

    private void PopulateMonitors()
    {
        MonitorComboBox.Items.Clear();
        MonitorComboBox.Items.Add(new ComboBoxItem { Content = "Primary", Tag = "Primary" });
        foreach (var screen in Screen.AllScreens)
        {
            var label = screen.Primary ? $"Primary - {screen.DeviceName}" : screen.DeviceName;
            MonitorComboBox.Items.Add(new ComboBoxItem { Content = label, Tag = screen.DeviceName });
        }
    }

    private void PopulateCorners()
    {
        CornerComboBox.ItemsSource = Enum.GetValues(typeof(OverlayCorner));
    }

    private void BindSettingsToControls()
    {
        SelectMonitor(_settings.MonitorDeviceName);
        CornerComboBox.SelectedItem = _settings.Corner;
        ScaleSlider.Value = _settings.Scale;
        OpacitySlider.Value = _settings.Opacity;
        AlwaysOnTopCheckBox.IsChecked = _settings.AlwaysOnTop;
        ClickThroughCheckBox.IsChecked = _settings.ClickThrough;
        GifPathTextBox.Text = _settings.GifPath;
        RefreshRecentList();
        UpdateScaleText();
        UpdateOpacityText();
    }

    private void HookEvents()
    {
        MonitorComboBox.SelectionChanged += (_, _) =>
        {
            if (MonitorComboBox.SelectedItem is ComboBoxItem item && item.Tag is string tag)
            {
                _settings.MonitorDeviceName = tag;
                SaveAndUpdate();
            }
        };

        CornerComboBox.SelectionChanged += (_, _) =>
        {
            if (CornerComboBox.SelectedItem is OverlayCorner corner)
            {
                _settings.Corner = corner;
                SaveAndUpdate();
            }
        };

        ScaleSlider.ValueChanged += (_, _) =>
        {
            _settings.Scale = ScaleSlider.Value;
            UpdateScaleText();
            SaveAndUpdate();
        };

        OpacitySlider.ValueChanged += (_, _) =>
        {
            _settings.Opacity = OpacitySlider.Value;
            UpdateOpacityText();
            SaveAndUpdate();
        };

        AlwaysOnTopCheckBox.Checked += (_, _) => ToggleAlwaysOnTop(true);
        AlwaysOnTopCheckBox.Unchecked += (_, _) => ToggleAlwaysOnTop(false);
        ClickThroughCheckBox.Checked += (_, _) => ToggleClickThrough(true);
        ClickThroughCheckBox.Unchecked += (_, _) => ToggleClickThrough(false);
        BrowseGifButton.Click += (_, _) => BrowseGif();
        StartStopButton.Click += (_, _) => ToggleOverlay();
        ResetButton.Click += (_, _) => ResetDefaults();
        RecentGifsListBox.MouseDoubleClick += (_, _) => ApplyRecentSelection();
    }

    private void UpdateScaleText() => ScaleValueText.Text = $"{ScaleSlider.Value:0.00}x";

    private void UpdateOpacityText() => OpacityValueText.Text = $"{OpacitySlider.Value:0.00}";

    private void ToggleAlwaysOnTop(bool value)
    {
        _settings.AlwaysOnTop = value;
        SaveAndUpdate();
    }

    private void ToggleClickThrough(bool value)
    {
        _settings.ClickThrough = value;
        SaveAndUpdate();
    }

    private void BrowseGif()
    {
        var dialog = new OpenFileDialog
        {
            Filter = "GIF files (*.gif)|*.gif|All files (*.*)|*.*",
            Title = "Select GIF"
        };

        if (dialog.ShowDialog() == true)
        {
            SetGif(dialog.FileName);
        }
    }

    private void SetGif(string path)
    {
        if (!File.Exists(path))
        {
            return;
        }

        _settings.GifPath = path;
        GifPathTextBox.Text = path;
        UpdateRecentGifs(path);
        SaveAndUpdate();
    }

    private void UpdateRecentGifs(string path)
    {
        _settings.RecentGifs.RemoveAll(item => string.Equals(item, path, StringComparison.OrdinalIgnoreCase));
        _settings.RecentGifs.Insert(0, path);
        if (_settings.RecentGifs.Count > 10)
        {
            _settings.RecentGifs.RemoveRange(10, _settings.RecentGifs.Count - 10);
        }

        RefreshRecentList();
    }

    private void RefreshRecentList()
    {
        RecentGifsListBox.ItemsSource = null;
        RecentGifsListBox.ItemsSource = _settings.RecentGifs;
    }

    private void ApplyRecentSelection()
    {
        if (RecentGifsListBox.SelectedItem is string path)
        {
            SetGif(path);
        }
    }

    private void ToggleOverlay()
    {
        if (_settings.OverlayRunning)
        {
            StopOverlay();
        }
        else
        {
            StartOverlay();
        }
    }

    private void StartOverlay()
    {
        _settings.OverlayRunning = true;
        UpdateOverlayState();
        UpdateStartStopButton();
        SaveSettings();
    }

    private void StopOverlay()
    {
        _settings.OverlayRunning = false;
        UpdateOverlayState();
        UpdateStartStopButton();
        SaveSettings();
    }

    private void UpdateOverlayState()
    {
        if (_settings.OverlayRunning)
        {
            _overlayWindow ??= new OverlayWindow();
            _overlayWindow.UpdateSettings(_settings);
            if (!_overlayWindow.IsVisible)
            {
                _overlayWindow.Show();
            }
        }
        else
        {
            if (_overlayWindow != null)
            {
                _overlayWindow.Close();
                _overlayWindow = null;
            }
        }
    }

    private void UpdateStartStopButton()
    {
        StartStopButton.Content = _settings.OverlayRunning ? "Stop Overlay" : "Start Overlay";
    }

    private void SaveAndUpdate()
    {
        SaveSettings();
        _overlayWindow?.UpdateSettings(_settings);
    }

    private void SaveSettings()
    {
        _settingsService.Save(_settings);
    }

    private void ResetDefaults()
    {
        _settings = new TreeOverlaySettings();
        EnsureDefaultGif();
        BindSettingsToControls();
        UpdateOverlayState();
        UpdateStartStopButton();
        SaveSettings();
    }

    private void EnsureDefaultGif()
    {
        if (!string.IsNullOrWhiteSpace(_settings.GifPath) && File.Exists(_settings.GifPath))
        {
            return;
        }

        var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        var fallbackPath = Path.Combine(appData, "TreeOverlay", "tree.gif");
        if (!File.Exists(fallbackPath))
        {
            DefaultGif.WriteTo(fallbackPath);
        }

        _settings.GifPath = fallbackPath;
    }

    private void SelectMonitor(string deviceName)
    {
        foreach (ComboBoxItem item in MonitorComboBox.Items)
        {
            if (item.Tag is string tag && string.Equals(tag, deviceName, StringComparison.OrdinalIgnoreCase))
            {
                MonitorComboBox.SelectedItem = item;
                return;
            }
        }

        MonitorComboBox.SelectedIndex = 0;
        _settings.MonitorDeviceName = "Primary";
    }

    private void CycleGif(int direction)
    {
        if (_settings.RecentGifs.Count == 0)
        {
            return;
        }

        var currentIndex = _settings.RecentGifs.FindIndex(path => string.Equals(path, _settings.GifPath, StringComparison.OrdinalIgnoreCase));
        if (currentIndex < 0)
        {
            currentIndex = 0;
        }

        var nextIndex = (currentIndex + direction + _settings.RecentGifs.Count) % _settings.RecentGifs.Count;
        SetGif(_settings.RecentGifs[nextIndex]);
    }

    private void ShowControlPanel()
    {
        Show();
        WindowState = WindowState.Normal;
        Activate();
    }

    private void HideControlPanel()
    {
        Hide();
    }

    private void QuitApplication()
    {
        _exitRequested = true;
        _notifyIcon?.Dispose();
        _overlayWindow?.Close();
        Application.Current.Shutdown();
    }

    private void OnClosing(object? sender, CancelEventArgs e)
    {
        if (_exitRequested)
        {
            return;
        }

        e.Cancel = true;
        HideControlPanel();
    }
}

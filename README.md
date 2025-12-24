# Tree Overlay

A Windows 10/11 native WPF app that overlays an animated GIF tree on the desktop with a control panel and tray menu.

## Build & Run

### Requirements
- Windows 10/11
- .NET SDK 8.x

### Build
```bash
# from the repo root
msbuild TreeOverlay.sln /p:Configuration=Release
```

### Run
```bash
# from the repo root
TreeOverlay\bin\Release\net8.0-windows\TreeOverlay.exe
```

## Notes
- Settings persist to `%APPDATA%\TreeOverlay\settings.json`.
- A default `tree.gif` is embedded and written to `%APPDATA%\\TreeOverlay\\tree.gif` on first run so the overlay runs immediately.

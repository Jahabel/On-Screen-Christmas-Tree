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

## Portable build (one-click share)

Use the provided publish script to generate a self-contained, portable build that runs on another Windows 10/11 PC without installing .NET.

```powershell
# from the repo root
./publish.ps1
```

Output: `publish\portable\TreeOverlay.exe`

## Installer (Inno Setup)

1. Run `publish.ps1` first to produce the portable build.
2. Open `installer.iss` in Inno Setup and build the installer.

The installer:
- Installs per-user to `%LOCALAPPDATA%\TreeOverlay` (no admin rights required).
- Adds a Start Menu shortcut.
- Offers an optional “Run on startup” task.

## Notes
- Settings persist to `%APPDATA%\TreeOverlay\settings.json`.
- A default `tree.gif` is embedded and written to `%APPDATA%\TreeOverlay\tree.gif` on first run so the overlay runs immediately.
- You can change the GIF via the Control Panel “Browse” button; recently used GIFs are shown in the list.

#define MyAppName "TreeOverlay"
#define MyAppVersion "1.0.0"
#define MyAppPublisher "Veli Yılmaz"
#define MyAppExeName "TreeOverlay.exe"

[Setup]
AppId={{7C6D77D5-8A87-4C19-92A8-0A00B6A4019A}}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
AppPublisher={#MyAppPublisher}
DefaultDirName={localappdata}\{#MyAppName}
DisableProgramGroupPage=yes
PrivilegesRequired=lowest
OutputBaseFilename=TreeOverlayInstaller
Compression=lzma
SolidCompression=yes

[Files]
Source: "publish\portable\{#MyAppExeName}"; DestDir: "{app}"; Flags: ignoreversion
Source: "publish\portable\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs

[Icons]
Name: "{userprograms}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"

[Tasks]
Name: "startup"; Description: "Run on startup"; Flags: unchecked

[Run]
Filename: "{app}\{#MyAppExeName}"; Description: "Launch {#MyAppName}"; Flags: nowait postinstall skipifsilent

[Code]
procedure CurStepChanged(CurStep: TSetupStep);
var
  StartupFolder: string;
  ShortcutPath: string;
  ShortcutContent: string;
begin
  if CurStep = ssPostInstall then
  begin
    StartupFolder := ExpandConstant('{userstartup}');
    ShortcutPath := StartupFolder + '\\TreeOverlay-startup.cmd';
    if WizardIsTaskSelected('startup') then
    begin
      ShortcutContent := '@echo off'#13#10 + '"' + ExpandConstant('{app}\{#MyAppExeName}') + '"'#13#10;
      SaveStringToFile(ShortcutPath, ShortcutContent, False);
    end
    else
    begin
      if FileExists(ShortcutPath) then
      begin
        DeleteFile(ShortcutPath);
      end;
    end;
  end;
end;

; Script de Inno Setup para LaunchDock
; Requiere Inno Setup 6+ ? https://jrsoftware.org/isinfo.php

#define MyAppName      "LaunchDock"
#define MyAppVersion   "1.4.0"
#define MyAppPublisher "Jorge Lacaci"
#define MyAppExeName   "LaunchDock.exe"

[Setup]
AppId={{A1B2C3D4-E5F6-7890-ABCD-EF1234567890}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
AppVerName={#MyAppName} {#MyAppVersion}
AppPublisher={#MyAppPublisher}
DefaultDirName={autopf}\{#MyAppName}
DefaultGroupName={#MyAppName}
AllowNoIcons=yes
OutputDir=.\publish\Installer
OutputBaseFilename=LaunchDock-Setup-{#MyAppVersion}
SetupIconFile=LaunchDock.ico
Compression=lzma2/ultra64
SolidCompression=yes
WizardStyle=modern
PrivilegesRequired=lowest
ArchitecturesAllowed=x64compatible
ArchitecturesInstallIn64BitMode=x64compatible
CloseApplications=yes
CloseApplicationsFilter=*.exe
RestartApplications=no
UninstallDisplayIcon={app}\{#MyAppExeName}
VersionInfoVersion={#MyAppVersion}
VersionInfoCompany={#MyAppPublisher}
VersionInfoDescription=Barra de lanzamiento rapido para Windows

[Languages]
Name: "spanish"; MessagesFile: "compiler:Languages\Spanish.isl"
Name: "english"; MessagesFile: "compiler:Default.isl"

[Tasks]
Name: "desktopicon";  Description: "Crear acceso directo en el Escritorio"; GroupDescription: "Iconos adicionales:"; Flags: unchecked
Name: "startupicon";  Description: "Iniciar LaunchDock con Windows";         GroupDescription: "Inicio:"

[Files]
Source: ".\publish\win-x64\{#MyAppExeName}"; DestDir: "{app}"; Flags: ignoreversion

[Icons]
Name: "{group}\{#MyAppName}";                        Filename: "{app}\{#MyAppExeName}"
Name: "{group}\Desinstalar {#MyAppName}";            Filename: "{uninstallexe}"
Name: "{autodesktop}\{#MyAppName}";                 Filename: "{app}\{#MyAppExeName}"; Tasks: desktopicon
Name: "{userstartup}\{#MyAppName}";                 Filename: "{app}\{#MyAppExeName}"; Tasks: startupicon

[Run]
Filename: "{app}\{#MyAppExeName}"; \
  Description: "Iniciar {#MyAppName} ahora"; \
  Flags: nowait postinstall skipifsilent

[UninstallRun]
; Cerrar el proceso antes de desinstalar
Filename: "taskkill.exe"; Parameters: "/F /IM {#MyAppExeName}"; RunOnceId: "KillApp"; Flags: runhidden

[UninstallDelete]
Type: filesandordirs; Name: "{userappdata}\LaunchDock"

[Code]
// Cierra LaunchDock si está corriendo antes de instalar/actualizar
procedure CloseRunningApp();
var
  ResultCode: Integer;
begin
  Exec('taskkill.exe', '/F /IM {#MyAppExeName}', '', SW_HIDE,
       ewWaitUntilTerminated, ResultCode);
  Sleep(800);
end;

function PrepareToInstall(var NeedsRestart: Boolean): String;
begin
  CloseRunningApp();
  Result := '';
end;

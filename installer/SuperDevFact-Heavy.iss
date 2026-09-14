; Installeur "version lourde" (WPF, client natif) de SUPER DEV FACT.
; Compiler avec : ISCC.exe SuperDevFact-Heavy.iss
#define MyAppName "SUPER DEV FACT"
#define MyAppVersion "1.0.0"
#define MyAppPublisher "Solaris Installation"
#define MyPublishDir "..\publish\desktop"

[Setup]
AppId={{E1C1A9C2-4B0B-4C6D-9A7C-2B5A1D8F5A11}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
AppPublisher={#MyAppPublisher}
DefaultDirName={localappdata}\Programs\SuperDevFact
DefaultGroupName={#MyAppName}
DisableProgramGroupPage=yes
PrivilegesRequired=lowest
ArchitecturesAllowed=x64compatible
ArchitecturesInstallIn64BitMode=x64compatible
OutputDir=output
OutputBaseFilename=SuperDevFact-Lourd-Setup
Compression=lzma2/max
SolidCompression=yes
SetupIconFile=assets\AppIcon.ico
WizardImageFile=assets\WizardBanner.bmp
WizardSmallImageFile=assets\WizardSmall.bmp
WizardStyle=modern
UninstallDisplayIcon={app}\SuperDevFact.Desktop.exe

[Languages]
Name: "french"; MessagesFile: "compiler:Languages\French.isl"

[Tasks]
Name: "desktopicon"; Description: "Créer une icône sur le Bureau"; GroupDescription: "Icônes supplémentaires :"

[Files]
Source: "{#MyPublishDir}\*"; DestDir: "{app}"; Flags: recursesubdirs ignoreversion

[Icons]
Name: "{group}\{#MyAppName}"; Filename: "{app}\SuperDevFact.Desktop.exe"
Name: "{group}\Désinstaller {#MyAppName}"; Filename: "{uninstallexe}"
Name: "{autodesktop}\{#MyAppName}"; Filename: "{app}\SuperDevFact.Desktop.exe"; Tasks: desktopicon

[Run]
Filename: "{app}\SuperDevFact.Desktop.exe"; Description: "Lancer {#MyAppName}"; Flags: nowait postinstall skipifsilent

; FusionOS desktop installer — packages the self-contained API host (which also
; serves the built SPA from wwwroot/, see Program.cs's servesSpa block) as a
; single Windows installer. Run scripts\build-windows-installer.ps1 to publish
; the app and frontend first, then compile this script with ISCC.exe.

#define MyAppName "FusionOS"
#define MyAppVersion "1.0.0"
#define MyAppPublisher "FusionOS"
#define MyAppURL "http://localhost:5000"
#define MyAppExeName "FusionOS.Api.Host.exe"

[Setup]
AppId={{8F2C6C7E-7E7A-4B7C-9E77-FUSIONOS0001}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
AppPublisher={#MyAppPublisher}
AppPublisherURL={#MyAppURL}
DefaultDirName={autopf}\{#MyAppName}
DefaultGroupName={#MyAppName}
DisableProgramGroupPage=yes
OutputDir=Output
OutputBaseFilename=FusionOS-Setup
Compression=lzma2
SolidCompression=yes
ArchitecturesAllowed=x64compatible
ArchitecturesInstallIn64BitMode=x64compatible
PrivilegesRequired=admin
WizardStyle=modern
UninstallDisplayIcon={app}\{#MyAppExeName}

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"

[Files]
Source: "..\build\publish\api\*"; DestDir: "{app}"; Flags: recursesubdirs ignoreversion
Source: "launcher.bat"; DestDir: "{app}"; Flags: ignoreversion
Source: "stop-fusionos.bat"; DestDir: "{app}"; Flags: ignoreversion

[Icons]
Name: "{group}\Start FusionOS"; Filename: "{app}\launcher.bat"; WorkingDir: "{app}"
Name: "{group}\Stop FusionOS"; Filename: "{app}\stop-fusionos.bat"; WorkingDir: "{app}"
Name: "{group}\Uninstall FusionOS"; Filename: "{uninstallexe}"
Name: "{autodesktop}\FusionOS"; Filename: "{app}\launcher.bat"; WorkingDir: "{app}"; Tasks: desktopicon

[Tasks]
Name: "desktopicon"; Description: "Create a &desktop shortcut"; GroupDescription: "Additional shortcuts:"

[Run]
Filename: "{app}\launcher.bat"; Description: "Start FusionOS now"; Flags: nowait postinstall skipifsilent

[UninstallRun]
Filename: "{cmd}"; Parameters: "/c taskkill /IM {#MyAppExeName} /F"; Flags: runhidden skipifdoesntexist

[Code]
var
  DbPage: TInputQueryWizardPage;

function RandomKeyChar(): Char;
var
  chars: String;
begin
  chars := 'ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789';
  Result := chars[Random(Length(chars)) + 1];
end;

function GenerateSigningKey(): String;
var
  i: Integer;
  s: String;
begin
  s := '';
  for i := 1 to 48 do
    s := s + RandomKeyChar();
  Result := s;
end;

procedure InitializeWizard;
begin
  DbPage := CreateInputQueryPage(wpSelectDir,
    'Database connection', 'Configure the PostgreSQL connection FusionOS will use',
    'FusionOS needs an existing PostgreSQL server. Enter its connection details below ' +
    '(the defaults match a local install with the standard fusionos/fusionos setup). ' +
    'You can change this later by editing appsettings.Production.json in the install folder.');
  DbPage.Add('Host:', False);
  DbPage.Add('Port:', False);
  DbPage.Add('Database name:', False);
  DbPage.Add('Username:', False);
  DbPage.Add('Password:', True);

  DbPage.Values[0] := 'localhost';
  DbPage.Values[1] := '5432';
  DbPage.Values[2] := 'fusionos';
  DbPage.Values[3] := 'fusionos';
  DbPage.Values[4] := 'fusionos';
end;

function NextButtonClick(CurPageID: Integer): Boolean;
begin
  Result := True;
  if CurPageID = DbPage.ID then
  begin
    if (DbPage.Values[0] = '') or (DbPage.Values[2] = '') or (DbPage.Values[3] = '') then
    begin
      MsgBox('Host, database name, and username are required.', mbError, MB_OK);
      Result := False;
    end;
  end;
end;

procedure CurStepChanged(CurStep: TSetupStep);
var
  connString, signingKey, json: String;
  settingsPath: String;
begin
  if CurStep = ssPostInstall then
  begin
    connString := 'Host=' + DbPage.Values[0] + ';Port=' + DbPage.Values[1] +
      ';Database=' + DbPage.Values[2] + ';Username=' + DbPage.Values[3] +
      ';Password=' + DbPage.Values[4];
    signingKey := GenerateSigningKey();

    json := '{' + #13#10 +
      '  "Deployment": { "AutoMigrate": true },' + #13#10 +
      '  "ConnectionStrings": { "Postgres": "' + connString + '" },' + #13#10 +
      '  "Jwt": { "Issuer": "fusionos", "Audience": "fusionos-clients", "SigningKey": "' + signingKey + '" }' + #13#10 +
      '}';

    settingsPath := ExpandConstant('{app}\appsettings.Production.json');
    SaveStringToFile(settingsPath, json, False);
  end;
end;

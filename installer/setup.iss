[Files]
Source: "bin\OxyPlot.dll"; DestDir: "{app}"
Source: "bin\OxyPlot.WindowsForms.dll"; DestDir: "{app}"
Source: "bin\Aga.Controls.dll"; DestDir: "{app}"
Source: "bin\OpenHardwareMonitor.exe"; DestDir: "{app}"; Permissions: admins-full
Source: "bin\OpenHardwareMonitor.exe.config"; DestDir: "{app}"
Source: "bin\OpenHardwareMonitorLib.dll"; DestDir: "{app}"
Source: "bin\License.html"; DestDir: "{app}"
Source: "BootTask\OHWBootTask\bin\Release\autostart.exe"; DestDir: "{app}"
Source: "BootTask\OHWBootTask\bin\Release\Microsoft.Win32.TaskScheduler.dll"; DestDir: "{app}"

[Run]
Filename: "{app}\autostart.exe"; Parameters: "enable ""{app}"""; WorkingDir: "{app}"; Flags: postinstall waituntilterminated runascurrentuser runhidden unchecked; Description: "Automatically start Open Hardware Monitor at logon"; StatusMsg: "Adding boot task"
Filename: "{app}\OpenHardwareMonitor.exe"; WorkingDir: "{app}"; Flags: runascurrentuser unchecked postinstall; Description: "Start Open Hardware Monitor"

[UninstallRun]
Filename: "{app}\autostart.exe"; Parameters: "disable"; Flags: waituntilterminated runhidden runascurrentuser

[Setup]
AppName=Open Hardware Monitor
AppVersion=0.8.0 beta
AppCopyright=2010-2018 Michael Möller
LicenseFile=license.rtf
Compression=lzma2/ultra64
InternalCompressLevel=ultra64
DefaultDirName={pf}\OpenHardwareMonitor
AppPublisher=Michael Möller
UninstallDisplayName=Open Hardware Monitor
UninstallDisplayIcon={app}\OpenHardwareMonitor.exe,0
AllowNoIcons=True
AppendDefaultGroupName=False
VersionInfoCompany=Michael Möller
VersionInfoCopyright=2010-2018
VersionInfoProductName=Open Hardware Monitor
VersionInfoProductTextVersion=0.8.0 beta
VersionInfoTextVersion=0.8.0 beta
VersionInfoVersion=0.8.0.0
OutputBaseFilename=OHMSetup

[Icons]
Name: "{userdesktop}\Open Hardware Monitor"; Filename: "{app}\OpenHardwareMonitor.exe"; WorkingDir: "{app}"; IconFilename: "{app}\OpenHardwareMonitor.exe"; IconIndex: 0; Tasks: desktopicon

[Tasks]
Name: "desktopicon"; Description: "Add Desktop Icon"; Flags: checkablealone

[UninstallDelete]
Type: files; Name: "{app}\OpenHardwareMonitor.config"
Type: filesandordirs; Name: "{app}"

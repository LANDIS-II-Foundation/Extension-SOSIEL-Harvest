; LANDIS-II Extension infomation
#define CoreRelease "LANDIS-II-V8"
#define ExtensionName "SOSIEL Harvest"
#define AppVersion "2.0"
#define AppPublisher "LANDIS-II Foundation"
#define AppURL "http://www.landis-ii.org/"

; Build directory
#define BuildDir "..\..\src\bin\Debug\netstandard2.0"

; LANDIS-II installation directories
#define ExtDir "C:\Program Files\LANDIS-II-v8\extensions"
#define AppDir "C:\Program Files\LANDIS-II-v8"
#define LandisPlugInDir "C:\Program Files\LANDIS-II-v8\plug-ins-installer-files"
#define ExtensionsCmd AppDir + "\commands\landis-ii-extensions.cmd"

[Setup]
; NOTE: The value of AppId uniquely identifies this application.
; Do not use the same AppId value in installers for other applications.
; (To generate a new GUID, click Tools | Generate GUID inside the IDE.)
AppId={{2B1775EF-F6C4-471A-AFED-DF11402EEBBA}
AppName={#CoreRelease} {#ExtensionName}
AppVersion={#AppVersion}
; Name in "Programs and Features"
AppVerName={#CoreRelease} {#ExtensionName} v{#AppVersion}
AppPublisher={#AppPublisher}
AppPublisherURL={#AppURL}
AppSupportURL={#AppURL}
AppUpdatesURL={#AppURL}
DefaultDirName={commonpf}\{#ExtensionName}
DisableDirPage=yes
DefaultGroupName={#ExtensionName}
DisableProgramGroupPage=yes
LicenseFile=SHE LGPL.txt
OutputDir={#SourcePath}
OutputBaseFilename={#CoreRelease} {#ExtensionName} {#AppVersion}-setup
Compression=lzma
SolidCompression=yes

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"


[Files]
; This .dll IS the extension (ie, the extension's assembly)
; NB: Do not put an additional version number in the file name of this .dll
; (The name of this .dll is defined in the extension's \src\*.csproj file)
Source: {#BuildDir}\Landis.Extension.SOSIELHarvest-v2.dll; DestDir: {#ExtDir}; Flags: ignoreversion
Source: {#BuildDir}\SOSIEL-v3.dll; DestDir: {#ExtDir}; Flags: ignoreversion
Source: {#BuildDir}\SOSIEL.VBGP-v1.dll; DestDir: {#ExtDir}; Flags: ignoreversion


; Requisite auxiliary libraries
; NB. These libraries are used by other extensions and thus are never uninstalled.
Source: {#BuildDir}\Landis.Extension.BiomassHarvest-v6.dll; DestDir: {#ExtDir}; Flags: uninsneveruninstall replacesameversion
Source: {#BuildDir}\Landis.Library.BiomassHarvest-v3.dll; DestDir: {#ExtDir}; Flags: uninsneveruninstall replacesameversion
Source: {#BuildDir}\Landis.Library.HarvestManagement-v4.dll; DestDir: {#ExtDir}; Flags: uninsneveruninstall replacesameversion
Source: {#BuildDir}\Landis.Library.SiteHarvest-v2.dll; DestDir: {#ExtDir}; Flags: uninsneveruninstall replacesameversion
Source: {#BuildDir}\CsvHelper.dll; DestDir: {#ExtDir}; Flags: replacesameversion
Source: {#BuildDir}\Newtonsoft.Json.dll; DestDir: {#ExtDir}; Flags: replacesameversion
Source: {#BuildDir}\NLog.dll; DestDir: {#ExtDir}; Flags: replacesameversion
Source: {#BuildDir}\Microsoft.CSharp.dll; DestDir: {#ExtDir}; Flags: replacesameversion

; LANDIS-II identifies the extension with the info in this .txt file
; NB. New releases must modify the name of this file and the info in it
#define InfoTxt "SHE 2.txt"
Source: {#InfoTxt}; DestDir: {#LandisPlugInDir}
; NOTE: Don't use "Flags: ignoreversion" on any shared system files


[Run]
Filename: {#ExtensionsCmd}; Parameters: "remove ""SOSIEL Harvest"" "; WorkingDir: {#LandisPlugInDir}
Filename: {#ExtensionsCmd}; Parameters: "add ""{#InfoTxt}"" "; WorkingDir: {#LandisPlugInDir} 


[UninstallRun]
Filename: {#ExtensionsCmd}; Parameters: "remove ""SOSIEL Harvest"" "; WorkingDir: {#LandisPlugInDir}



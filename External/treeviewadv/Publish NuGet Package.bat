:: search for msbuild make it available
set msbuildPath=%ProgramFiles(x86)%\Microsoft Visual Studio\2019\Community\MSBuild\Current\Bin\
set PATH=%PATH%;%msbuildPath%

:: make sure we have a clean release build
msbuild /t:Rebuild /p:Configuration=Release /p:Platform="Any CPU" TreeViewAdv.sln

:: remove any existing nupkg files
del *.nupkg

:: build the nuget packages
.nuget\nuget pack Aga.Controls\Aga.Controls.csproj -Properties Configuration=Release

:: upload the nuget packages
.nuget\nuget push *.nupkg -Source "https://www.nuget.org"

:: remove nupkg files after uploading them
del *.nupkg

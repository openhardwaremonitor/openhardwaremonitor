$sourceUri = "https://openlibsys.org/download/VirtualChecker3.zip"
$sourceFile = "VirtualChecker3.zip"
$sourceDir = "VirtualChecker3"

Invoke-WebRequest -Uri $sourceUri -OutFile $sourceFile
Expand-Archive -Path $sourceFile -DestinationPath $sourceDir
Move-Item $sourceDir/WinRing0.sys . -Force
Move-Item $sourceDir/WinRing0x64.sys . -Force
Remove-Item $sourceDir -Recurse
Remove-Item $sourceFile

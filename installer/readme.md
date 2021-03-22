## Step 1
#### Install prerequisites
- [Inno Setup](http://www.jrsoftware.org/isdl.php) - Inno Setup Studio _highly_ recommended
- [Visual Studio](https://visualstudio.microsoft.com/downloads/)
## Step 2
- Build BootTask project
- Copy Open Hardware Monitor's required files to `installer/bin/`, as listed below:
  - OpenHardwareMonitor.exe
  - OpenHardwareMonitorLib.dll
  - OxyPlot.dll
  - OxyPlot.WindowsForms.dll
  - Aga.Controls.dll
  - OpenHardwareMonitor.exe.config
  - License.html
- Run the inno setup compiler
  - If using Inno Setup Studio, press `Ctrl-F9` to compile

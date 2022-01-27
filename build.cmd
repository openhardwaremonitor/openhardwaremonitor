@echo off

dotnet build --configuration debug OpenHardwareMonitor.sln
dotnet build --configuration release OpenHardwareMonitor.sln

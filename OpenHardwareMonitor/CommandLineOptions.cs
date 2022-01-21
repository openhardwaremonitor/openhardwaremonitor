using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using CommandLine;

namespace OpenHardwareMonitor {
  class CommandLineOptions {
    [Option("IgnoreMonitorCPU", HelpText = "Disable monitoring of CPU")]
    public bool IgnoreMonitorCPU { get; set; }

    [Option("IgnoreMonitorFanController", HelpText = "Disable monitoring of fan controller")]
    public bool IgnoreMonitorFanController { get; set; }

    [Option("IgnoreMonitorGPU", HelpText = "Disable monitoring of GPU")]
    public bool IgnoreMonitorGPU { get; set; }

    [Option("IgnoreMonitorHDD", HelpText = "Disable monitoring of HDDs/SSDs")]
    public bool IgnoreMonitorHDD { get; set; }

    [Option("IgnoreMonitorRemovableDisks", HelpText = "Ignore monitoring of removable disks")]
    public bool IgnoreRemovableDrives { get; set; }

    [Option("IgnoreMonitorMainboard", HelpText = "Disable monitoring of mainboard")]
    public bool IgnoreMonitorMainboard { get; set; }

    [Option("IgnoreMonitorRAM", HelpText = "Disable monitoring of RAM")]
    public bool IgnoreMonitorRAM { get; set; }

    [Option("IgnoreMonitorNetwork", HelpText = "Disable network monitoring")]
    public bool IgnoreMonitorNetwork { get; set; }

    [Option('m', "startminimized", HelpText = "Force minimized start")]
    public bool StartMinimized { get; set; }

    [Option("startnormal", HelpText = "Starts in normal mode (not minimized). This option is not persistent and is used to temporarily override the current configuration setting")]
    public bool StartNormal { get; set; }

    [Option("minimizetotray", HelpText = "Force minimize to tray")]
    public bool MinimizeToTray { get; set; }

    [Option('p', "port", HelpText = "Network port to use for remote web access (default: 8086)")]
    public int? WebServerPort { get; set; }

    [Option('r', "run", HelpText = "Run webserver at startup")]
    public bool RunWebServer { get; set; }

    [Option("remote", HelpText = "Allow remote access to web server (if disabled, only localhost is allowed)", Default = false)]
    public bool AllowRemoteAccess { get; set; }

    [Option("autostartupmode", HelpText = "Set the auto startup mode. Valid values are 'Disabled', 'Logon', and 'Bootup'")]
    public string AutoStartupModeInternal { get; set; }

    [Option("ignoreconfiguration", HelpText = "Do not load configuration file at startup. If this is not specified, the other settings still supersede the " +
                                              "settings from configuration, but default to the previous setting instead of the default")]
    public bool DoNotLoadConfiguration { get; set; }

    [Option("CloseAll", HelpText = "Close or kill all instances and exit. This can be used to ensure the application no longer runs or to configure default settings without launching.")]
    public bool CloseAll { get; set; }

    public AutoStartupMode AutoStartupMode
    {
        get
        {
            if (Enum.TryParse<AutoStartupMode>(AutoStartupModeInternal, true, out var result))
            {
                return result;
            }

            return AutoStartupMode.NoChange;
        }
    }
  }
}

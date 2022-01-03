/*
 
  This Source Code Form is subject to the terms of the Mozilla Public
  License, v. 2.0. If a copy of the MPL was not distributed with this
  file, You can obtain one at http://mozilla.org/MPL/2.0/.
 
  Copyright (C) 2018 Jochen Wezel <jwezel@compumaster.de>
	
*/

using CommandLine;

namespace OpenHardwareMonitorReport
{
    public class CommandLineOptions
    {

        public class OptionsBase
        {
            [Option('t', "TemperatureUnit", HelpText = "Temperature values in Fahrenheit oder Celsius (defaults to Celsius)")]
            public OpenHardwareMonitor.GUI.TemperatureUnit TemperatureUnit { get; set; }

            [Option('v', "Verbose", HelpText = "Verbose mode")]
            public bool VerboseMode { get; set; }

            [Option("WaitOnExitForSeconds", HelpText = "On application exit, wait for X seconds")]
            public int WaitOnExitForSeconds { get; set; }

            [Option("WaitOnExitForEnterKey", HelpText = "On application exit, wait for <Enter> key")]
            public bool WaitOnExitForEnterKey { get; set; }

            [Option("LogFile", HelpText = "Log messages and errors to a file")]
            public string LogFile { get; set; }

            [Option("IgnoreMonitorCPU", HelpText = "Disable monitoring of CPU")]
            public bool IgnoreMonitorCPU { get; set; }

            [Option("IgnoreMonitorFanController", HelpText = "Disable monitoring of fan controller")]
            public bool IgnoreMonitorFanController { get; set; }

            [Option("IgnoreMonitorGPU", HelpText = "Disable monitoring of GPU")]
            public bool IgnoreMonitorGPU { get; set; }

            [Option("IgnoreMonitorHDD", HelpText = "Disable monitoring of HDDs/SSDs")]
            public bool IgnoreMonitorHDD { get; set; }

            [Option("IgnoreMonitorMainboard", HelpText = "Disable monitoring of mainboard")]
            public bool IgnoreMonitorMainboard { get; set; }

            [Option("IgnoreMonitorRAM", HelpText = "Disable monitoring of RAM")]
            public bool IgnoreMonitorRAM { get; set; }

            [Option("IgnoreMonitorNetwork", HelpText = "Disable network monitoring")]
            public bool IgnoreMonitorNetwork { get; set; }
        }

        [Verb("RunWebserver", HelpText = "Run a webserver with REST api")]
        public class RunWebserver : OptionsBase
        {
            [Option('p', "port", Default = 8086, HelpText = "TCP port for the webserver (defaults to 8086)")]
            public int Port { get; set; }

            [Option('i', "interval", Default = 1000, HelpText = "The refresh interval for all data in ms (defaults to 1000 ms)")]
            public int Interval { get; set; }
        }

        [Verb("ReportToConsole", HelpText = "Report to the console")]
        public class ReportToConsole : OptionsBase
        {
        }

        [Verb("ReportToFile", HelpText = "Report to a file")]
        public class ReportToFile : OptionsBase
        {
            [Option('f', "File", Required = true, HelpText = "File path")]
            public string FilePath { get; set; }
        }

    }
}

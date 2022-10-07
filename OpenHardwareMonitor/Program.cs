/*
 
  This Source Code Form is subject to the terms of the Mozilla Public
  License, v. 2.0. If a copy of the MPL was not distributed with this
  file, You can obtain one at http://mozilla.org/MPL/2.0/.
 
  Copyright (C) 2009-2020 Michael Möller <mmoeller@openhardwaremonitor.org>
	
*/

using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using CommandLine;
using Microsoft.Extensions.Logging;
using OpenHardwareMonitor.GUI;
using OpenHardwareMonitor.Utilities;
using OpenHardwareMonitorLib;

namespace OpenHardwareMonitor
{
    public static class Program
    {
        internal static CommandLineOptions Arguments;
        [STAThread]
        public static void Main(string[] args)
        {
            if (!AllRequiredFilesAvailable())
                return;

            if (CheckIfProcessExists())
                return;

            InstallAndConfigureNlog();

            if (!ParseCommandLine(args))
            {
                return;
            }
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            using (GUI.MainForm form = new GUI.MainForm())
            {
                form.FormClosed += delegate (Object sender, FormClosedEventArgs e)
                {
                    Application.Exit();
                };
                if (!Arguments.CloseAll)
                {
                    Application.Run();
                }
                else
                {
                    form.ForceClose();
                }
            }
        }

        private static void InstallAndConfigureNlog()
        {
            Logging.LoggerFactory = new ToNLogLoggerFactory();
            Logging.LogInfo("Starting new session");
        }

        /// <summary>
        /// Checks if there is already an instance of Openhardwaremonitor running and brings up its window
        /// in case its minimized or as icon in taskbar
        /// </summary>
        private static bool CheckIfProcessExists()
        {
            bool processExists = false;
            Process thisInstance = Process.GetCurrentProcess();
            if(Process.GetProcessesByName(thisInstance.ProcessName).Length > 1)
            {
                processExists = true;
                using (var clientPipe = InterprocessCommunicationFactory.GetClientPipe())
                {
                    clientPipe.Connect();
                    clientPipe.Write(new byte[] { (byte)SecondInstanceService.SecondInstanceRequest.MaximizeWindow }, 0, 1);
                } 
            }
            return processExists;
        }

        private static bool ParseCommandLine(string[] args)
        {
            StringBuilder helpText = new StringBuilder();
            TextWriter helpWriter = new StringWriter(helpText);
            CommandLine.Parser caseInsensitiveParser = new CommandLine.Parser(settings =>
            {
                settings.CaseSensitive = false;
                settings.HelpWriter = helpWriter;
                settings.EnableDashDash = CommandLine.Parser.Default.Settings.EnableDashDash;
                settings.CaseInsensitiveEnumValues = CommandLine.Parser.Default.Settings.CaseInsensitiveEnumValues;
                settings.IgnoreUnknownArguments = CommandLine.Parser.Default.Settings.IgnoreUnknownArguments;
                settings.MaximumDisplayWidth = CommandLine.Parser.Default.Settings.MaximumDisplayWidth;
                settings.ParsingCulture = CommandLine.Parser.Default.Settings.ParsingCulture;
            });

            var result = caseInsensitiveParser.ParseArguments<CommandLineOptions>(args);
            Arguments = new CommandLineOptions(); // ensure it is not null, even if the command line parsing fails
            result.WithParsed(x => Arguments = x);

            // This writer is not empty if the (implicit) --help option was specified.
            if (helpText.Length > 0)
            {
                helpWriter.WriteLine("Unknown options are silently ignored");
                MessageBox.Show(helpWriter.ToString(), "Command line options", MessageBoxButtons.OK);

                return false;
            }

            return true;
        }

        private static bool IsFileAvailable(string fileName)
        {
            string path = Path.GetDirectoryName(Application.ExecutablePath) +
              Path.DirectorySeparatorChar;

            if (!File.Exists(path + fileName))
            {
                MessageBox.Show("The following file could not be found: " + fileName +
                  "\nPlease extract all files from the archive.", "Error",
                   MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            return true;
        }

        private static bool AllRequiredFilesAvailable()
        {
            if (!IsFileAvailable("Aga.Controls.dll"))
                return false;
            if (!IsFileAvailable("OpenHardwareMonitorLib.dll"))
                return false;
            if (!IsFileAvailable("OxyPlot.dll"))
                return false;
            if (!IsFileAvailable("OxyPlot.WindowsForms.dll"))
                return false;

            return true;
        }

        private sealed class ToNLogLoggerFactory : ILoggerFactory
        {
            public void Dispose()
            {
            }

            public ILogger CreateLogger(string categoryName)
            {
                return new NLog.Extensions.Logging.NLogLoggerFactory().CreateLogger(categoryName);
            }

            public void AddProvider(ILoggerProvider provider) => throw new NotImplementedException();
        }
    }
}

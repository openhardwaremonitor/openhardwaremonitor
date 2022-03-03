/*
 
  This Source Code Form is subject to the terms of the Mozilla Public
  License, v. 2.0. If a copy of the MPL was not distributed with this
  file, You can obtain one at http://mozilla.org/MPL/2.0/.
 
  Copyright (C) 2017 - 2018 Michael Möller <mmoeller@openhardwaremonitor.org>, Alexandr Zarubkin <me21@yandex.ru> and Jochen Wezel <jwezel@compumaster.de>
	
*/

using System;
using OpenHardwareMonitor.Hardware;
using CommandLine;

namespace OpenHardwareMonitorReport
{

    class Program
    {
        static private ComputerHardware computerHardware = new ComputerHardware();

        /// <summary>
        /// Run the OpenHardwareMonitor application as a console application
        /// </summary>
        /// <param name="args"></param>
        /// <returns>0 in case of success, 1 if arguments mismatch, 2 if required application files are missing, 3 in case of unexpected errors, 9 if administrative privileges are missing, or higher values for task specific errors</returns>
        static int Main(string[] args)
        {
            int ExitCode = 0;
            try
            {
                if (!AllRequiredFilesAvailable())
                    return 2;

                CommandLine.Parser CaseInsensitiveParser = new CommandLine.Parser(settings =>
                {
                    settings.CaseSensitive = false;
                    settings.HelpWriter = CommandLine.Parser.Default.Settings.HelpWriter;
                    settings.EnableDashDash = CommandLine.Parser.Default.Settings.EnableDashDash;
                    settings.CaseInsensitiveEnumValues = CommandLine.Parser.Default.Settings.CaseInsensitiveEnumValues;
                    settings.IgnoreUnknownArguments = CommandLine.Parser.Default.Settings.IgnoreUnknownArguments;
                    settings.MaximumDisplayWidth = CommandLine.Parser.Default.Settings.MaximumDisplayWidth;
                    settings.ParsingCulture = CommandLine.Parser.Default.Settings.ParsingCulture;
                });

                var Result = CaseInsensitiveParser.ParseArguments<CommandLineOptions.RunWebserver, CommandLineOptions.ReportToConsole, CommandLineOptions.ReportToFile>(args);
                ExitCode = Result.MapResult(
                      (CommandLineOptions.RunWebserver opts) => RunConsoleHttpServerAndReturnExitCode(opts),
                      (CommandLineOptions.ReportToConsole opts) => RunConsoleReportAndReturnExitCode(opts),
                      (CommandLineOptions.ReportToFile opts) => RunFileReportAndReturnExitCode(opts),
                      errs => 1);
            }
            catch (Exception ex)
            {
                if (VerboseMode)
                {
                    Utility.WriteLogMessage("ERROR: " + ex.ToString(), null, true);
                }
                else
                {
                    Utility.WriteLogMessage("ERROR: " + ex.Message, null, true);
                }
                ExitCode = 3;
            }
            if ((ExitCode == 1) && (!Utility.IsUserAdministrator()))
            {
                System.Console.Out.WriteLine("WARNING: you need to run this application with administrator permission");
                System.Console.Out.WriteLine();
            }


            if (WaitOnExitForSeconds > 0)
            {
                Console.Out.WriteLine("Waiting " + WaitOnExitForSeconds.ToString() + " seconds before exiting . . .");
                System.Threading.Thread.Sleep(WaitOnExitForSeconds * 1000);
            }
            if (WaitOnExitForEnterKey)
            {
                Console.Out.WriteLine("Press <Enter> to exit . . .");
                Console.ReadLine();
            }
            return ExitCode;
        }

        static private bool VerboseMode = false;
        static private bool WaitOnExitForEnterKey;
        static private int WaitOnExitForSeconds;
        static private int InitBaseOptionsAndEnforceAdminPrivileges(CommandLineOptions.OptionsBase options)
        {
            WaitOnExitForSeconds = options.WaitOnExitForSeconds;
            WaitOnExitForEnterKey = options.WaitOnExitForEnterKey;
            VerboseMode = options.VerboseMode;

            //Console.Out.WriteLine("Time to attach the debugger . . . ");
            //Console.ReadLine();

            //prepare log file

            if (!string.IsNullOrEmpty(options.LogFile) && System.IO.File.Exists(options.LogFile))
            {
                Utility.WriteLogMessage("--------------------------------------------------------------------------------------------", options, false);
            }
            if (!string.IsNullOrEmpty(options.LogFile))
            {
                Utility.WriteLogMessage("Application started on " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), options, false);
            }
            if (!Utility.IsUserAdministrator())
            {
                Utility.WriteLogMessage("Application requires administration privileges, restart of process as administrator required", options, false);
            }

            int ExitCodeOfAdminMode = Utility.EnforceAppIsRunAsAdmin();
            if (ExitCodeOfAdminMode == -2)
            {
                Utility.WriteLogMessage("ERROR: Administrator permission required (" + ExitCodeOfAdminMode.ToString() + ")", options, true);
                return 9;
            }
            else if (ExitCodeOfAdminMode > 0)
            {
                //code already executed successfully in separate application process with successfully requested administration privileges
                return ExitCodeOfAdminMode;
            }
            else if (ExitCodeOfAdminMode == -1)
            {
                //regular code execution should take place in following steps - we are running with adminitration privileges
                if (!string.IsNullOrEmpty(options.LogFile))
                {
                    Utility.WriteLogMessage("Application started with administration privileges", options, false);
                }
                return ExitCodeOfAdminMode;
            }
            else if (ExitCodeOfAdminMode == 0)
            {
                //Succcess! :-)
                return ExitCodeOfAdminMode;
            }
            else
            {
                //window close button or system terminates (e.g. logoff event) can cause negative exit codes - just log it as warning
                Utility.WriteLogMessage("WARNING: exit code of process with administration privileges was " + ExitCodeOfAdminMode.ToString() + ", possibly caused by a close event by system or user", null, false);
                return ExitCodeOfAdminMode;
            }
        }

        #region Ensure required assemblies
        private static bool IsFileAvailable(string fileName)
        {
            string path = AppDomain.CurrentDomain.BaseDirectory;

            if (!System.IO.File.Exists(System.IO.Path.Combine(path, fileName)))
            {
                Utility.WriteLogMessage("ERROR: The following file could not be found: " + fileName, null, true);
                Utility.WriteLogMessage("       Please extract all files from the archive.", null, true);
                return false;
            }
            return true;
        }

        private static bool AllRequiredFilesAvailable()
        {
            if (!IsFileAvailable("OpenHardwareMonitor.exe"))
                return false;
            if (!IsFileAvailable("Aga.Controls.dll"))
                return false;
            if (!IsFileAvailable("CommandLine.dll"))
                return false;
            if (!IsFileAvailable("OpenHardwareMonitorLib.dll"))
                return false;
            if (!IsFileAvailable("OxyPlot.dll"))
                return false;
            if (!IsFileAvailable("OxyPlot.WindowsForms.dll"))
                return false;

            return true;
        }
        #endregion

        static public int RunFileReportAndReturnExitCode(CommandLineOptions.ReportToFile options)
        {
            int EnforcedExitCode = InitBaseOptionsAndEnforceAdminPrivileges(options);
            if (EnforcedExitCode != -1) { return EnforcedExitCode; }
            try
            {
                System.IO.FileInfo OutputFile = new System.IO.FileInfo(options.FilePath);
                System.IO.StreamWriter sw = OutputFile.CreateText();
                try
                {
                    sw.Write(GetPlainTextReport(options));
                    sw.Flush();
                    sw.Close();
                    return 0;
                }
                catch (Exception ex)
                {
                    Utility.WriteLogMessage(ex, options);
                    return 21;
                }
            }
            catch (Exception ex)
            {
                Utility.WriteLogMessage(ex, options);
                return 20;
            }
        }

        static string GetPlainTextReport(CommandLineOptions.OptionsBase options)
        {
            Computer computer = computerHardware.ComputerDiagnostics(options);
            string Result = computer.GetReport();
            computer.Close();
            return Result;
        }

        static public int RunConsoleReportAndReturnExitCode(CommandLineOptions.ReportToConsole options)
        {
            int EnforcedExitCode = InitBaseOptionsAndEnforceAdminPrivileges(options);
            if (EnforcedExitCode != -1) { return EnforcedExitCode; }
            try
            {
                Console.Out.Write(GetPlainTextReport(options));
                return 0;
            }
            catch (Exception ex)
            {
                Utility.WriteLogMessage(ex, options);
                return 30;
            }
        }

        private static void RunConsoleHttpServerTimer_Elapsed(Object source, System.Timers.ElapsedEventArgs e)
        {
            computerHardware.RefreshData();
        }

        static private System.Timers.Timer RunConsoleHttpServerTimer;
        static public int RunConsoleHttpServerAndReturnExitCode(CommandLineOptions.RunWebserver options)
        {
            int EnforcedExitCode = InitBaseOptionsAndEnforceAdminPrivileges(options);
            if (EnforcedExitCode != -1) { return EnforcedExitCode; }
            try
            {
                //currently running with administrative privileges
                Computer computer = computerHardware.ComputerDiagnostics(options);

                OpenHardwareMonitor.Utilities.GrapevineServer server = new OpenHardwareMonitor.Utilities.GrapevineServer(computerHardware.root, computer, options.Port, true);
                if (server.PlatformNotSupported)
                {
                    Utility.WriteLogMessage("ERROR: Platform not supported", options, true);
                    return 11;
                }

                if (server.Start())
                {
                    // enable refresh timer
                    RunConsoleHttpServerTimer = new System.Timers.Timer(options.Interval);
                    RunConsoleHttpServerTimer.Elapsed += RunConsoleHttpServerTimer_Elapsed;
                    RunConsoleHttpServerTimer.AutoReset = true;
                    RunConsoleHttpServerTimer.Enabled = true;

                    // output connection details to console and logfile
                    Utility.WriteLogMessage("HTTP webserver started at port " + options.Port.ToString(), options, false, true);
                    Utility.WriteLogMessage("It is available at these addresses:", options, false, true);
                    foreach (System.Net.IPAddress ip in Utility.LocalIPAddresses())
                    {
                        Utility.WriteLogMessage("- http://" + ip.ToString() + ":" + options.Port.ToString() + "/", options, false, true);
                    }

                    // wait for user to quit the application
                    System.Console.Out.WriteLine("Press <Enter> to stop the webserver . . .");
                    System.Console.ReadLine();

                    // shutdown the webserver
                    RunConsoleHttpServerTimer.Stop();
                    RunConsoleHttpServerTimer.Dispose();
                    server.Stop();

                    return 0;
                }
                else
                {
                    Utility.WriteLogMessage("ERROR: Failed to start HTTP webserver", options, true);
                    // Utility.WriteLogMessage(server.StartHttpListenerException.ToString(), options, true);
                    return 12;
                }
            }
            catch (Exception ex)
            {
                Utility.WriteLogMessage(ex, options);
                return 10;
            }
        }

    }

}

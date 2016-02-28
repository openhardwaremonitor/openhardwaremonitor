/*
 
  This Source Code Form is subject to the terms of the Mozilla Public
  License, v. 2.0. If a copy of the MPL was not distributed with this
  file, You can obtain one at http://mozilla.org/MPL/2.0/.
 
  Copyright (C) 2009-2013 Michael Möller <mmoeller@openhardwaremonitor.org>
	
*/

using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Windows.Forms;
using OpenHardwareMonitor.GUI;

namespace OpenHardwareMonitor
{
    public static class Program
    {
        [STAThread]
        public static void Main()
        {
#if !DEBUG
        Application.ThreadException += 
          new ThreadExceptionEventHandler(Application_ThreadException);
        Application.SetUnhandledExceptionMode(
          UnhandledExceptionMode.CatchException);

        AppDomain.CurrentDomain.UnhandledException += 
          new UnhandledExceptionEventHandler(CurrentDomain_UnhandledException);
      #endif

            if (!AllRequiredFilesAvailable())
                Environment.Exit(0);

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            using (var form = new MainForm())
            {
                form.FormClosed += delegate { Application.Exit(); };
                Application.Run();
            }
        }

        private static bool IsFileAvailable(string fileName)
        {
            var path = Path.GetDirectoryName(Application.ExecutablePath) +
                       Path.DirectorySeparatorChar;

            if (File.Exists(path + fileName)) return true;
            MessageBox.Show("The following file could not be found: " + fileName +
                            "\nPlease extract all files from the archive.", "Error",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
            return false;
        }

        private static bool AllRequiredFilesAvailable()
        {
            return IsFileAvailable("Aga.Controls.dll") && (IsFileAvailable("OpenHardwareMonitorLib.dll") &&
                                                           (IsFileAvailable("OxyPlot.dll") &&
                                                            IsFileAvailable("OxyPlot.WindowsForms.dll")));
        }

        private static void ReportException(Exception e)
        {
            var form = new CrashForm {Exception = e};
            form.ShowDialog();
        }

        public static void Application_ThreadException(object sender,
            ThreadExceptionEventArgs e)
        {
            try
            {
                ReportException(e.Exception);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
            finally
            {
                Application.Exit();
            }
        }

        public static void CurrentDomain_UnhandledException(object sender,
            UnhandledExceptionEventArgs args)
        {
            try
            {
                var e = args.ExceptionObject as Exception;
                if (e != null)
                    ReportException(e);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
            finally
            {
                Environment.Exit(0);
            }
        }
    }
}
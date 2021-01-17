/*
 
  This Source Code Form is subject to the terms of the Mozilla Public
  License, v. 2.0. If a copy of the MPL was not distributed with this
  file, You can obtain one at http://mozilla.org/MPL/2.0/.
 
  Copyright (C) 2009-2020 Michael Möller <mmoeller@openhardwaremonitor.org>
	
*/

using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;
using OpenHardwareMonitor.GUI;

namespace OpenHardwareMonitor {
  public static class Program {

    [STAThread]
    public static void Main() {
      #if !DEBUG
        Application.ThreadException += 
          new ThreadExceptionEventHandler(Application_ThreadException);
        Application.SetUnhandledExceptionMode(
          UnhandledExceptionMode.CatchException);

        AppDomain.CurrentDomain.UnhandledException += 
          new UnhandledExceptionEventHandler(CurrentDomain_UnhandledException);
      #endif

      if (!AllRequiredFilesAvailable() || !IsNetFramework45Installed())
        Environment.Exit(0);

      Application.EnableVisualStyles();
      Application.SetCompatibleTextRenderingDefault(false);
      using (GUI.MainForm form = new GUI.MainForm()) {
        form.FormClosed += delegate(Object sender, FormClosedEventArgs e) {
          Application.Exit();
        };        
        Application.Run();
      }
    }

    private static bool IsFileAvailable(string fileName) {
      string path = Path.GetDirectoryName(Application.ExecutablePath) +
        Path.DirectorySeparatorChar;

      if (!File.Exists(path + fileName)) {
        MessageBox.Show("The following file could not be found: " + fileName + 
          "\nPlease extract all files from the archive.", "Error",
           MessageBoxButtons.OK, MessageBoxIcon.Error);
        return false;
      }
      return true;      
    }

    private static bool AllRequiredFilesAvailable() {
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

    private static bool IsNetFramework45Installed() {
      Type type;      
      try {
        type = TryGetDefaultDllImportSearchPathsAttributeType();
      } catch (TypeLoadException) {
        MessageBox.Show(
          "This application requires the .NET Framework 4.5 or a later version.\n" +
          "Please install the latest .NET Framework. For more information, see\n\n" +
          "https://dotnet.microsoft.com/download/dotnet-framework",
          "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        return false;
      }
      return type != null;
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static Type TryGetDefaultDllImportSearchPathsAttributeType() {
      return typeof(DefaultDllImportSearchPathsAttribute);
    }

    private static void ReportException(Exception e) {
      CrashForm form = new CrashForm();
      form.Exception = e;
      form.ShowDialog();
    }

    public static void Application_ThreadException(object sender, 
      ThreadExceptionEventArgs e) 
    {
      try {
        ReportException(e.Exception);
      } catch {
      } finally {
        Application.Exit();
      }
    }

    public static void CurrentDomain_UnhandledException(object sender, 
      UnhandledExceptionEventArgs args) 
    {
      try {
        Exception e = args.ExceptionObject as Exception;
        if (e != null)
          ReportException(e);
      } catch {
      } finally {
        Environment.Exit(0);
      }
    }   
  }
}

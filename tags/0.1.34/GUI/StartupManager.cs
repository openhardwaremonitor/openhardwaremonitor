/*
  
  Version: MPL 1.1/GPL 2.0/LGPL 2.1

  The contents of this file are subject to the Mozilla Public License Version
  1.1 (the "License"); you may not use this file except in compliance with
  the License. You may obtain a copy of the License at
 
  http://www.mozilla.org/MPL/

  Software distributed under the License is distributed on an "AS IS" basis,
  WITHOUT WARRANTY OF ANY KIND, either express or implied. See the License
  for the specific language governing rights and limitations under the License.

  The Original Code is the Open Hardware Monitor code.

  The Initial Developer of the Original Code is 
  Michael Möller <m.moeller@gmx.ch>.
  Portions created by the Initial Developer are Copyright (C) 2009-2010
  the Initial Developer. All Rights Reserved.

  Contributor(s):

  Alternatively, the contents of this file may be used under the terms of
  either the GNU General Public License Version 2 or later (the "GPL"), or
  the GNU Lesser General Public License Version 2.1 or later (the "LGPL"),
  in which case the provisions of the GPL or the LGPL are applicable instead
  of those above. If you wish to allow use of your version of this file only
  under the terms of either the GPL or the LGPL, and not to allow others to
  use your version of this file under the terms of the MPL, indicate your
  decision by deleting the provisions above and replace them with the notice
  and other provisions required by the GPL or the LGPL. If you do not delete
  the provisions above, a recipient may use your version of this file under
  the terms of any one of the MPL, the GPL or the LGPL.
 
*/

using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using Microsoft.Win32;
using OpenHardwareMonitor.TaskScheduler;

namespace OpenHardwareMonitor.GUI {
  public class StartupManager {

    private TaskSchedulerClass scheduler;
    private bool startup;

    private const string REGISTRY_RUN =
      @"Software\Microsoft\Windows\CurrentVersion\Run";

    public StartupManager() {
      try {
        scheduler = new TaskSchedulerClass();
        scheduler.Connect(null, null, null, null);
      } catch {
        scheduler = null;
      }

      if (scheduler != null) {
        try {
          ITaskFolder folder = scheduler.GetFolder("\\Open Hardware Monitor");
          IRegisteredTask task = folder.GetTask("Startup");
          startup = task != null;
        } catch (IOException) {
          startup = false;
        }
      } else {
        RegistryKey key = Registry.CurrentUser.OpenSubKey(REGISTRY_RUN);
        startup = false;
        if (key != null) {
          string value = (string)key.GetValue("OpenHardwareMonitor");
          if (value != null)
            startup = value == Application.ExecutablePath;
        }
      }
    }

    private void CreateSchedulerTask() {
      ITaskDefinition definition = scheduler.NewTask(0);
      definition.RegistrationInfo.Description =
        "This task starts the Open Hardware Monitor on Windows startup.";
      definition.Principal.RunLevel =
        TASK_RUNLEVEL.TASK_RUNLEVEL_HIGHEST;
      definition.Settings.DisallowStartIfOnBatteries = false;
      definition.Settings.StopIfGoingOnBatteries = false;
      definition.Settings.ExecutionTimeLimit = "PT0S";

      ILogonTrigger trigger = (ILogonTrigger)definition.Triggers.Create(
        TASK_TRIGGER_TYPE2.TASK_TRIGGER_LOGON);

      IExecAction action = (IExecAction)definition.Actions.Create(
        TASK_ACTION_TYPE.TASK_ACTION_EXEC);
      action.Path = Application.ExecutablePath;
      action.WorkingDirectory =
        Path.GetDirectoryName(Application.ExecutablePath);

      ITaskFolder root = scheduler.GetFolder("\\");
      ITaskFolder folder;
      try {
        folder = root.GetFolder("Open Hardware Monitor");
      } catch (IOException) {
        folder = root.CreateFolder("Open Hardware Monitor", "");
      }
      folder.RegisterTaskDefinition("Startup", definition,
        (int)TASK_CREATION.TASK_CREATE_OR_UPDATE, null, null,
        TASK_LOGON_TYPE.TASK_LOGON_INTERACTIVE_TOKEN, "");
    }

    private void DeleteSchedulerTask() {
      ITaskFolder root = scheduler.GetFolder("\\");
      try {
        ITaskFolder folder = root.GetFolder("Open Hardware Monitor");
        folder.DeleteTask("Startup", 0);
      } catch (IOException) { }
      try {
        root.DeleteFolder("Open Hardware Monitor", 0);
      } catch (IOException) { }
    }

    private void CreateRegistryRun() {
      RegistryKey key = Registry.CurrentUser.CreateSubKey(REGISTRY_RUN);
      key.SetValue("OpenHardwareMonitor", Application.ExecutablePath);
    }

    private void DeleteRegistryRun() {
      RegistryKey key = Registry.CurrentUser.CreateSubKey(REGISTRY_RUN);
      key.DeleteValue("OpenHardwareMonitor");
    }

    public bool Startup {
      get {
        return startup;
      }
      set {
        if (startup != value) {
          startup = value;
          if (scheduler != null) {
            if (startup)
              CreateSchedulerTask();
            else
              DeleteSchedulerTask();
          } else {
            if (startup)
              CreateRegistryRun();
            else
              DeleteRegistryRun();
          }
        }
      }
    }
  }

 

 

}

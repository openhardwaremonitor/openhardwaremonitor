/*
 
  This Source Code Form is subject to the terms of the Mozilla Public
  License, v. 2.0. If a copy of the MPL was not distributed with this
  file, You can obtain one at http://mozilla.org/MPL/2.0/.
 
  Copyright (C) 2009-2010 Michael Möller <mmoeller@openhardwaremonitor.org>
	
*/

using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.Principal;
using System.Windows.Forms;
using Microsoft.Win32;
using OpenHardwareMonitor.TaskScheduler;

namespace OpenHardwareMonitor.GUI
{
    public class StartupManager
    {
        private const string REGISTRY_RUN =
            @"Software\Microsoft\Windows\CurrentVersion\Run";

        private readonly TaskSchedulerClass scheduler;
        private bool startup;

        public StartupManager()
        {
            var p = (int) Environment.OSVersion.Platform;
            if ((p == 4) || (p == 128))
            {
                scheduler = null;
                IsAvailable = false;
                return;
            }

            if (IsAdministrator())
            {
                try
                {
                    scheduler = new TaskSchedulerClass();
                    scheduler.Connect(null, null, null, null);
                }
                catch
                {
                    scheduler = null;
                }

                if (scheduler != null)
                {
                    try
                    {
                        // check if the taskscheduler is running
                        var collection = scheduler.GetRunningTasks(0);

                        var folder = scheduler.GetFolder("\\Open Hardware Monitor");
                        var task = folder.GetTask("Startup");
                        startup = (task != null) &&
                                  (task.Definition.Triggers.Count > 0) &&
                                  (task.Definition.Triggers[1].Type ==
                                   TASK_TRIGGER_TYPE2.TASK_TRIGGER_LOGON) &&
                                  (task.Definition.Actions.Count > 0) &&
                                  (task.Definition.Actions[1].Type ==
                                   TASK_ACTION_TYPE.TASK_ACTION_EXEC) &&
                                  (task.Definition.Actions[1] as IExecAction != null) &&
                                  ((task.Definition.Actions[1] as IExecAction).Path ==
                                   Application.ExecutablePath);
                    }
                    catch (IOException)
                    {
                        startup = false;
                    }
                    catch (UnauthorizedAccessException)
                    {
                        scheduler = null;
                    }
                    catch (COMException)
                    {
                        scheduler = null;
                    }
                }
            }
            else
            {
                scheduler = null;
            }

            if (scheduler == null)
            {
                try
                {
                    using (var key =
                        Registry.CurrentUser.OpenSubKey(REGISTRY_RUN))
                    {
                        startup = false;
                        if (key != null)
                        {
                            var value = (string) key.GetValue("OpenHardwareMonitor");
                            if (value != null)
                                startup = value == Application.ExecutablePath;
                        }
                    }
                    IsAvailable = true;
                }
                catch (SecurityException)
                {
                    IsAvailable = false;
                }
            }
            else
            {
                IsAvailable = true;
            }
        }

        public bool IsAvailable { get; }

        public bool Startup
        {
            get { return startup; }
            set
            {
                if (startup != value)
                {
                    if (IsAvailable)
                    {
                        if (scheduler != null)
                        {
                            if (value)
                                CreateSchedulerTask();
                            else
                                DeleteSchedulerTask();
                            startup = value;
                        }
                        else
                        {
                            try
                            {
                                if (value)
                                    CreateRegistryRun();
                                else
                                    DeleteRegistryRun();
                                startup = value;
                            }
                            catch (UnauthorizedAccessException)
                            {
                                throw new InvalidOperationException();
                            }
                        }
                    }
                    else
                    {
                        throw new InvalidOperationException();
                    }
                }
            }
        }

        private bool IsAdministrator()
        {
            try
            {
                var identity = WindowsIdentity.GetCurrent();
                var principal = new WindowsPrincipal(identity);
                return principal.IsInRole(WindowsBuiltInRole.Administrator);
            }
            catch
            {
                return false;
            }
        }

        private void CreateSchedulerTask()
        {
            var definition = scheduler.NewTask(0);
            definition.RegistrationInfo.Description =
                "This task starts the Open Hardware Monitor on Windows startup.";
            definition.Principal.RunLevel =
                TASK_RUNLEVEL.TASK_RUNLEVEL_HIGHEST;
            definition.Settings.DisallowStartIfOnBatteries = false;
            definition.Settings.StopIfGoingOnBatteries = false;
            definition.Settings.ExecutionTimeLimit = "PT0S";

            var trigger = (ILogonTrigger) definition.Triggers.Create(
                TASK_TRIGGER_TYPE2.TASK_TRIGGER_LOGON);

            var action = (IExecAction) definition.Actions.Create(
                TASK_ACTION_TYPE.TASK_ACTION_EXEC);
            action.Path = Application.ExecutablePath;
            action.WorkingDirectory =
                Path.GetDirectoryName(Application.ExecutablePath);

            var root = scheduler.GetFolder("\\");
            ITaskFolder folder;
            try
            {
                folder = root.GetFolder("Open Hardware Monitor");
            }
            catch (IOException)
            {
                folder = root.CreateFolder("Open Hardware Monitor", "");
            }
            folder.RegisterTaskDefinition("Startup", definition,
                (int) TASK_CREATION.TASK_CREATE_OR_UPDATE, null, null,
                TASK_LOGON_TYPE.TASK_LOGON_INTERACTIVE_TOKEN, "");
        }

        private void DeleteSchedulerTask()
        {
            var root = scheduler.GetFolder("\\");
            try
            {
                var folder = root.GetFolder("Open Hardware Monitor");
                folder.DeleteTask("Startup", 0);
            }
            catch (IOException)
            {
            }
            try
            {
                root.DeleteFolder("Open Hardware Monitor", 0);
            }
            catch (IOException)
            {
            }
        }

        private void CreateRegistryRun()
        {
            var key = Registry.CurrentUser.CreateSubKey(REGISTRY_RUN);
            key.SetValue("OpenHardwareMonitor", Application.ExecutablePath);
        }

        private void DeleteRegistryRun()
        {
            var key = Registry.CurrentUser.CreateSubKey(REGISTRY_RUN);
            key.DeleteValue("OpenHardwareMonitor");
        }
    }
}
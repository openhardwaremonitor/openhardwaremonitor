/*
 
  This Source Code Form is subject to the terms of the Mozilla Public
  License, v. 2.0. If a copy of the MPL was not distributed with this
  file, You can obtain one at http://mozilla.org/MPL/2.0/.
 
  Copyright (C) 2009-2020 Michael Möller <mmoeller@openhardwaremonitor.org>
	
*/

using System;
using System.Collections.Generic;
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

        private TaskSchedulerClass scheduler;
        private bool startup;
        private bool isAvailable;
        private bool startupAsService;

        private const string REGISTRY_RUN =
            @"Software\Microsoft\Windows\CurrentVersion\Run";

        private bool IsAdministrator()
        {
            try
            {
                WindowsIdentity identity = WindowsIdentity.GetCurrent();
                WindowsPrincipal principal = new WindowsPrincipal(identity);
                return principal.IsInRole(WindowsBuiltInRole.Administrator);
            }
            catch
            {
                return false;
            }
        }

        public StartupManager()
        {
            startupAsService = false;
            if (Hardware.OperatingSystem.IsUnix)
            {
                scheduler = null;
                isAvailable = false;
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
                        try
                        {
                            // check if the taskscheduler is running
                            IRunningTaskCollection collection = scheduler.GetRunningTasks(0);
                        }
                        catch (ArgumentException)
                        {
                        }

                        ITaskFolder folder = scheduler.GetFolder("\\Open Hardware Monitor");
                        IRegisteredTask task = folder.GetTask("Startup");
                        startup = (task != null) &&
                                  (task.Definition.Triggers.Count > 0) &&
                                  (task.Definition.Actions.Count > 0) &&
                                  (task.Definition.Actions[1].Type ==
                                   TASK_ACTION_TYPE.TASK_ACTION_EXEC) &&
                                  (task.Definition.Actions[1] as IExecAction != null) &&
                                  ((task.Definition.Actions[1] as IExecAction).Path ==
                                   Application.ExecutablePath);
                        startupAsService = startup && (task.Definition.Triggers.Count > 0) &&
                                           (task.Definition.Triggers[1].Type ==
                                            TASK_TRIGGER_TYPE2.TASK_TRIGGER_BOOT);

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
                    catch (NotImplementedException)
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
                    using (RegistryKey key =
                           Registry.CurrentUser.OpenSubKey(REGISTRY_RUN))
                    {
                        startup = false;
                        if (key != null)
                        {
                            string value = (string)key.GetValue("OpenHardwareMonitor");
                            if (value != null)
                                startup = value == Application.ExecutablePath;
                        }
                    }

                    isAvailable = true;
                }
                catch (SecurityException)
                {
                    isAvailable = false;
                }
            }
            else
            {
                isAvailable = true;
            }
        }

        /// <summary>
        /// Enables starting OHM via the task scheduler (in contrast to the registry, this has the advantage of not popping up any UAC prompts)
        /// </summary>
        /// <param name="atBootup">True to configure for bootup (service) start, false to configure for logon start</param>
        private bool CreateSchedulerTask(bool atBootup)
        {
            ITaskDefinition definition = scheduler.NewTask(0);
            definition.RegistrationInfo.Description =
                "This task starts the Open Hardware Monitor on Windows startup.";
            definition.Principal.RunLevel =
                TASK_RUNLEVEL.TASK_RUNLEVEL_HIGHEST;
            if (atBootup)
            {
                definition.Principal.UserId = "S-1-5-18"; // SYSTEM
            }

            definition.Settings.DisallowStartIfOnBatteries = false;
            definition.Settings.StopIfGoingOnBatteries = false;
            definition.Settings.ExecutionTimeLimit = "PT0S";

            // While it's possible to set the trigger to BOOT (when also configuring the user as system) it doesn't seem to
            // be possible to interact with the program in that case. So this is only a good option if no one should normally log on to that system.
            ITrigger trigger = null;
            if (atBootup)
            {
                trigger = (IBootTrigger)definition.Triggers.Create(
                    TASK_TRIGGER_TYPE2.TASK_TRIGGER_BOOT);
            }
            else
            {
                trigger = (ILogonTrigger)definition.Triggers.Create(
                    TASK_TRIGGER_TYPE2.TASK_TRIGGER_LOGON);
            }

            IExecAction action = (IExecAction)definition.Actions.Create(
                TASK_ACTION_TYPE.TASK_ACTION_EXEC);
            action.Path = Application.ExecutablePath;
            action.WorkingDirectory =
                Path.GetDirectoryName(Application.ExecutablePath);

            // Always start the process minimized when starting via autostart/task scheduler
            action.Arguments = "--startminimized";
            
            ITaskFolder root = scheduler.GetFolder("\\");
            ITaskFolder folder;
            try
            {
                folder = root.GetFolder("Open Hardware Monitor");
            }
            catch (IOException)
            {
                folder = root.CreateFolder("Open Hardware Monitor", "");
            }

            if (atBootup)
            {
                return folder.RegisterTaskDefinition("Startup", definition,
                    (int)TASK_CREATION.TASK_CREATE_OR_UPDATE, null, null,
                    TASK_LOGON_TYPE.TASK_LOGON_NONE, "") != null;
            }
            else
            {
                return folder.RegisterTaskDefinition("Startup", definition,
                    (int)TASK_CREATION.TASK_CREATE_OR_UPDATE, null, null,
                    TASK_LOGON_TYPE.TASK_LOGON_INTERACTIVE_TOKEN, "") != null;
            }
        }

        private void DeleteSchedulerTask()
        {
            ITaskFolder root = scheduler.GetFolder("\\");
            try
            {
                ITaskFolder folder = root.GetFolder("Open Hardware Monitor");
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

            try
            {
                // Previous versions might have registered under this name
                root.DeleteTask("OpenHardwareMonitorAutoStart", 0);
            }
            catch (IOException)
            {
            }
        }

        private void CreateRegistryRun()
        {
            RegistryKey key = Registry.CurrentUser.CreateSubKey(REGISTRY_RUN);
            key.SetValue("OpenHardwareMonitor", Application.ExecutablePath);
        }

        private void DeleteRegistryRun()
        {
            RegistryKey key = Registry.CurrentUser.CreateSubKey(REGISTRY_RUN);
            key.DeleteValue("OpenHardwareMonitor");
        }

        public bool IsAvailable
        {
            get { return isAvailable; }
        }

        public bool IsAutoStartupEnabled
        {
            get { return startup; }
        }

        public bool IsStartupAsService
        {
            get
            {
                return startupAsService;
            }
        }

        /// <summary>
        /// Enable auto startup
        /// </summary>
        /// <param name="enable">True to enable, false to disable</param>
        /// <param name="atBootup">True to start at bootup, false to start at logon (if enable == false, this parameter is ignored)</param>
        /// <exception cref="InvalidOperationException"></exception>
        public void ConfigureAutoStartup(bool enable, bool atBootup)
        {
            bool enabled = false;
            if (isAvailable)
            {
                if (scheduler != null)
                {
                    DeleteSchedulerTask();
                    if (enable)
                        enabled = CreateSchedulerTask(atBootup);
                    startup = enabled;
                    startupAsService = enabled && atBootup;
                }
                else
                {
                    DeleteRegistryRun();

                    if (enable)
                        CreateRegistryRun();
                    startup = enable;
                    startupAsService = false;
                }
            }
            else
            {
                throw new InvalidOperationException();
            }
        }
    }
}

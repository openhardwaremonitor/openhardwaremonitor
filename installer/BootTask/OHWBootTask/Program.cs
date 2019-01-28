using System;
using Microsoft.Win32.TaskScheduler;


namespace OHWBootTask
{
    class Program
    {
        static int Main(string[] args)
        {
            if((args.Length != 2 && args[0].ToLower() == "enable") || (args.Length != 1 && args[0].ToLower() == "disable"))
                return 255;
            if (args[0].ToLower() != "enable" && args[0].ToLower() != "disable")
                return 254;
            using (TaskService ts = new TaskService())
            {
                if (args[0].ToLower() == "enable")
                {
                    TaskDefinition td = ts.NewTask();
                    td.RegistrationInfo.Description = "Open Hardware Monitor";
                    td.Triggers.Add(new LogonTrigger());
                    td.Actions.Add(new ExecAction(args[1] + "\\OpenHardwareMonitor.exe", null, null));
                    td.Settings.AllowDemandStart = true;
                    td.Settings.DisallowStartIfOnBatteries = false;
                    td.Settings.DisallowStartOnRemoteAppSession = false;
                    td.Settings.ExecutionTimeLimit = new TimeSpan();
                    td.Settings.MultipleInstances = TaskInstancesPolicy.IgnoreNew;
                    td.Settings.StopIfGoingOnBatteries = false;
                    td.Principal.RunLevel = TaskRunLevel.Highest;
                    ts.RootFolder.RegisterTaskDefinition(@"Open Hardware Monitor", td);
                    return 0;
                }
                else
                {
                    ts.RootFolder.DeleteTask("Open Hardware Monitor");
                    return 0;
                }
            }
        }
    }
}

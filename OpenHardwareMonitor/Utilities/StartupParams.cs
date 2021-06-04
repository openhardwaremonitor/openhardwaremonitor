using System;
using System.Collections.Generic;

namespace OpenHardwareMonitor.Utilities {
  public class StartupParams {
    private List<Types> Items;

    public StartupParams(string[] args) {
      Items = new List<Types>();

      for (int i = 0; i < args.Length; i++) {
        if (!args[i].StartsWith("--"))
          continue;

        Types type;

        if (Enum.TryParse(args[i].ToUpper().TrimStart('-'), out type)) {
          Items.Add(type);
        }
      }//for
    }

    public bool Contains(Types type) {
      return Items.Contains(type);
    }

    public enum Types {
      STARTMINIMIZED
    }
  }
}

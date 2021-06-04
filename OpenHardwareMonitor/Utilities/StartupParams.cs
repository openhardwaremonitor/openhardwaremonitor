using System;
using System.Collections.Generic;

namespace OpenHardwareMonitor.Utilities {
  public class StartupParams {
    private Dictionary<Types, string> Items;

    public StartupParams(string[] args) {
      Items = new Dictionary<Types, string>();

      for (int i = 0; i < args.Length; i++) {
        if (!args[i].StartsWith("--"))
          continue;

        Types type;
        var parts = args[i].Split('=');

        if (Enum.TryParse(parts[0].ToUpper().TrimStart('-'), out type)) {
          var value = parts.Length > 1 ? parts[1] : string.Empty;

          if (Items.ContainsKey(type)) Items[type] = value;
          else Items.Add(type, value);
        }
      }//for
    }

    public bool Contains(Types type) {
      return Items.ContainsKey(type);
    }

    public string GetValue(Types type) {
      if (Contains(type)) return Items[type];
      else return string.Empty;
    }

    public enum Types {
      STARTMINIMIZED
    }
  }
}

/*
 
  This Source Code Form is subject to the terms of the Mozilla Public
  License, v. 2.0. If a copy of the MPL was not distributed with this
  file, You can obtain one at http://mozilla.org/MPL/2.0/.
 
  Copyright (C) 2009-2010 Michael Möller <mmoeller@openhardwaremonitor.org>
	
*/

using System;
using System.Collections.Generic;
using System.Windows.Forms;
using OpenHardwareMonitor.Utilities;

namespace OpenHardwareMonitor.GUI {
  public class UserOption {
    private string name;
    private bool value;
    private ToolStripMenuItem menuItem;
    private event EventHandler changed;
    private PersistentSettings settings;

    public UserOption(string name, bool value,
      ToolStripMenuItem menuItem, PersistentSettings settings) {

      this.settings = settings;
      this.name = name;
      if (name != null)
        this.value = settings.GetValue(name, value);
      else
        this.value = value;
      this.menuItem = menuItem;
      this.menuItem.Checked = this.value;
      this.menuItem.Click += new EventHandler(menuItem_Click);
    }

    public UserOption(string name, bool value,
      ToolStripMenuItem menuItem, PersistentSettings settings, Func<bool?> overridenValue) {

      this.settings = settings;
      this.name = name;
      if (name != null)
        this.value = settings.GetValue(name, value);
      else
        this.value = value;
      if (overridenValue != null) {
        var v1 = overridenValue();
        if (v1.HasValue) {
          this.value = v1.Value;
        }
      }
      this.menuItem = menuItem;
      this.menuItem.Checked = this.value;
      this.menuItem.Click += new EventHandler(menuItem_Click);
    }
    private void menuItem_Click(object sender, EventArgs e) {
      this.Value = !this.Value;
    }    

    public bool Value {
      get { return value; }
      set {
        if (this.value != value) {
          this.value = value;
          if (this.name != null)
            settings.SetValue(name, value);
          this.menuItem.Checked = value;
          if (changed != null)
            changed(this, null);
        }
      }
    }

    public event EventHandler Changed {
      add {
        changed += value;
        if (changed != null)
          changed(this, null);
      }
      remove {
        changed -= value;
      }
    }
  }
}

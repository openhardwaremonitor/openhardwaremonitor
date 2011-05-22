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
  Portions created by the Initial Developer are Copyright (C) 20011
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
using System.Windows.Forms;
using OpenHardwareMonitor.Utilities;

namespace OpenHardwareMonitor.GUI {
  public class UserRadioGroup {
    private string name;
    private int value;
    private MenuItem[] menuItems;
    private event EventHandler changed;
    private PersistentSettings settings;

    public UserRadioGroup(string name, int value,
      MenuItem[] menuItems, PersistentSettings settings) {
      this.settings = settings;
      this.name = name;
      if (name != null)
        this.value = settings.GetValue(name, value);
      else
        this.value = value;
      this.menuItems = menuItems;
      this.value = Math.Max(Math.Min(this.value, menuItems.Length - 1), 0);

      for (int i = 0; i < this.menuItems.Length; i++) {
        this.menuItems[i].Checked = i == this.value;
        int index = i;
        this.menuItems[i].Click += delegate(object sender, EventArgs e) {
          this.Value = index;
        };
      }      
    }

    public int Value {
      get { return value; }
      set {
        if (this.value != value) {
          this.value = value;
          if (this.name != null)
            settings.SetValue(name, value);
          for (int i = 0; i < this.menuItems.Length; i++) 
            this.menuItems[i].Checked = i == value;
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

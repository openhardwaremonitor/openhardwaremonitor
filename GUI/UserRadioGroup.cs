/*
 
  This Source Code Form is subject to the terms of the Mozilla Public
  License, v. 2.0. If a copy of the MPL was not distributed with this
  file, You can obtain one at http://mozilla.org/MPL/2.0/.
 
  Copyright (C) 2011 Michael Möller <mmoeller@openhardwaremonitor.org>
	
*/

using System;
using System.Windows.Forms;

namespace OpenHardwareMonitor.GUI
{
    public class UserRadioGroup
    {
        private readonly MenuItem[] menuItems;
        private readonly string name;
        private readonly PersistentSettings settings;
        private int value;

        public UserRadioGroup(string name, int value,
            MenuItem[] menuItems, PersistentSettings settings)
        {
            this.settings = settings;
            this.name = name;
            this.value = name != null ? settings.GetValue(name, value) : value;
            this.menuItems = menuItems;
            this.value = Math.Max(Math.Min(this.value, menuItems.Length - 1), 0);

            for (var i = 0; i < this.menuItems.Length; i++)
            {
                this.menuItems[i].Checked = i == this.value;
                var index = i;
                this.menuItems[i].Click += delegate { Value = index; };
            }
        }

        public int Value
        {
            get => value;
            set
            {
                if (this.value != value)
                {
                    this.value = value;
                    if (name != null)
                        settings.SetValue(name, value);
                    for (var i = 0; i < menuItems.Length; i++)
                        menuItems[i].Checked = i == value;
                    changed?.Invoke(this, null);
                }
            }
        }

        private event EventHandler changed;

        public event EventHandler Changed
        {
            add
            {
                changed += value;
                changed?.Invoke(this, null);
            }
            remove => changed -= value;
        }
    }
}
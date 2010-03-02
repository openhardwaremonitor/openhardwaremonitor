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
using System.Drawing;

namespace OpenHardwareMonitor.Hardware.LPC {
  public abstract class LPCHardware : Hardware {
        
    private Image icon;
    protected readonly string name;
    protected readonly Chip chip;

    public LPCHardware(Chip chip) {
      this.chip = chip;
      this.icon = Utilities.EmbeddedResources.GetImage("chip.png");

      switch (chip) {
        case Chip.F71858: name = "Fintek F71858"; break;
        case Chip.F71862: name = "Fintek F71862"; break;
        case Chip.F71869: name = "Fintek F71869"; break;
        case Chip.F71882: name = "Fintek F71882"; break;
        case Chip.F71889: name = "Fintek F71889"; break;
        case Chip.IT8716F: this.name = "ITE IT8716F"; break;
        case Chip.IT8718F: this.name = "ITE IT8718F"; break;
        case Chip.IT8720F: this.name = "ITE IT8720F"; break;
        case Chip.IT8726F: this.name = "ITE IT8726F"; break;
        case Chip.W83627DHG: this.name = "Winbond W83627DHG"; break;
        case Chip.W83627DHGP: this.name = "Winbond W83627DHG-P"; break;
        case Chip.W83627EHF: this.name = "Winbond W83627EHF"; break;
        case Chip.W83627HF: this.name = "Winbond W83627HF"; break;
        case Chip.W83627THF: this.name = "Winbond W83627THF"; break;
        case Chip.W83667HG: this.name = "Winbond W83667HG"; break;
        case Chip.W83667HGB: this.name = "Winbond W83667HG-B"; break;
        case Chip.W83687THF: this.name = "Winbond W83687THF"; break;
      }
    }

    public string Identifier {
      get { return "/lpc/" + chip.ToString().ToLower(); }
    }

    public Image Icon {
      get { return icon; }
    }

    public string Name {
      get { return name; }
    }
  }
}

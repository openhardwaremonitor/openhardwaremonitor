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
  Roland Reinl <roland-reinl@gmx.de>.
  Portions created by the Initial Developer are Copyright (C) 2009-2012
  the Initial Developer. All Rights Reserved.

  Contributor(s):
    Paul Werelds
    Michael Möller <m.moeller@gmx.ch>
 
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

namespace OpenHardwareMonitor.Hardware.HDD {
  using System.Collections.Generic;

  [NamePrefix(""), RequireSmart(0x01), RequireSmart(0x09), RequireSmart(0x0C), 
    RequireSmart(0xD1), RequireSmart(0xCE), RequireSmart(0xCF)]
  internal class SSDIndilinx : AbstractHarddrive {

    private static readonly IEnumerable<SmartAttribute> smartAttributes =
      new List<SmartAttribute> {
        new SmartAttribute(0x01, SmartNames.ReadErrorRate),
        new SmartAttribute(0x09, SmartNames.PowerOnHours),
        new SmartAttribute(0x0C, SmartNames.PowerCycleCount),
        new SmartAttribute(0xB8, SmartNames.InitialBadBlockCount),
        new SmartAttribute(0xC3, SmartNames.ProgramFailure),
        new SmartAttribute(0xC4, SmartNames.EraseFailure),
        new SmartAttribute(0xC5, SmartNames.ReadFailure),
        new SmartAttribute(0xC6, SmartNames.SectorsRead),
        new SmartAttribute(0xC7, SmartNames.SectorsWritten),
        new SmartAttribute(0xC8, SmartNames.ReadCommands),
        new SmartAttribute(0xC9, SmartNames.WriteCommands),
        new SmartAttribute(0xCA, SmartNames.BitErrors),
        new SmartAttribute(0xCB, SmartNames.CorrectedErrors),
        new SmartAttribute(0xCC, SmartNames.BadBlockFullFlag),
        new SmartAttribute(0xCD, SmartNames.MaxCellCycles),
        new SmartAttribute(0xCE, SmartNames.MinErase),
        new SmartAttribute(0xCF, SmartNames.MaxErase),
        new SmartAttribute(0xD0, SmartNames.AverageEraseCount),
        new SmartAttribute(0xD1, SmartNames.RemainingLife,
          null, SensorType.Level, 0),
        new SmartAttribute(0xD2, SmartNames.UnknownUnique),
        new SmartAttribute(0xD3, SmartNames.SataErrorCountCrc),
        new SmartAttribute(0xD4, SmartNames.SataErrorCountHandshake)
      };

    public SSDIndilinx(ISmart smart, string name, string firmwareRevision, 
      int index, ISettings settings)
      : base(smart, name, firmwareRevision, index, smartAttributes, settings) {}
  }
}




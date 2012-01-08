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
  Portions created by the Initial Developer are Copyright (C) 2012
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

namespace OpenHardwareMonitor.Hardware.HDD {
  using System.Collections.Generic;

  [NamePrefix(""), RequireSmart(0xB0), RequireSmart(0xB1), RequireSmart(0xB2), 
    RequireSmart(0xB3), RequireSmart(0xB4), RequireSmart(0xB5), 
    RequireSmart(0xB6), RequireSmart(0xB7)]
  internal class SSDSamsung : AbstractHarddrive {

    private static readonly IEnumerable<SmartAttribute> smartAttributes =
      new List<SmartAttribute> {
      new SmartAttribute(0x09, SmartNames.PowerOnHours, RawToInt),
      new SmartAttribute(0x0C, SmartNames.PowerCycleCount, RawToInt),
      new SmartAttribute(0xAF, SmartNames.ProgramFailCountChip, RawToInt),
      new SmartAttribute(0xB0, SmartNames.EraseFailCountChip, RawToInt),
      new SmartAttribute(0xB1, SmartNames.WearLevelingCount, RawToInt),
      new SmartAttribute(0xB2, SmartNames.UsedReservedBlockCountChip, RawToInt),
      new SmartAttribute(0xB3, SmartNames.UsedReservedBlockCountTotal, RawToInt),

      // Unused Reserved Block Count (Total)
      new SmartAttribute(0xB4, SmartNames.RemainingLife,
        null, SensorType.Level, 0),
      
      new SmartAttribute(0xB5, SmartNames.ProgramFailCountTotal, RawToInt),
      new SmartAttribute(0xB6, SmartNames.EraseFailCountTotal, RawToInt),
      new SmartAttribute(0xB7, SmartNames.RuntimeBadBlockTotal, RawToInt),
      new SmartAttribute(0xBB, SmartNames.UncorrectableErrorCount, RawToInt),
      new SmartAttribute(0xBE, SmartNames.TemperatureExceedCount, RawToInt),
      new SmartAttribute(0xC2, SmartNames.AirflowTemperature),
      new SmartAttribute(0xC3, SmartNames.ECCRate),
      new SmartAttribute(0xC6, SmartNames.OffLineUncorrectableErrorCount, RawToInt),
      new SmartAttribute(0xC7, SmartNames.CRCErrorCount, RawToInt),
      new SmartAttribute(0xC9, SmartNames.SupercapStatus),
      new SmartAttribute(0xCA, SmartNames.ExceptionModeStatus)
    };

    public SSDSamsung(ISmart smart, string name, string firmwareRevision,
      int index, ISettings settings)
      : base(smart, name, firmwareRevision, index, smartAttributes, settings) { }
  }
}

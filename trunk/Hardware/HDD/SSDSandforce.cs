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
  Portions created by the Initial Developer are Copyright (C) 2009-2012
  the Initial Developer. All Rights Reserved.

  Contributor(s):
    Paul Werelds
 
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

  [NamePrefix(""), RequireSmart(0xAB)]
  internal class SSDSandforce : AbstractHarddrive {

    private static readonly IEnumerable<SmartAttribute> smartAttributes =
      new List<SmartAttribute> {
      new SmartAttribute(0x05, SmartAttributeNames.RetiredBlockCount),
      new SmartAttribute(0x09, SmartAttributeNames.PowerOnHours, RawToInt),
      new SmartAttribute(0x0C, SmartAttributeNames.PowerCycleCount, RawToInt),
      new SmartAttribute(0xAB, SmartAttributeNames.ProgramFailCount),
      new SmartAttribute(0xAC, SmartAttributeNames.EraseFailCount),
      new SmartAttribute(0xAE, SmartAttributeNames.UnexpectedPowerLossCount),
      new SmartAttribute(0xB1, SmartAttributeNames.WearRangeDelta),
      new SmartAttribute(0xB5, SmartAttributeNames.AlternativeProgramFailCount),
      new SmartAttribute(0xB6, SmartAttributeNames.AlternativeEraseFailCount),
      new SmartAttribute(0xC3, SmartAttributeNames.UnrecoverableEcc), 
      new SmartAttribute(0xC4, SmartAttributeNames.ReallocationEventCount),
      new SmartAttribute(0xE7, SmartAttributeNames.RemainingLife, 
        null, SensorType.Level, 0),
      new SmartAttribute(0xF1, SmartAttributeNames.HostWrites, 
        (byte[] r, byte v) => { return RawToInt(r, v); }, 
        SensorType.Data, 0),
      new SmartAttribute(0xF2, SmartAttributeNames.HostReads, 
        (byte[] r, byte v) => { return RawToInt(r, v); }, 
        SensorType.Data, 1)
    };

    public SSDSandforce(ISmart smart, string name, string firmwareRevision, 
      int index, ISettings settings) 
      : base(smart, name, firmwareRevision,  index, smartAttributes, settings) 
    { }
  }
}

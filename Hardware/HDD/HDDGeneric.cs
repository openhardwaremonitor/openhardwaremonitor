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
  Portions created by the Initial Developer are Copyright (C) 2011-2012
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

using System;
using System.Collections.Generic;
using System.Text;

namespace OpenHardwareMonitor.Hardware.HDD {

  [NamePrefix("")]
  internal class GenericHarddisk : AbstractHarddrive {

    private static readonly List<SmartAttribute> smartAttributes =
      new List<SmartAttribute> {
      new SmartAttribute(0x01, SmartAttributeNames.ReadErrorRate),
      new SmartAttribute(0x02, SmartAttributeNames.ThroughputPerformance),
      new SmartAttribute(0x03, SmartAttributeNames.SpinUpTime),
      new SmartAttribute(0x04, SmartAttributeNames.StartStopCount, RawToInt),
      new SmartAttribute(0x05, SmartAttributeNames.ReallocatedSectorsCount),
      new SmartAttribute(0x06, SmartAttributeNames.ReadChannelMargin),
      new SmartAttribute(0x07, SmartAttributeNames.SeekErrorRate),
      new SmartAttribute(0x08, SmartAttributeNames.SeekTimePerformance),
      new SmartAttribute(0x09, SmartAttributeNames.PowerOnHours, RawToInt),
      new SmartAttribute(0x0A, SmartAttributeNames.SpinRetryCount),
      new SmartAttribute(0x0B, SmartAttributeNames.RecalibrationRetries),
      new SmartAttribute(0x0C, SmartAttributeNames.PowerCycleCount, RawToInt),
      new SmartAttribute(0x0D, SmartAttributeNames.SoftReadErrorRate),
      new SmartAttribute(0xAA, SmartAttributeNames.Unknown), 
      new SmartAttribute(0xAB, SmartAttributeNames.Unknown), 
      new SmartAttribute(0xAC, SmartAttributeNames.Unknown),
      new SmartAttribute(0xB7, SmartAttributeNames.SataDownshiftErrorCount),
      new SmartAttribute(0xB8, SmartAttributeNames.EndToEndError),
      new SmartAttribute(0xB9, SmartAttributeNames.HeadStability),
      new SmartAttribute(0xBA, SmartAttributeNames.InducedOpVibrationDetection),
      new SmartAttribute(0xBB, SmartAttributeNames.ReportedUncorrectableErrors),
      new SmartAttribute(0xBC, SmartAttributeNames.CommandTimeout),
      new SmartAttribute(0xBD, SmartAttributeNames.HighFlyWrites),
      new SmartAttribute(0xBF, SmartAttributeNames.GSenseErrorRate),
      new SmartAttribute(0xC0, SmartAttributeNames.EmergencyRetractCycleCount),
      new SmartAttribute(0xC1, SmartAttributeNames.LoadCycleCount),
      new SmartAttribute(0xC3, SmartAttributeNames.HardwareEccRecovered),
      new SmartAttribute(0xC4, SmartAttributeNames.ReallocationEventCount),
      new SmartAttribute(0xC5, SmartAttributeNames.CurrentPendingSectorCount),
      new SmartAttribute(0xC6, SmartAttributeNames.UncorrectableSectorCount),
      new SmartAttribute(0xC7, SmartAttributeNames.UltraDmaCrcErrorCount),
      new SmartAttribute(0xC8, SmartAttributeNames.WriteErrorRate),
      new SmartAttribute(0xCA, SmartAttributeNames.DataAddressMarkErrors),
      new SmartAttribute(0xCB, SmartAttributeNames.RunOutCancel),
      new SmartAttribute(0xCC, SmartAttributeNames.SoftEccCorrection),
      new SmartAttribute(0xCD, SmartAttributeNames.ThermalAsperityRate),
      new SmartAttribute(0xCE, SmartAttributeNames.FlyingHeight),
      new SmartAttribute(0xCF, SmartAttributeNames.SpinHighCurrent),
      new SmartAttribute(0xD0, SmartAttributeNames.SpinBuzz),
      new SmartAttribute(0xD1, SmartAttributeNames.OfflineSeekPerformance),
      new SmartAttribute(0xD3, SmartAttributeNames.VibrationDuringWrite),
      new SmartAttribute(0xD4, SmartAttributeNames.ShockDuringWrite),
      new SmartAttribute(0xDC, SmartAttributeNames.DiskShift),
      new SmartAttribute(0xDD, SmartAttributeNames.AlternativeGSenseErrorRate), 
      new SmartAttribute(0xDE, SmartAttributeNames.LoadedHours),
      new SmartAttribute(0xDF, SmartAttributeNames.LoadUnloadRetryCount),
      new SmartAttribute(0xE0, SmartAttributeNames.LoadFriction),
      new SmartAttribute(0xE1, SmartAttributeNames.LoadUnloadCycleCount),
      new SmartAttribute(0xE2, SmartAttributeNames.LoadInTime),
      new SmartAttribute(0xE3, SmartAttributeNames.TorqueAmplificationCount),
      new SmartAttribute(0xE4, SmartAttributeNames.PowerOffRetractCycle),
      new SmartAttribute(0xE6, SmartAttributeNames.GmrHeadAmplitude),      
      new SmartAttribute(0xE8, SmartAttributeNames.EnduranceRemaining),
      new SmartAttribute(0xE9, SmartAttributeNames.PowerOnHours),
      new SmartAttribute(0xF0, SmartAttributeNames.HeadFlyingHours),
      new SmartAttribute(0xF1, SmartAttributeNames.TotalLbasWritten),
      new SmartAttribute(0xF2, SmartAttributeNames.TotalLbasRead),
      new SmartAttribute(0xFA, SmartAttributeNames.ReadErrorRetryRate),
      new SmartAttribute(0xFE, SmartAttributeNames.FreeFallProtection),

      new SmartAttribute(0xC2, SmartAttributeNames.Temperature, 
        (byte[] r, byte v) => { return r[0]; }, SensorType.Temperature, 0),
      new SmartAttribute(0xE7, SmartAttributeNames.Temperature, 
        (byte[] r, byte v) => { return r[0]; }, SensorType.Temperature, 0),
      new SmartAttribute(0xBE, SmartAttributeNames.TemperatureDifferenceFrom100, 
        null, SensorType.Temperature, 0)
    };

    public GenericHarddisk(ISmart smart, string name, string firmwareRevision, 
      int index, ISettings settings)
      : base(smart, name, firmwareRevision, index, smartAttributes, settings) {}
  }
}

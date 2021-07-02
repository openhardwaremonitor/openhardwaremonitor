/*
 
  This Source Code Form is subject to the terms of the Mozilla Public
  License, v. 2.0. If a copy of the MPL was not distributed with this
  file, You can obtain one at http://mozilla.org/MPL/2.0/.
 
  Copyright (C) 2012-2020 Michael Möller <mmoeller@openhardwaremonitor.org>
	
*/

using System.Collections.Generic;
using OpenHardwareMonitor.Collections;

namespace OpenHardwareMonitor.Hardware.HDD {  

  [NamePrefix(""), RequireSmart(0xAB), RequireSmart(0xAC), 
   RequireSmart(0xAD), RequireSmart(0xAE), RequireSmart(0xC4),
   RequireSmart(0xCA), RequireSmart(0xCE)]
  internal class SSDMicron : ATAStorage {

    private static readonly IEnumerable<SmartAttribute> smartAttributes =
      new List<SmartAttribute> {
      
      new SmartAttribute(0x01, SmartNames.ReadErrorRate, RawToValue),
      new SmartAttribute(0x05, SmartNames.ReallocatedNANDBlockCount, RawToValue),
      new SmartAttribute(0x09, SmartNames.PowerOnHours, RawToValue),
      new SmartAttribute(0x0C, SmartNames.PowerCycleCount, RawToValue),
      new SmartAttribute(0xAA, SmartNames.NewFailingBlockCount, RawToValue),
      new SmartAttribute(0xAB, SmartNames.ProgramFailCount, RawToValue),
      new SmartAttribute(0xAC, SmartNames.EraseFailCount, RawToValue),
      new SmartAttribute(0xAD, SmartNames.WearLevelingCount, RawToValue),
      new SmartAttribute(0xAE, SmartNames.UnexpectedPowerLossCount, RawToValue),
      new SmartAttribute(0xB4, SmartNames.UnusedReserveNANDBlocks, RawToValue),
      new SmartAttribute(0xB5, SmartNames.Non4kAlignedAccess, 
        (byte[] raw, byte value, IReadOnlyArray<IParameter> p) 
          => { return 6e4f * ((raw[5] << 8) | raw[4]); }),      
      new SmartAttribute(0xB7, SmartNames.SataDownshiftErrorCount, RawToValue),
      new SmartAttribute(0xB8, SmartNames.ErrorCorrectionCount, RawToValue),
      new SmartAttribute(0xBB, SmartNames.ReportedUncorrectableErrors, RawToValue),
      new SmartAttribute(0xBC, SmartNames.CommandTimeout, RawToValue),
      new SmartAttribute(0xBD, SmartNames.FactoryBadBlockCount, RawToValue),
      new SmartAttribute(0xC2, SmartNames.Temperature, (raw, value, p) => SignedRawToValue(raw, 1)),
      new SmartAttribute(0xC4, SmartNames.ReallocationEventCount, RawToValue),
      new SmartAttribute(0xC5, SmartNames.CurrentPendingSectorCount),
      new SmartAttribute(0xC6, SmartNames.OffLineUncorrectableErrorCount, RawToValue),
      new SmartAttribute(0xC7, SmartNames.UltraDmaCrcErrorCount, RawToValue),
      new SmartAttribute(0xCA, SmartNames.RemainingLife, 
        (byte[] raw, byte value, IReadOnlyArray<IParameter> p) 
          => { return 100 - RawToValue(raw, value, p); }, 
        SensorType.Level, 0, SmartNames.RemainingLife),
      new SmartAttribute(0xCE, SmartNames.WriteErrorRate,
         (byte[] raw, byte value, IReadOnlyArray<IParameter> p)
           => { return 6e4f * ((raw[1] << 8) | raw[0]); }),
      new SmartAttribute(0xD2, SmartNames.SuccessfulRAINRecoveryCount, RawToValue),
      new SmartAttribute(0xF6, SmartNames.TotalLbasWritten,
        (byte[] r, byte v, IReadOnlyArray<IParameter> p) => {
          return (((long)r[5] << 40) | ((long)r[4] << 32) | ((long)r[3] << 24) |
            ((long)r[2] << 16) | ((long)r[1] << 8) | r[0]) *
            (512.0f / 1024 / 1024 / 1024);
        }, SensorType.Data, 0, "Total Bytes Written"),
      new SmartAttribute(0xF7, SmartNames.HostProgramNANDPagesCount, RawToValue),
      new SmartAttribute(0xF8, SmartNames.FTLProgramNANDPagesCount, RawToValue)
    };

    private Sensor temperature;
    private Sensor writeAmplification;

    public SSDMicron(ISmart smart, string name, string firmwareRevision, 
      int index, ISettings settings)
      : base(smart, name, firmwareRevision, "ssd", index, smartAttributes, settings) 
    {
      this.temperature = new Sensor("Temperature", 0, false,
        SensorType.Temperature, this,
        new[] { new ParameterDescription("Offset [°C]",
          "Temperature offset of the thermal sensor.\n" +
          "Temperature = Value + Offset.", 0) }, settings);
      this.writeAmplification = new Sensor("Write Amplification", 0,
        SensorType.Factor, this, settings);
    }

    public override void UpdateAdditionalSensors(DriveAttributeValue[] values) {
      double? hostProgramPagesCount = null;
      double? ftlProgramPagesCount = null;
      foreach (DriveAttributeValue value in values) {
        if (value.Identifier == 0xF7)
          hostProgramPagesCount = RawToValue(value.RawValue, value.AttrValue, null);

        if (value.Identifier == 0xF8)
          ftlProgramPagesCount = RawToValue(value.RawValue, value.AttrValue, null);

        if (value.Identifier == 0xC2) {
          temperature.Value = 
            value.RawValue[0] + temperature.Parameters[0].Value;
          if (value.RawValue[0] != 0)
            ActivateSensor(temperature);
        }
      }
      if (hostProgramPagesCount.HasValue && ftlProgramPagesCount.HasValue) {
        if (hostProgramPagesCount.Value > 0)
          writeAmplification.Value =
            (hostProgramPagesCount.Value + ftlProgramPagesCount) / 
            hostProgramPagesCount.Value;
        else
          writeAmplification.Value = 0;
        ActivateSensor(writeAmplification);
      }
    }
  }
}

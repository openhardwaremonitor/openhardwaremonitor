/*
 
  This Source Code Form is subject to the terms of the Mozilla Public
  License, v. 2.0. If a copy of the MPL was not distributed with this
  file, You can obtain one at http://mozilla.org/MPL/2.0/.
 
  Copyright (C) 2009-2022 Christoph Walcher
*/

using System.Runtime.CompilerServices;

namespace OpenHardwareMonitor.Hardware.LPC {
  internal class ChromiumEC : ISuperIO {

    private readonly LPCPort port;

    private readonly float?[] voltages = new float?[1];
    private readonly float?[] temperatures = new float?[EC_TEMP_SENSOR_ENTRIES];
    private readonly float?[] fans = new float?[EC_FAN_SPEED_ENTRIES];
    private readonly float?[] controls = new float?[0];

    public ChromiumEC(LPCPort port) {
      this.port = port;
    }

    public string GetReport() {
      return "Chromium Embedded Controller";
    }

    public byte? ReadGPIO(int index) {
      return null;
    }

    public void SetControl(int index, byte? value) { }


    private float fahrenheitToCelcius(float fahrenheit) => (fahrenheit - 32) / 1.8f;


    // https://github.com/FrameworkComputer/EmbeddedController/blob/hx20/include/ec_commands.h
    private const ushort EC_MEMMAP_TEMP_SENSOR_OFFSET = 0x100;
    private const ushort EC_MEMMAP_FAN_OFFSET = 0x110;
    public const ushort EC_MEMMAP_ID = 0x120;
    private const ushort EC_MEMMAP_BATT_VOLT = 0x140;

    private const int EC_TEMP_SENSOR_ENTRIES = 16;
    private const int EC_FAN_SPEED_ENTRIES = 4;

    private const byte EC_TEMP_SENSOR_NOT_PRESENT = 0xff;
    private const byte EC_TEMP_SENSOR_ERROR = 0xfe;
    private const byte EC_TEMP_SENSOR_NOT_POWERED = 0xfd;
    private const byte EC_TEMP_SENSOR_NOT_CALIBRATED = 0xfc;

    private const ushort EC_FAN_SPEED_NOT_PRESENT = 0xffff;
    private const ushort EC_FAN_SPEED_STALLED = 0xfffe;

    public void Update() {
      byte[] buf = port.ReadChromiumEC(EC_MEMMAP_TEMP_SENSOR_OFFSET, EC_TEMP_SENSOR_ENTRIES);
      for (int i = 0; i < EC_TEMP_SENSOR_ENTRIES; i++) {
        byte val = buf[i];
        if (val == EC_TEMP_SENSOR_NOT_PRESENT) { continue; }
        if (val == EC_TEMP_SENSOR_ERROR || val == EC_TEMP_SENSOR_NOT_POWERED || val == EC_TEMP_SENSOR_NOT_CALIBRATED) {
          temperatures[i] = float.NaN;
        } else {
          temperatures[i] = fahrenheitToCelcius(buf[i]);
        }
      }

      buf = port.ReadChromiumEC(EC_MEMMAP_FAN_OFFSET, EC_FAN_SPEED_ENTRIES * 2);
      for (int i = 0; i < EC_FAN_SPEED_ENTRIES; i++) {
        ushort val = (ushort)(buf[i * 2] << 8 | buf[i * 2 + 1]);
        if (val == EC_FAN_SPEED_NOT_PRESENT) { continue; }
        if (val == EC_FAN_SPEED_STALLED) {
          fans[i] = float.NaN;
        } else {
          fans[i] = val;
        }
      }

      // Read in mV
      voltages[0] = port.ReadWord(EC_MEMMAP_BATT_VOLT) / 1000f;
    }

    public void WriteGPIO(int index, byte value) { }

    public Chip Chip { get { return Chip.ChromiumEC; } }
    public float?[] Voltages { get { return voltages; } }
    public float?[] Temperatures { get { return temperatures; } }
    public float?[] Fans { get { return fans; } }
    public float?[] Controls { get { return controls; } }
  }
}

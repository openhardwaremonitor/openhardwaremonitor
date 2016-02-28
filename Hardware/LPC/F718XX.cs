/*
 
  This Source Code Form is subject to the terms of the Mozilla Public
  License, v. 2.0. If a copy of the MPL was not distributed with this
  file, You can obtain one at http://mozilla.org/MPL/2.0/.
 
  Copyright (C) 2009-2011 Michael Möller <mmoeller@openhardwaremonitor.org>
	
*/

using System.Globalization;
using System.Text;

namespace OpenHardwareMonitor.Hardware.LPC
{
    internal class F718XX : ISuperIO
    {
        // Hardware Monitor
        private const byte ADDRESS_REGISTER_OFFSET = 0x05;
        private const byte DATA_REGISTER_OFFSET = 0x06;

        // Hardware Monitor Registers
        private const byte VOLTAGE_BASE_REG = 0x20;
        private const byte TEMPERATURE_CONFIG_REG = 0x69;
        private const byte TEMPERATURE_BASE_REG = 0x70;

        private readonly ushort address;

        private readonly byte[] FAN_TACHOMETER_REG =
        {0xA0, 0xB0, 0xC0, 0xD0};

        public F718XX(Chip chip, ushort address)
        {
            this.address = address;
            Chip = chip;

            Voltages = new float?[chip == Chip.F71858 ? 3 : 9];
            Temperatures = new float?[chip == Chip.F71808E ? 2 : 3];
            Fans = new float?[chip == Chip.F71882 || chip == Chip.F71858 ? 4 : 3];
            Controls = new float?[0];
        }

        public byte? ReadGPIO(int index)
        {
            return null;
        }

        public void WriteGPIO(int index, byte value)
        {
        }

        public void SetControl(int index, byte? value)
        {
        }

        public Chip Chip { get; }
        public float?[] Voltages { get; }
        public float?[] Temperatures { get; }
        public float?[] Fans { get; }
        public float?[] Controls { get; }

        public string GetReport()
        {
            var r = new StringBuilder();

            r.AppendLine("LPC " + GetType().Name);
            r.AppendLine();
            r.Append("Base Adress: 0x");
            r.AppendLine(address.ToString("X4", CultureInfo.InvariantCulture));
            r.AppendLine();

            if (!Ring0.WaitIsaBusMutex(100))
                return r.ToString();

            r.AppendLine("Hardware Monitor Registers");
            r.AppendLine();
            r.AppendLine("      00 01 02 03 04 05 06 07 08 09 0A 0B 0C 0D 0E 0F");
            r.AppendLine();
            for (var i = 0; i <= 0xF; i++)
            {
                r.Append(" ");
                r.Append((i << 4).ToString("X2", CultureInfo.InvariantCulture));
                r.Append("  ");
                for (var j = 0; j <= 0xF; j++)
                {
                    r.Append(" ");
                    r.Append(ReadByte((byte) ((i << 4) | j)).ToString("X2",
                        CultureInfo.InvariantCulture));
                }
                r.AppendLine();
            }
            r.AppendLine();

            Ring0.ReleaseIsaBusMutex();

            return r.ToString();
        }

        public void Update()
        {
            if (!Ring0.WaitIsaBusMutex(10))
                return;

            for (var i = 0; i < Voltages.Length; i++)
            {
                if (Chip == Chip.F71808E && i == 6)
                {
                    // 0x26 is reserved on F71808E
                    Voltages[i] = 0;
                }
                else
                {
                    int value = ReadByte((byte) (VOLTAGE_BASE_REG + i));
                    Voltages[i] = 0.008f*value;
                }
            }

            for (var i = 0; i < Temperatures.Length; i++)
            {
                switch (Chip)
                {
                    case Chip.F71858:
                    {
                        var tableMode = 0x3 & ReadByte(TEMPERATURE_CONFIG_REG);
                        int high =
                            ReadByte((byte) (TEMPERATURE_BASE_REG + 2*i));
                        int low =
                            ReadByte((byte) (TEMPERATURE_BASE_REG + 2*i + 1));
                        if (high != 0xbb && high != 0xcc)
                        {
                            var bits = 0;
                            switch (tableMode)
                            {
                                case 0:
                                    bits = 0;
                                    break;
                                case 1:
                                    bits = 0;
                                    break;
                                case 2:
                                    bits = (high & 0x80) << 8;
                                    break;
                                case 3:
                                    bits = (low & 0x01) << 15;
                                    break;
                            }
                            bits |= high << 7;
                            bits |= (low & 0xe0) >> 1;
                            var value = (short) (bits & 0xfff0);
                            Temperatures[i] = value/128.0f;
                        }
                        else
                        {
                            Temperatures[i] = null;
                        }
                    }
                        break;
                    default:
                    {
                        var value = (sbyte) ReadByte((byte) (
                            TEMPERATURE_BASE_REG + 2*(i + 1)));
                        if (value < sbyte.MaxValue && value > 0)
                            Temperatures[i] = value;
                        else
                            Temperatures[i] = null;
                    }
                        break;
                }
            }

            for (var i = 0; i < Fans.Length; i++)
            {
                var value = ReadByte(FAN_TACHOMETER_REG[i]) << 8;
                value |= ReadByte((byte) (FAN_TACHOMETER_REG[i] + 1));

                if (value > 0)
                    Fans[i] = (value < 0x0fff) ? 1.5e6f/value : 0;
                else
                    Fans[i] = null;
            }

            Ring0.ReleaseIsaBusMutex();
        }

        private byte ReadByte(byte register)
        {
            Ring0.WriteIoPort(
                (ushort) (address + ADDRESS_REGISTER_OFFSET), register);
            return Ring0.ReadIoPort((ushort) (address + DATA_REGISTER_OFFSET));
        }
    }
}
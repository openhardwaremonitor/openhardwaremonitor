using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace OpenHardwareMonitor.Hardware.LPC {
  public class W83627DHG : IHardware {

    private byte revision;

    private string name;
    private Image icon;

    private bool available = false;
    private ushort address;

    private List<ISensor> active = new List<ISensor>();

    private Sensor[] temperatures;
    private Sensor[] fans;
    private Sensor[] voltages;

    private float[] voltageGains;

    // Consts 
    private const ushort WINBOND_VENDOR_ID = 0x5CA3;
    private const byte HIGH_BYTE = 0x80;

    // Hardware Monitor
    private const byte ADDRESS_REGISTER_OFFSET = 0x05;
    private const byte DATA_REGISTER_OFFSET = 0x06;

    // Hardware Monitor Registers
    private const byte VOLTAGE_BASE_REG = 0x20;
    private const byte BANK_SELECT_REGISTER = 0x04E;
    private const byte VENDOR_ID_REGISTER = 0x4F;
    private const byte FIRST_BANK_REGISTER = 0x50;
    private const byte TEMPERATURE_BASE_REG = 0x50;
    private const byte TEMPERATURE_SYS_REG = 0x27;

    private byte[] FAN_TACHO_REG = new byte[] { 0x28, 0x29, 0x2A, 0x3F, 0x53 };
    private byte[] FAN_TACHO_BANK = new byte[] { 0, 0, 0, 0, 5 };    
    private string[] FAN_NAME = new string[] 
      { "System", "CPU #1", "Auxiliary #1", "CPU #2", "Auxiliary #2" };
    private byte[] FAN_BIT_REG = new byte[] { 0x47, 0x4B, 0x4C, 0x59, 0x5D };
    private byte[] FAN_DIV_BIT0 = new byte[] { 36, 38, 30, 8, 10 };
    private byte[] FAN_DIV_BIT1 = new byte[] { 37, 39, 31, 9, 11 };
    private byte[] FAN_DIV_BIT2 = new byte[] { 5, 6, 7, 23, 15 };

    private byte ReadByte(byte bank, byte register) {  
      WinRing0.WriteIoPortByte(
         (ushort)(address + ADDRESS_REGISTER_OFFSET), BANK_SELECT_REGISTER);
      WinRing0.WriteIoPortByte(
         (ushort)(address + DATA_REGISTER_OFFSET), bank);
      WinRing0.WriteIoPortByte(
         (ushort)(address + ADDRESS_REGISTER_OFFSET), register);
      return WinRing0.ReadIoPortByte(
        (ushort)(address + DATA_REGISTER_OFFSET));
    }    

    public W83627DHG(byte revision, ushort address) {      
      this.revision = revision;
      this.address = address;

      // Check vendor id
      ushort vendorId =
        (ushort)((ReadByte(HIGH_BYTE, VENDOR_ID_REGISTER) << 8) |
           ReadByte(0, VENDOR_ID_REGISTER));
      if (vendorId != WINBOND_VENDOR_ID)
        return;

      voltageGains = new float[] { 0.008f, 1, 1, 0.016f, 1, 1, 1, 0.016f };
      voltages = new Sensor[3];
      voltages[0] = new Sensor("CPU VCore", 0, SensorType.Voltage, this);
      voltages[1] = new Sensor("+3.3V", 3, SensorType.Voltage, this);
      voltages[2] = new Sensor("Battery", 7, SensorType.Voltage, this);

      temperatures = new Sensor[3];
      temperatures[0] = new Sensor("CPU", 0, SensorType.Temperature, this);
      temperatures[1] = new Sensor("Auxiliary", 1, SensorType.Temperature, this);
      temperatures[2] = new Sensor("System", 2, SensorType.Temperature, this);

      fans = new Sensor[FAN_NAME.Length];
      for (int i = 0; i < FAN_NAME.Length; i++)
        fans[i] = new Sensor(FAN_NAME[i], i, SensorType.Fan, this);

      this.name = "Winbond W83627DHG";
      this.icon = Utilities.EmbeddedResources.GetImage("chip.png");
      available = true;
    }

    public bool IsAvailable {
      get { return available; }
    }

    public string Name {
      get { return name; }
    }

    public string Identifier {
      get { return "/lpc/w83627dhg"; }
    }

    public Image Icon {
      get { return icon; }
    }

    public ISensor[] Sensors {
      get { return active.ToArray(); }
    }

    public string GetReport() {
      StringBuilder r = new StringBuilder();

      r.AppendLine("LPC W83627DHG");
      r.AppendLine();
      r.Append("Chip revision: 0x"); r.AppendLine(revision.ToString("X"));
      r.Append("Base Adress: 0x"); r.AppendLine(address.ToString("X4"));
      r.AppendLine();
      r.AppendLine("Hardware Monitor Registers");
      r.AppendLine();
      r.AppendLine("      00 01 02 03 04 05 06 07 08 09 0A 0B 0C 0D 0E 0F");
      r.AppendLine();
      for (int i = 0; i < 0x7; i++) {
        r.Append(" "); r.Append((i << 4).ToString("X2")); r.Append("  ");
        for (int j = 0; j <= 0xF; j++) {
          r.Append(" ");
          r.Append(ReadByte(0, (byte)((i << 4) | j)).ToString("X2"));
        }
        r.AppendLine();
      }      
      for (int k = 1; k <=5; k++) {
        r.AppendLine("Bank " + k);
        for (int i = 0x5; i < 0x6; i++) {
          r.Append(" "); r.Append((i << 4).ToString("X2")); r.Append("  ");
          for (int j = 0; j <= 0xF; j++) {
            r.Append(" ");
            r.Append(ReadByte((byte)(k), 
              (byte)((i << 4) | j)).ToString("X2"));
          }
          r.AppendLine();
        }        
      }
      r.AppendLine();

      return r.ToString();
    }

    public void Update() {
      foreach (Sensor sensor in voltages) {
        if (sensor.Index < 7) {
          int value = ReadByte(0, (byte)(VOLTAGE_BASE_REG + sensor.Index));
          sensor.Value = voltageGains[sensor.Index] * value;
          if (sensor.Value > 0)
            ActivateSensor(sensor);
          else
            DeactivateSensor(sensor);
        } else {
          // Battery voltage
          bool valid = (ReadByte(0, 0x5D) & 0x01) > 0;
          if (valid) {
            sensor.Value = voltageGains[sensor.Index] * 
              ReadByte(5, 0x51);
            ActivateSensor(sensor);
          } else
            DeactivateSensor(sensor);
        }
      }

      foreach (Sensor sensor in temperatures) {
        int value;
        if (sensor.Index < 2) {
          value = ReadByte((byte)(sensor.Index + 1), TEMPERATURE_BASE_REG);
          value = (value << 1) | ReadByte((byte)(sensor.Index + 1),
            (byte)(TEMPERATURE_BASE_REG + 1)) >> 7;
        } else {
          value = ReadByte(0, TEMPERATURE_SYS_REG) << 1;
        }
        sensor.Value = value / 2.0f;
        if (value < 0x1FE)
          ActivateSensor(sensor);
        else
          DeactivateSensor(sensor);
      }

      long bits = 0;
      for (int i = 0; i < FAN_BIT_REG.Length; i++)
        bits = (bits << 8) | ReadByte(0, FAN_BIT_REG[i]);
      foreach (Sensor sensor in fans) {
        int count = ReadByte(FAN_TACHO_BANK[sensor.Index], 
          FAN_TACHO_REG[sensor.Index]);
        int divisorBits = (int)(
          (((bits >> FAN_DIV_BIT2[sensor.Index]) & 1) << 2) |
          (((bits >> FAN_DIV_BIT1[sensor.Index]) & 1) << 1) |
           ((bits >> FAN_DIV_BIT0[sensor.Index]) & 1));
        int divisor = 1 << divisorBits;
        sensor.Value = (count < 0xff) ? 1.35e6f / (count * divisor) : 0;
        ActivateSensor(sensor);        
      }     
    }

    private void ActivateSensor(Sensor sensor) {
      if (!active.Contains(sensor)) {
        active.Add(sensor);
        SensorAdded(sensor);
      }
    }

    private void DeactivateSensor(Sensor sensor) {
      if (active.Contains(sensor)) {
        active.Remove(sensor);
        SensorRemoved(sensor);
      }
    }

    public event SensorEventHandler SensorAdded;
    public event SensorEventHandler SensorRemoved;
  }
}

/*
 
  This Source Code Form is subject to the terms of the Mozilla Public
  License, v. 2.0. If a copy of the MPL was not distributed with this
  file, You can obtain one at http://mozilla.org/MPL/2.0/.
 
  Copyright (C) 2009-2015 Michael Möller <mmoeller@openhardwaremonitor.org>
	Copyright (C) 2010 Paul Werelds
  Copyright (C) 2011 Roland Reinl <roland-reinl@gmx.de>
	
*/

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.Extensions.Logging;
using OpenHardwareMonitorLib;

namespace OpenHardwareMonitor.Hardware.HDD {
  internal abstract class AbstractStorage : Hardware {

    private const int UPDATE_DIVIDER = 5; // update only every 30s
    private const double BYTES_TO_GIGABYTES = 1.0 / (1024 * 1024 * 1024);
    private const double BYTES_TO_MEGABYTES = 1.0 / (1024 * 1024);

    protected string firmwareRevision;

    protected readonly int index;
    private int count;

    private DriveInfo[] driveInfos;
    private Sensor usageSensor;
    private List<(Sensor Sensor, double? Value)> performanceSensors;
    private DrivePerformanceValues lastPerformanceValues;
    private ISmart smart;

    protected AbstractStorage(string name, string firmwareRevision,
      string id, int index, ISettings settings)
      : base(name, new Identifier(id,
        index.ToString(CultureInfo.InvariantCulture)), settings) {
      this.firmwareRevision = firmwareRevision;

      this.index = index;
      this.count = 0;

      performanceSensors = new List<(Sensor, double? value)>();
      lastPerformanceValues = null;

      string[] logicalDrives = WindowsStorage.GetLogicalDrives(index);
      List<DriveInfo> driveInfoList = new List<DriveInfo>(logicalDrives.Length);
      foreach (string logicalDrive in logicalDrives) {
        try {
          DriveInfo di = new DriveInfo(logicalDrive);
          if (di.TotalSize > 0)
            driveInfoList.Add(new DriveInfo(logicalDrive));
        } catch (Exception x) when (x is ArgumentException || x is IOException || x is UnauthorizedAccessException) {
          Logger.LogError(x, $"Unable to obtain drive info for {logicalDrive}");
        }
      }
      driveInfos = driveInfoList.ToArray();

      smart = new WindowsSmart(index);
    }

    protected override void Dispose(bool disposing) {
      if (disposing) {
        if (smart != null) {
          smart.Dispose();
          smart = null;
        }
      }
      base.Dispose(disposing);
    }

    public static AbstractStorage CreateInstance(int driveNumber, NVMeGeneric previousNvMe, ISettings settings) {
      StorageInfo info = WindowsStorage.GetStorageInfo(driveNumber);
      if (info == null) {
        Logging.LogInfo($"Could not retrieve storage information for drive number {driveNumber}");
        return null;
      }

      bool alsoShowRemovables;
      if (!bool.TryParse(settings.GetValue("hddMenuItemRemovable", "true"), out alsoShowRemovables)) {
        alsoShowRemovables = true;
      }

      if (info.Removable && alsoShowRemovables) {
        return null;
      }
      AbstractStorage ret = null;
      if (info.BusType == StorageBusType.BusTypeNvme) {
        ret = NVMeGeneric.CreateInstance(info, previousNvMe, settings);
      }

      // If the disk uses Nvme, but does not support the required interfaces, we try Sata instead.
      if (ret == null && (info.BusType == StorageBusType.BusTypeAta || info.BusType == StorageBusType.BusTypeSata ||
                          info.BusType == StorageBusType.BusTypeNvme)) {
        ret = ATAStorage.CreateInstance(info, settings);
      }

      if (ret == null) {
        ret = StorageGeneric.CreateInstance(info, settings);
      }

      return ret;
    }

    protected virtual void CreateSensors() {
      if (driveInfos.Length > 0) {
        usageSensor =
          new Sensor("Used Space", 0, SensorType.Load, this, settings);
        ActivateSensor(usageSensor);
      }

      var performanceValues = smart.ReadThroughputValues();

      // Our sensor indices just need to be different from any existing sensors
      if (performanceValues != null) {
        var sensors = CreatePerformanceSensors(performanceValues);
        foreach (var elem in sensors) {
          ActivateSensor(elem.Item1);
        }

        performanceSensors = sensors;
      }
    }

    /// <summary>
    /// This method is used both for construction as well as updating the sensors. This avoids big if's on names
    /// </summary>
    private List<(Sensor, double? value)> CreatePerformanceSensors(DrivePerformanceValues throughputValues) {
      int idx = Sensors.Length + 1;
      List<(Sensor, double?)> newPerformanceSensors = new List<(Sensor, double?)>();

      TimeSpan deltaTime = default;
      if (lastPerformanceValues != null) {
        deltaTime = throughputValues.QueryTime - lastPerformanceValues.QueryTime;
      }

      Sensor s = new Sensor("Bytes read total", idx++, SensorType.Data, this, settings);
      double? v = throughputValues.BytesRead * BYTES_TO_GIGABYTES;
      newPerformanceSensors.Add((s, v));

      s = new Sensor("Bytes written total", idx++, SensorType.Data, this, settings);
      v = throughputValues.BytesWritten * BYTES_TO_GIGABYTES;
      newPerformanceSensors.Add((s, v));

      s = new Sensor("Read time total", idx++, SensorType.TimeSpan, this, settings);
      v = throughputValues.ReadTime.TotalSeconds;
      newPerformanceSensors.Add((s, v));

      s = new Sensor("Write time total", idx++, SensorType.TimeSpan, this, settings);
      v = throughputValues.WriteTime.TotalSeconds;
      newPerformanceSensors.Add((s, v));

      s = new Sensor("Idle time total", idx++, SensorType.TimeSpan, this, settings);
      v = throughputValues.IdleTime.TotalSeconds;
      newPerformanceSensors.Add((s, v));

      s = new Sensor("Read active time", idx++, SensorType.Load, this, settings);
      if (lastPerformanceValues != null) {
        TimeSpan valueDelta = throughputValues.ReadTime - lastPerformanceValues.ReadTime;
        v = valueDelta.TotalSeconds / deltaTime.TotalSeconds;
      } else {
        v = null;
      }

      newPerformanceSensors.Add((s, v));

      s = new Sensor("Write active time", idx++, SensorType.Load, this, settings);
      if (lastPerformanceValues != null) {
        TimeSpan valueDelta = throughputValues.WriteTime - lastPerformanceValues.WriteTime;
        v = valueDelta.TotalSeconds / deltaTime.TotalSeconds;
      } else {
        v = null;
      }

      newPerformanceSensors.Add((s, v));

      s = new Sensor("Job queue length", idx++, SensorType.RawValue, this, settings);
      v = throughputValues.QueueDepth;
      newPerformanceSensors.Add((s, v));

      s = new Sensor("Read throughput", idx++, SensorType.Throughput, this, settings);
      if (lastPerformanceValues != null) {
        double valueDelta = throughputValues.BytesRead - lastPerformanceValues.BytesRead;
        v = (valueDelta * BYTES_TO_MEGABYTES) / deltaTime.TotalSeconds;
      } else {
        v = null;
      }

      newPerformanceSensors.Add((s, v));

      s = new Sensor("Write throughput", idx++, SensorType.Throughput, this, settings);
      if (lastPerformanceValues != null) {
        double valueDelta = throughputValues.BytesWritten - lastPerformanceValues.BytesWritten;
        v = (valueDelta * BYTES_TO_MEGABYTES) / deltaTime.TotalSeconds;
      } else {
        v = null;
      }
      newPerformanceSensors.Add((s, v));

      lastPerformanceValues = throughputValues;

      return newPerformanceSensors;
    }

    public override HardwareType HardwareType {
      get { return HardwareType.HDD; }
    }

    protected virtual void UpdateSensors() {
      if (performanceSensors.Count > 0) {
        var newValues = smart.ReadThroughputValues();
        if (newValues != null) {
          var update = CreatePerformanceSensors(newValues);
          foreach (var s in performanceSensors) {
            var found = update.Single(x => x.Item1.Name == s.Sensor.Name);
            if (found.value.HasValue) {
              s.Sensor.Value = found.value;
            } else {
              s.Sensor.Value = null;
            }
          }
        }
      }
    }

    public override void Update() {
      if (count == 0) {
        UpdateSensors();

        if (usageSensor != null) {
          long totalSize = 0;
          long totalFreeSpace = 0;

          for (int i = 0; i < driveInfos.Length; i++) {
            if (!driveInfos[i].IsReady)
              continue;
            try {
              totalSize += driveInfos[i].TotalSize;
              totalFreeSpace += driveInfos[i].TotalFreeSpace;
            } catch (Exception x) when (x is IOException || x is UnauthorizedAccessException) {
              Logger.LogError($"Unable to read drive info for volume {driveInfos[i].Name}");
            }
          }
          if (totalSize > 0) {
            usageSensor.Value = 100.0f - (100.0f * totalFreeSpace) / totalSize;
          } else {
            usageSensor.Value = null;
          }
        }
      }

      count++;
      count %= UPDATE_DIVIDER;
    }

    protected abstract void GetReport(StringBuilder r);

    public override string GetReport() {
      StringBuilder r = new StringBuilder();

      r.AppendLine(this.GetType().Name);
      r.AppendLine();
      r.AppendLine("Drive name: " + name);
      r.AppendLine("Firmware version: " + firmwareRevision);
      r.AppendLine();

      GetReport(r);

      foreach (DriveInfo di in driveInfos) {
        if (!di.IsReady)
          continue;
        try {
          r.AppendLine("Logical drive name: " + di.Name);
          r.AppendLine("Format: " + di.DriveFormat);
          r.AppendLine("Total size: " + di.TotalSize);
          r.AppendLine("Total free space: " + di.TotalFreeSpace);
          r.AppendLine();
        } catch (IOException) { } catch (UnauthorizedAccessException) { }
      }

      return r.ToString();
    }

    public override void Traverse(IVisitor visitor) {
      foreach (ISensor sensor in Sensors)
        sensor.Accept(visitor);
    }
  }
}

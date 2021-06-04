using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenHardwareMonitor.Hardware.HDD {
  internal class DrivePerformanceValues {
    public DrivePerformanceValues(WindowsSmart.DISK_PERFORMANCE rawData) {
      BytesRead = rawData.BytesRead;
      BytesWritten = rawData.BytesWritten;
      ReadTime = TimeSpan.FromTicks(rawData.ReadTime); // Value is given in 100-nanosecond intervals, which is just the same as the timespan resolution
      WriteTime = TimeSpan.FromTicks(rawData.WriteTime);
      IdleTime = TimeSpan.FromTicks(rawData.IdleTime);
      ReadCount = rawData.ReadCount;
      WriteCount = rawData.WriteCount;
      QueueDepth = rawData.QueueDepth;
      SplitCount = rawData.SplitCount;
      QueryTime = new DateTime(rawData.QueryTime); // Ticks, but since when?
      StorageDeviceNumber = rawData.StorageDeviceNumber;
      StorageManagerName = Encoding.Unicode.GetString(rawData.StorageManagerName, 0, 16);
    }

    public long BytesRead {
      get;
    }

    public long BytesWritten {
      get;
    }

    public TimeSpan ReadTime {
      get;
    }

    public TimeSpan WriteTime {
      get;
    }

    public TimeSpan IdleTime {
      get;
    }

    public uint ReadCount {
      get;
    }

    public uint WriteCount {
      get;
    }

    public uint QueueDepth {
      get;
    }

    public uint SplitCount {
      get;
    }

    public DateTime QueryTime {
      get;
    }

    public uint StorageDeviceNumber {
      get;
    }

    public string StorageManagerName {
      get;
    }
  }
}

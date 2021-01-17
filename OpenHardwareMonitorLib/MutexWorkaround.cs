using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.AccessControl;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Win32.SafeHandles;

//.net 5 (core 5) does not have the open mutex with rights API method
//https://github.com/dotnet/runtime/issues/2117#issuecomment-577982767

namespace OpenHardwareMonitor {
  static class MutexWorkaround {
    [DllImport("kernel32", EntryPoint = "OpenMutexW", SetLastError = true, CharSet = CharSet.Unicode)]
    private static extern SafeWaitHandle OpenMutex(uint desiredAccess, bool inheritHandle, string name);

    public static bool TryOpenExisting(string name, MutexRights rights, out Mutex result) {
      SafeWaitHandle myHandle = OpenMutex((uint)rights, false, name);
      result = null;
      if (myHandle.IsInvalid) {
        return false;
      }
      result = new Mutex(initiallyOwned: false);
      SafeWaitHandle old = result.SafeWaitHandle;
      result.SafeWaitHandle = myHandle;
      old.Dispose();

      return true;
    }
  }
}

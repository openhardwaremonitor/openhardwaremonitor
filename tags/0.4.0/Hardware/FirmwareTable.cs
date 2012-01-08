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
  Portions created by the Initial Developer are Copyright (C) 2011
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

using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Text;

namespace OpenHardwareMonitor.Hardware {

  internal static class FirmwareTable {

    public static byte[] GetTable(Provider provider, string table) {
      int id = table[3] << 24 | table[2] << 16 | table[1] << 8 | table[0];
      return GetTable(provider, id);
    }

    public static byte[] GetTable(Provider provider, int table) {
      
      int size;
      try {
        size = NativeMethods.GetSystemFirmwareTable(provider, table, 
          IntPtr.Zero, 0);
      } catch (DllNotFoundException) { return null; } 
        catch (EntryPointNotFoundException) { return null; }

      if (size <= 0)
        return null;

      IntPtr nativeBuffer = Marshal.AllocHGlobal(size);
      NativeMethods.GetSystemFirmwareTable(provider, table, nativeBuffer, size);

      if (Marshal.GetLastWin32Error() != 0)
        return null;

      byte[] buffer = new byte[size];
      Marshal.Copy(nativeBuffer, buffer, 0, size);
      Marshal.FreeHGlobal(nativeBuffer);

      return buffer;
    }

    public static string[] EnumerateTables(Provider provider) {
      int size;
      try {
        size = NativeMethods.EnumSystemFirmwareTables(
          provider, IntPtr.Zero, 0);
      } catch (DllNotFoundException) { return null; } 
        catch (EntryPointNotFoundException) { return null; }

      IntPtr nativeBuffer = Marshal.AllocHGlobal(size);
      NativeMethods.EnumSystemFirmwareTables(
        provider, nativeBuffer, size);
      byte[] buffer = new byte[size];
      Marshal.Copy(nativeBuffer, buffer, 0, size);
      Marshal.FreeHGlobal(nativeBuffer);

      string[] result = new string[size / 4];
      for (int i = 0; i < result.Length; i++) 
        result[i] = Encoding.ASCII.GetString(buffer, 4 * i, 4);

      return result;
    }

    public enum Provider : int {
      ACPI = (byte)'A' << 24 | (byte)'C' << 16 | (byte)'P' << 8 | (byte)'I',
      FIRM = (byte)'F' << 24 | (byte)'I' << 16 | (byte)'R' << 8 | (byte)'M',
      RSMB = (byte)'R' << 24 | (byte)'S' << 16 | (byte)'M' << 8 | (byte)'B'
    }

    private static class NativeMethods {
      private const string KERNEL = "kernel32.dll";

      [DllImport(KERNEL, CallingConvention = CallingConvention.Winapi,
        SetLastError = true)]
      public static extern int EnumSystemFirmwareTables(
        Provider firmwareTableProviderSignature,
        IntPtr firmwareTableBuffer, int bufferSize);

      [DllImport(KERNEL, CallingConvention = CallingConvention.Winapi,
        SetLastError = true)]
      public static extern int GetSystemFirmwareTable(
        Provider firmwareTableProviderSignature,
        int firmwareTableID, IntPtr firmwareTableBuffer, int bufferSize);
    }
  }
}

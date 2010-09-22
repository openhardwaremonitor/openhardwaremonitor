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
  Portions created by the Initial Developer are Copyright (C) 2009-2010
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

namespace OpenHardwareMonitor.Hardware.CPU {

  internal abstract class AMDCPU : GenericCPU {

    private const byte PCI_BUS = 0;
    private const byte PCI_BASE_DEVICE = 24;
    private const byte DEVICE_VENDOR_ID_REGISTER = 0;
    private const ushort AMD_VENDOR_ID = 0x1022;

    public AMDCPU(int processorIndex, CPUID[][] cpuid, ISettings settings)
      : base(processorIndex, cpuid, settings) { }

    protected uint GetPciAddress(byte function, ushort deviceId) {
      uint address = WinRing0.GetPciAddress(PCI_BUS,
        (byte)(PCI_BASE_DEVICE + processorIndex), function);

      uint deviceVendor;
      if (!WinRing0.ReadPciConfigDwordEx(
        address, DEVICE_VENDOR_ID_REGISTER, out deviceVendor))
        return WinRing0.InvalidPciAddress;

      if (deviceVendor != (deviceId << 16 | AMD_VENDOR_ID))
        return WinRing0.InvalidPciAddress;

      return address;
    }

  }
}

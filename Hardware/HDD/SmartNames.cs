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
  Portions created by the Initial Developer are Copyright (C) 2011
  the Initial Developer. All Rights Reserved.

  Contributor(s):
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

namespace OpenHardwareMonitor.Hardware.HDD {
  /// <summary>
  /// Localization class for SMART attribute names.
  /// </summary>
  internal static class SmartNames {

    public static string AirflowTemperature {
      get { return "Airflow Temperature"; }
    }

    public static string Temperature {
      get { return "Temperature"; }
    }

    public static string RetiredBlockCount {
      get { return "Retired Block Count"; }
    }

    public static string ProgramFailCount {
      get { return "Program Fail Count"; }
    }

    public static string EraseFailCount {
      get { return "Erase Fail Count"; }
    }

    public static string UnexpectedPowerLossCount {
      get { return "Unexpected Power Loss Count"; }
    }

    public static string WearRangeDelta {
      get { return "Wear Range Delta"; }
    }

    public static string AlternativeProgramFailCount {
      get { return "Alternative Program Fail Count"; }
    }

    public static string AlternativeEraseFailCount {
      get { return "Alternative Erase Fail Count"; }
    }

    public static string UnrecoverableEcc {
      get { return "Unrecoverable ECC"; }
    }

    public static string ReallocationEventCount {
      get { return "Reallocation Event Count"; }
    }

    public static string RemainingLife {
      get { return "Remaining Life"; }
    }

    public static string AvailableReservedSpace {
      get { return "Available Reserved Space"; }
    }

    public static string CalibrationRetryCount {
      get { return "Calibration Retry Count"; }
    }

    public static string CommandTimeout {
      get { return "Command Timeout"; }
    }

    public static string CurrentPendingSectorCount {
      get { return "Current Pending Sector Count"; }
    }

    public static string DataAddressMarkErrors {
      get { return "Data Address Mark errors"; }
    }

    public static string DiskShift {
      get { return "Disk Shift"; }
    }

    public static string DriveTemperature {
      get { return "Drive Temperature"; }
    }

    public static string EmergencyRetractCycleCount {
      get { return "Emergency Retract Cycle Count"; }
    }

    public static string EndToEndError {
      get { return "End-to-End error"; }
    }

    public static string EnduranceRemaining {
      get { return "Endurance Remaining"; }
    }

    public static string FlyingHeight {
      get { return "Flying Height"; }
    }

    public static string FreeFallProtection {
      get { return "Free Fall Protection"; }
    }

    public static string GmrHeadAmplitude {
      get { return "GMR Head Amplitude"; }
    }

    public static string GSenseErrorRate {
      get { return "G-sense Error Rate"; }
    }

    public static string HardwareEccRecovered {
      get { return "Hardware ECC Recovered"; }
    }

    public static string HeadFlyingHours {
      get { return "Head Flying Hours"; }
    }

    public static string HeadStability {
      get { return "Head Stability"; }
    }

    public static string HighFlyWrites {
      get { return "High Fly Writes"; }
    }

    public static string InducedOpVibrationDetection {
      get { return "Induced Op-Vibration Detection"; }
    }

    public static string LoadedHours {
      get { return "Loaded Hours"; }
    }

    public static string LoadFriction {
      get { return "Load Friction"; }
    }

    public static string LoadInTime {
      get { return "Load 'In'-time"; }
    }

    public static string LoadUnloadCycleCount {
      get { return "Load/Unload Cycle Count"; }
    }

    public static string LoadUnloadCycleCountFujitsu {
      get { return "Load/Unload Cycle Count (Fujitus)"; }
    }

    public static string LoadUnloadRetryCount {
      get { return "Load/Unload Retry Count"; }
    }

    public static string MediaWearoutIndicator {
      get { return "Media Wearout Indicator"; }
    }

    public static string MultiZoneErrorRate {
      get { return "Multi-Zone Error Rate"; }
    }

    public static string OfflineSeekPerformance {
      get { return "Offline Seek Performance"; }
    }

    public static string PowerCycleCount {
      get { return "Power Cycle Count"; }
    }

    public static string PowerOffRetractCycle {
      get { return "Power-Off Retract Cycle"; }
    }

    public static string PowerOnHours {
      get { return "Power-On Hours (POH)"; }
    }

    public static string ReadChannelMargin {
      get { return "Read Channel Margin"; }
    }

    public static string ReadErrorRate {
      get { return "Read Error Rate"; }
    }

    public static string ReadErrorRetryRate {
      get { return "Read Error Retry Rate"; }
    }

    public static string ReallocatedSectorsCount {
      get { return "Reallocated Sectors Count"; }
    }

    public static string ReportedUncorrectableErrors {
      get { return "Reported Uncorrectable Errors"; }
    }

    public static string RunOutCancel {
      get { return "Run Out Cancel"; }
    }

    public static string SataDownshiftErrorCount {
      get { return "SATA Downshift Error Count"; }
    }

    public static string SeekErrorRate {
      get { return "Seek Error Rate"; }
    }

    public static string SeekTimePerformance {
      get { return "Seek Time Performance"; }
    }

    public static string ShockDuringWrite {
      get { return "Shock During Write"; }
    }

    public static string SoftEccCorrection {
      get { return "Soft ECC Correction"; }
    }

    public static string SoftReadErrorRate {
      get { return "Soft Read Error Rate"; }
    }

    public static string SpinBuzz {
      get { return "Spin Buzz"; }
    }

    public static string SpinHighCurrent {
      get { return "Spin High Current"; }
    }

    public static string SpinRetryCount {
      get { return "Spin Retry Count"; }
    }

    public static string SpinUpTime {
      get { return "Spin-Up Time"; }
    }

    public static string StartStopCount {
      get { return "Start/Stop Count"; }
    }

    public static string TaCounterDetected {
      get { return "TA Counter Detected"; }
    }

    public static string TemperatureDifferenceFrom100 {
      get { return "Temperature Difference from 100"; }
    }

    public static string ThermalAsperityRate {
      get { return "Thermal Asperity Rate (TAR)"; }
    }

    public static string ThroughputPerformance {
      get { return "Throughput Performance"; }
    }

    public static string TorqueAmplificationCount {
      get { return "Torque Amplification Count"; }
    }

    public static string TotalLbasRead {
      get { return "Total LBAs Read"; }
    }

    public static string TotalLbasWritten {
      get { return "Total LBAs Written"; }
    }

    public static string TransferErrorRate {
      get { return "Transfer Error Rate"; }
    }

    public static string UltraDmaCrcErrorCount {
      get { return "UltraDMA CRC Error Count"; }
    }

    public static string UncorrectableSectorCount {
      get { return "Uncorrectable Sector Count"; }
    }

    public static string Unknown {
      get { return "Unknown"; }
    }

    public static string VibrationDuringWrite {
      get { return "Vibration During Write"; }
    }

    public static string WriteErrorRate {
      get { return "Write Error Rate"; }
    }

    public static string RecalibrationRetries {
      get { return "Recalibration Retries"; }
    }

    public static string LoadCycleCount {
      get { return "Load Cycle Count"; }
    }

    public static string AlternativeGSenseErrorRate {
      get { return "Alternative G-Sense Error Rate"; }
    }

    public static string InitialBadBlockCount {
      get { return "Initial Bad Block Count"; }
    }

    public static string ProgramFailure {
      get { return "Program Failure"; }
    }

    public static string EraseFailure {
      get { return "Erase Failure"; }
    }

    public static string ReadFailure {
      get { return "Read Failure"; }
    }

    public static string SectorsRead {
      get { return "Sectors Read"; }
    }

    public static string SectorsWritten {
      get { return "Sectors Written"; }
    }

    public static string ReadCommands {
      get { return "Read Commands"; }
    }

    public static string WriteCommands {
      get { return "Write Commands"; }
    }

    public static string BitErrors {
      get { return "Bit Errors"; }
    }

    public static string CorrectedErrors {
      get { return "Corrected Errors"; }
    }

    public static string BadBlockFullFlag {
      get { return "Bad Block Full Flag"; }
    }

    public static string MaxCellCycles {
      get { return "Max Cell Cycles"; }
    }

    public static string MinErase {
      get { return "Min Erase"; }
    }

    public static string MaxErase {
      get { return "Max Erase"; }
    }

    public static string AverageEraseCount {
      get { return "Average Erase Count"; }
    }

    public static string UnknownUnique {
      get { return "Unknown Unique"; }
    }

    public static string SataErrorCountCrc {
      get { return "SATA Error Count CRC"; }
    }

    public static string SataErrorCountHandshake {
      get { return "SATA Error Count Handshake"; }
    }

    public static string UnsafeShutdownCount {
      get { return "Unsafe Shutdown Count"; }
    }

    public static string HostWrites {
      get { return "Host Writes"; }
    }

    public static string HostReads {
      get { return "Host Reads"; }
    }

    public static string MediaWearOutIndicator {
      get { return "Media Wear Out Indicator"; }
    }    

    public static string ProgramFailCountChip {
      get { return "Program Fail Count (Chip)"; }
    }

    public static string EraseFailCountChip {
      get { return "Erase Fail Count (Chip)"; }
    }

    public static string WearLevelingCount {
      get { return "Wear Leveling Count"; }
    }

    public static string UsedReservedBlockCountChip {
      get { return "Used Reserved Block Count (Chip)"; }
    }

    public static string UsedReservedBlockCountTotal {
      get { return "Used Reserved Block Count (Total)"; }
    }

    public static string ProgramFailCountTotal {
      get { return "Program Fail Count (Total)"; }
    }

    public static string EraseFailCountTotal {
      get { return "Erase Fail Count (Total)"; }
    }

    public static string RuntimeBadBlockTotal {
      get { return "Runtime Bad Block Total"; }
    }

    public static string UncorrectableErrorCount {
      get { return "Uncorrectable Error Count"; }
    }

    public static string TemperatureExceedCount {
      get { return "Temperature Exceed Count"; }
    }

    public static string ECCRate {
      get { return "ECC Rate"; }
    }

    public static string OffLineUncorrectableErrorCount {
      get { return "Off-Line Uncorrectable Error Count"; }
    }

    public static string CRCErrorCount {
      get { return "CRC Error Count"; }
    }

    public static string SupercapStatus {
      get { return "Supercap Status"; }
    }

    public static string ExceptionModeStatus {
      get { return "Exception Mode Status"; }
    }
  }
}
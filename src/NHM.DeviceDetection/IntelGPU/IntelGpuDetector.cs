using Newtonsoft.Json;
using NHM.Common;
using NHM.Common.Device;
using NHM.Common.Enums;
using NHM.UUID;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NHM.DeviceDetection.IntelGPU
{
    internal static class IntelGpuDetector
    {
        private const string Tag = "IntelGpuDetector";

        public static async Task<(string rawOutput, IntelGpuDeviceDetectionResult parsed)> TryQueryIGCLDevicesAsync()
        {
            var execStr = "igcl -n";
            Logger.Info(Tag, $"TryQueryIGCLDevicesAsync START {execStr}");
            var result = await DeviceDetectionPrinter.GetDeviceDetectionResultAsync<IntelGpuDeviceDetectionResult>(execStr, 30 * 1000);
            Logger.Info(Tag, $"TryQueryIGCLDevicesAsync END {execStr}");

            return result;
        }

        private static string convertSize(double size)
        {
            try
            {
                string[] units = new string[] { "B", "KB", "MB", "GB", "TB", "PB" };
                double mod = 1024.0;
                int i = 0;

                while (size >= mod)
                {
                    size /= mod;
                    i++;
                }
                var GBResult = Math.Round(size, 0);
                // if number is odd we can assume that free memory was presented and we can return the upper even...
                if (GBResult > 5.0 && GBResult % 2 != 0)//1,3,5gb gpus exist
                {
                    GBResult += 1;
                }
                return GBResult + units[i];
            }
            catch
            {
                return null;
            }
        }

        public static IntelDevice Transform(IntelGpuDeviceDetectionResult.Device igclDevice)
        {
            var uuid = "";
            // if no nvml loaded fallback ID
            if (string.IsNullOrEmpty(uuid))
            {
                var infoToHashed = $"{igclDevice.PciDeviceId}--{DeviceType.INTEL}--{igclDevice.DeviceName}--{igclDevice.PciBusID}";
                var uuidHEX = UUID.UUID.GetHexUUID(infoToHashed);
                uuid = $"GPU-{uuidHEX}";
            }
            var intelDev =  new IntelDevice
            {
                DeviceType = DeviceType.INTEL,
                UUID = uuid,
                Name = igclDevice.DeviceName,
                ID = igclDevice.PciDeviceId,
                PCIeBusID = igclDevice.PciBusID,
                GpuRam = igclDevice.DeviceMemory,
                RawDeviceData = JsonConvert.SerializeObject(igclDevice),
            };
            if (Version.TryParse(igclDevice.DriverVersion, out var parsedVer)) intelDev.DEVICE_INTEL_DRIVER = parsedVer;
            return intelDev;
        }

        public static void LogDevices(StringBuilder stringBuilder, IEnumerable<IntelDevice> devs)
        {
            if (devs == null || devs.Count() == 0)
            {
                var emptyStr = "\tADDED DEVICES ZERO";
                stringBuilder.AppendLine(emptyStr);
                return;
            }
            var supportedStr =  "ADDED";
            stringBuilder.AppendLine($"\t{supportedStr} devices:");
            foreach (var intelDev in devs)
            {
                stringBuilder.AppendLine($"\t\t--");
                stringBuilder.AppendLine($"\t\tUUID: {intelDev.UUID}");
                stringBuilder.AppendLine($"\t\tID: {intelDev.ID}");
                stringBuilder.AppendLine($"\t\tBusID: {intelDev.PCIeBusID}");
                stringBuilder.AppendLine($"\t\tNAME: {intelDev.Name}");
                stringBuilder.AppendLine($"\t\tMEMORY: {intelDev.GpuRam}");
                stringBuilder.AppendLine($"\t\tDRIVER: {intelDev.DEVICE_INTEL_DRIVER}");
            }
        }
    }
}

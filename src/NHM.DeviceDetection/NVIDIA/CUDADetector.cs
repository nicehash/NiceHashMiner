using NHM.Common;
using NHM.Common.Device;
using NHM.Common.Enums;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NHM.DeviceDetection.NVIDIA
{
    using Newtonsoft.Json;
    using NHM.UUID;

    internal static class CUDADetector
    {
        private const string Tag = "CUDADetector";

        public static async Task<(string rawOutput, CudaDeviceDetectionResult parsed)> TryQueryCUDADevicesAsync()
        {
            var execStr = "cuda -n";
            Logger.Info(Tag, $"TryQueryCUDADevicesAsync START {execStr}");
            var result = await DeviceDetectionPrinter.GetDeviceDetectionResultAsync<CudaDeviceDetectionResult>(execStr, 30 * 1000);
            Logger.Info(Tag, $"TryQueryCUDADevicesAsync END {execStr}");

            return result;
        }

        public static void LogDevices(StringBuilder stringBuilder, IEnumerable<CUDADevice> devs, bool supported)
        {
            if (devs == null || devs.Count() == 0)
            {
                var emptyStr = supported ? "\tSUPPORTED/ADDED DEVICES ZERO" : "\tNOT SUPPORTED/SKIPPED DEVICES ZERO";
                stringBuilder.AppendLine(emptyStr);
                return;
            }
            var supportedStr = supported ? "SUPPORTED AND ADDED" : "NOT SUPPORTED SKIPPING";
            stringBuilder.AppendLine($"\t{supportedStr} devices:");
            foreach (var cudaDev in devs)
            {
                stringBuilder.AppendLine($"\t\t--");
                stringBuilder.AppendLine($"\t\tUUID: {cudaDev.UUID}");
                stringBuilder.AppendLine($"\t\tID: {cudaDev.ID}");
                stringBuilder.AppendLine($"\t\tBusID: {cudaDev.PCIeBusID}");
                stringBuilder.AppendLine($"\t\tNAME: {cudaDev.Name}");
                stringBuilder.AppendLine($"\t\tSM: {cudaDev.SM_major}.{cudaDev.SM_minor}");
                stringBuilder.AppendLine($"\t\tMEMORY: {cudaDev.GpuRam}");
            }
        }

        private static string GetNameFromCudaDevice(CudaDeviceDetectionResult.Device cudaDevice)
        {
            if (cudaDevice.VendorName == "UNKNOWN")
            {
                return $"V_ID_{cudaDevice.VendorID} {cudaDevice.DeviceName}";
            }
            return $"{cudaDevice.VendorName} {cudaDevice.DeviceName}";
        }

        public static CUDADevice Transform(CudaDeviceDetectionResult.Device cudaDevice)
        {
            string uuid = cudaDevice.UUID;
            // if no nvml loaded fallback ID
            if (string.IsNullOrEmpty(uuid))
            {
                var infoToHashed = $"{cudaDevice.DeviceID}--{DeviceType.NVIDIA}--{cudaDevice.DeviceGlobalMemory}--{cudaDevice.SM_major}--{cudaDevice.SM_minor}--{cudaDevice.DeviceName}--{cudaDevice.pciBusID}";
                var uuidHEX = UUID.GetHexUUID(infoToHashed);
                uuid = $"NVF-{uuidHEX}"; // TODO indicate NVF as NVIDIA Fallback 
            }
            var name = GetNameFromCudaDevice(cudaDevice);
            var bd = new BaseDevice(DeviceType.NVIDIA, uuid, name, (int)cudaDevice.DeviceID);
            var isLHR = IsLHR(name, (int)cudaDevice.pciDeviceId);
            var ret = new CUDADevice(bd, cudaDevice.pciBusID, cudaDevice.DeviceGlobalMemory, cudaDevice.SM_major, cudaDevice.SM_minor, isLHR);
            ret.RawDeviceData = JsonConvert.SerializeObject(cudaDevice);
            return ret;
        }

        private static bool IsLHR(string name, int deviceId)
        {
            var nonLHR_GPUs = new (string name, int pciDeviceID)[] { ("GeForce RTX 3060", 9475), ("GeForce RTX 3060 Ti", 9350), ("GeForce RTX 3070", 9348), ("GeForce RTX 3080", 8710), ("GeForce RTX 3090", 8708) };
            return nonLHR_GPUs.Any(gpu => name.Contains(gpu.name) && (deviceId >> 16) != gpu.pciDeviceID);
        }
    }
}

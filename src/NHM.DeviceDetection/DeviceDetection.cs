using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NHM.DeviceDetection.NVIDIA;
using NiceHashMinerLegacy.Common;
using NiceHashMinerLegacy.Common.Device;

namespace NHM.DeviceDetection
{
    public static class DeviceDetection
    {
        private static readonly NvidiaSmiDriver NvidiaMinDetectionDriver = new NvidiaSmiDriver(362, 61); // 362.61;
        private static string Tag => "DeviceDetection";
        private static bool _initCalled = false;

        public static DeviceDetectionResult DetectionResult { get; } = new DeviceDetectionResult();


        private static async Task DetectCPU()
        {
            Logger.Info(Tag, $"DetectCPU START");
            var cpu = await CPU.CPUDetector.TryQueryCPUDeviceTask();
            DetectionResult.CPU = cpu;
            if (cpu == null)
            {
                Logger.Info(Tag, $"Found No Compatible CPU");
            }
            else
            {
                var stringBuilder = new StringBuilder();
                stringBuilder.AppendLine($"Found CPU:");
                stringBuilder.AppendLine($"\tUUID: {cpu.UUID}");
                stringBuilder.AppendLine($"\tName: {cpu.Name}");
                stringBuilder.AppendLine($"\tPhysicalProcessorCount: {cpu.PhysicalProcessorCount}");
                stringBuilder.AppendLine($"\tThreadsPerCPU: {cpu.ThreadsPerCPU}");
                stringBuilder.AppendLine($"\tSupportsHyperThreading: {cpu.SupportsHyperThreading}");
                Logger.Info(Tag, stringBuilder.ToString());
            }
            Logger.Info(Tag, $"DetectCPU END");
        }

        private static async Task DetectWMIVideoControllers()
        {
            var vidControllers = await WMI.VideoControllerDetector.QueryWin32_VideoControllerTask();
            var busDevices = await WMI.DeviceBusDetector.QueryWin32_DeviceBusPCITask();

            // cross reference to get bus ids
            foreach (var vidCtr in vidControllers)
            {
                var compareVidCtr = WMI.PnpDeviceIDHelper.ToCompareFormat(vidCtr.PnpDeviceID);
                foreach (var busData in busDevices)
                {
                    var compareBusData = WMI.PnpDeviceIDHelper.ToCompareFormat(busData.Dependent);
                    if (compareBusData.Contains(compareVidCtr))
                    {
                        vidCtr.PCI_BUS_ID = busData.GetPCIBusID();
                    }
                }
            }

            DetectionResult.AvailableVideoControllers = vidControllers;
            // check NVIDIA drivers, we assume all NVIDIA devices are using the same driver version
            var nvidiaVideoControllerData = vidControllers.Where(vidC => vidC.IsNvidia).FirstOrDefault();
            if (nvidiaVideoControllerData != null)
            {
                var wmiNvidiaVer = WMI.WmiNvidiaDriverParser.ParseNvSmiDriver(nvidiaVideoControllerData.DriverVersion);
                if (wmiNvidiaVer.IsValid()) DetectionResult.NvidiaDriverWMI = wmiNvidiaVer.ToVersion();
            }
            // log result
            var stringBuilder = new StringBuilder();
            var driverWmiString = DetectionResult.NvidiaDriverWMI == null ? "N/A" : $"{DetectionResult.NvidiaDriverWMI.Major}.{DetectionResult.NvidiaDriverWMI.Minor}";
            stringBuilder.AppendLine($"NVIDIA driver version: {driverWmiString}");
            stringBuilder.AppendLine("QueryVideoControllers: ");
            foreach (var vidController in vidControllers)
            {
                stringBuilder.AppendLine("\tWin32_VideoController detected:");
                stringBuilder.AppendLine($"{vidController.GetFormattedString()}");
            }
            Logger.Info(Tag, stringBuilder.ToString());
        }

        private static async Task DetectCUDADevices()
        {
            var cudaQueryResult = await CUDADetector.TryQueryCUDADevicesAsync(UseNvmlFallback.Enabled);
            Logger.Info(Tag, $"TryQueryCUDADevicesAsync RAW: '{cudaQueryResult.rawOutput}'");
            var result = cudaQueryResult.parsed;
            var unsuported = new List<CUDADevice>();
            if (result?.CudaDevices?.Count > 0)
            {
                // we got NVIDIA devices
                var cudaDevices = result.CudaDevices.Select(dev => CUDADetector.Transform(dev)).ToList();
                // filter out no supported SM versions
                // SM 3.0+ 
                DetectionResult.CUDADevices = cudaDevices.Where(cudaDev => cudaDev.SM_major >= 3).ToList();
                unsuported = cudaDevices.Where(cudaDev => cudaDev.SM_major < 3).ToList();
                // NVIDIA drivers
                var nvmlLoaded = result?.NvmlLoaded ?? false;
                DetectionResult.IsDCHDriver = nvmlLoaded == false;
                DetectionResult.IsNvmlFallback = result?.NvmlLoadedFallback ?? false;
                if (nvmlLoaded)
                {
                    var driverVer = result.DriverVersion.Split('.').Select(s => int.Parse(s)).ToArray();
                    if (driverVer.Count() >= 2) DetectionResult.NvidiaDriver = new Version(driverVer[0], driverVer[1]);
                }
            }

            // set NVIDIA driver version here
            if (DetectionResult.NvidiaDriver != null)
            {
                // from cuda detect
                Logger.Info(Tag, "DetectCUDADevices Setting Nvidia Driver");
                CUDADevice.INSTALLED_NVIDIA_DRIVERS = DetectionResult.NvidiaDriver;
            }
            else if (DetectionResult.NvidiaDriverWMI != null)
            {
                // from WMI
                Logger.Info(Tag, "DetectCUDADevices Setting Nvidia Driver Fallback (WMI)");
                CUDADevice.INSTALLED_NVIDIA_DRIVERS = DetectionResult.NvidiaDriverWMI;
            }

            // log result
            var stringBuilder = new StringBuilder();
            stringBuilder.AppendLine("");
            stringBuilder.AppendLine("DetectCUDADevices:");
            CUDADetector.LogDevices(stringBuilder, unsuported, false);
            CUDADetector.LogDevices(stringBuilder, DetectionResult.CUDADevices, true);
            Logger.Info(Tag, stringBuilder.ToString());
        }

        private static async Task DetectAMDDevices()
        {
            DetectionResult.AMDDevices = await AMD.AMDDetector.TryQueryAMDDevicesAsync(DetectionResult.AvailableVideoControllers.ToList());
            if (DetectionResult.AMDDevices == null || DetectionResult.AMDDevices.Count == 0)
            {
                Logger.Info(Tag, "DetectAMDDevices ZERO Found.");
                return;
            }
            // log result
            var stringBuilder = new StringBuilder();
            stringBuilder.AppendLine("");
            stringBuilder.AppendLine("DetectAMDDevices:");
            foreach (var amdDev in DetectionResult.AMDDevices)
            {
                stringBuilder.AppendLine($"\t\t--");
                stringBuilder.AppendLine($"\t\tUUID: {amdDev.UUID}");
                stringBuilder.AppendLine($"\t\tID: {amdDev.ID}");
                stringBuilder.AppendLine($"\t\tBusID: {amdDev.PCIeBusID}");
                stringBuilder.AppendLine($"\t\tNAME: {amdDev.Name}");
                stringBuilder.AppendLine($"\t\tCodename: {amdDev.Codename}");
                stringBuilder.AppendLine($"\t\tInfSection: {amdDev.InfSection}");
                stringBuilder.AppendLine($"\t\tMEMORY: {amdDev.GpuRam}");
                stringBuilder.AppendLine($"\t\tOpenCLPlatformID: {amdDev.OpenCLPlatformID}");
            }
            Logger.Info(Tag, stringBuilder.ToString());
        }

        public static async Task DetectDevices(IProgress<DeviceDetectionStep> progress)
        {
            if (_initCalled) return;
            _initCalled = true;
            progress?.Report(DeviceDetectionStep.CPU);
            await DetectCPU();
            progress?.Report(DeviceDetectionStep.WMIWMIVideoControllers);
            await DetectWMIVideoControllers();
            progress?.Report(DeviceDetectionStep.NVIDIA_CUDA);
            await DetectCUDADevices();
            progress?.Report(DeviceDetectionStep.AMD_OpenCL);
            await DetectAMDDevices();
        }

        public static IEnumerable<BaseDevice> GetDetectedDevices()
        {
            // CPU
            if (DetectionResult.CPU != null) yield return DetectionResult.CPU;
            // NVIDIA
            if (DetectionResult.CUDADevices != null)
            {
                foreach (var cudaDev in DetectionResult.CUDADevices)
                {
                    yield return cudaDev;
                }
            }
            // AMD
            if (DetectionResult.AMDDevices != null)
            {
                foreach (var amdDev in DetectionResult.AMDDevices)
                {
                    yield return amdDev;
                }
            }
            // simulate no devices
            //return Enumerable.Empty<BaseDevice>();
        }


        public static async Task<int> CUDADevicesNumCheck()
        {
            var cudaQueryResult = await CUDADetector.TryQueryCUDADevicesAsync(UseNvmlFallback.Enabled);
            var result = cudaQueryResult.parsed;
            return result?.CudaDevices?.Count ?? 0;
        }
    }
}

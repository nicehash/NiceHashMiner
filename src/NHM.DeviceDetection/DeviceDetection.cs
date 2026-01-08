using Newtonsoft.Json;
using NHM.Common;
using NHM.Common.Configs;
using NHM.Common.Device;
using NHM.Common.Enums;
using NHM.DeviceDetection.IntelGPU;
using NHM.DeviceDetection.NVIDIA;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NHM.DeviceDetection
{
    public static class DeviceDetection
    {
        // NvidiaMinDetectionDriver 362.61 => CUDA 8.0
        private static string Tag => "DeviceDetection";
        private static bool _initCalled = false;

        public static DeviceDetectionResult DetectionResult { get; } = new DeviceDetectionResult();

        private class DeviceDetectionSettings : IInternalSetting
        {
            public bool UseUserSettings { get; set; } = false;
            public bool FakeDevices { get; set; } = false;
            public List<FakeDevice> Devices { get; set; } = new List<FakeDevice>
            {
                new FakeDevice
                {
                    DeviceType = DeviceType.CPU,
                    UUID = "FCPU-d0e3cc7b-9455-4386-9d7c-154754ae577e",
                    Name = "Fake CPU",
                    ID = 0,
                },
                new FakeDevice
                {
                    DeviceType = DeviceType.NVIDIA,
                    UUID = "FGPU-75555d30-b049-4e10-8add-eff796028b14",
                    Name = "Fake NVIDIA",
                    ID = 0,
                },
                new FakeDevice
                {
                    DeviceType = DeviceType.NVIDIA,
                    UUID = "FGPU-da69753a-4d27-4ab2-95ba-6504be9e8a9a",
                    Name = "Fake NVIDIA",
                    ID = 1,
                },
                new FakeDevice
                {
                    DeviceType = DeviceType.AMD,
                    UUID = "FAMD-bbc36d15-61db-4342-bb0d-2c97c62fe387",
                    Name = "Fake AMD",
                    ID = 0,
                },
                new FakeDevice
                {
                    DeviceType = DeviceType.AMD,
                    UUID = "FAMD-7c779e99-4b58-47e1-825c-0a4d6c01a70d",
                    Name = "Fake AMD",
                    ID = 1,
                },
            };
        }

        private static readonly DeviceDetectionSettings Settings;
        static DeviceDetection()
        {
            (Settings, _) = InternalConfigs.GetDefaultOrFileSettings(Paths.InternalsPath("devices.json"), new DeviceDetectionSettings { });
        }


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

        internal static async Task DetectWMIVideoControllers()
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

            DetectionResult.AvailableVideoControllers = vidControllers.ToList();
            // check NVIDIA drivers, we assume all NVIDIA devices are using the same driver version
            var nvidiaVideoControllerData = vidControllers.Where(vidC => vidC.IsNvidia).FirstOrDefault();
            if (nvidiaVideoControllerData != null)
            {
                var wmiNvidiaVer = WMI.WmiNvidiaDriverParser.ParseNvSmiDriver(nvidiaVideoControllerData.DriverVersion);
                if (wmiNvidiaVer.IsValid) DetectionResult.NvidiaDriverWMI = wmiNvidiaVer.Version;
            }

            // log result
            var stringBuilder = new StringBuilder();
            var driverWmiStringNVIDIA = DetectionResult.NvidiaDriverWMI == null ? "N/A" : $"{DetectionResult.NvidiaDriverWMI.Major}.{DetectionResult.NvidiaDriverWMI.Minor}";
            var driverWDSStringAMD = DetectionResult.AmdDriver == null ? "N/A" : $"{DetectionResult.AmdDriver.Major}.{DetectionResult.AmdDriver.Minor}.{DetectionResult.AmdDriver.Build}.{DetectionResult.AmdDriver.Revision}";
            stringBuilder.AppendLine($"NVIDIA driver version: {driverWmiStringNVIDIA}");
            stringBuilder.AppendLine($"AMD driver version: {driverWDSStringAMD}");
            stringBuilder.AppendLine("QueryVideoControllers: ");
            foreach (var vidController in vidControllers)
            {
                stringBuilder.AppendLine("\tWin32_VideoController detected:");
                stringBuilder.AppendLine($"{vidController}");
            }
            Logger.Info(Tag, stringBuilder.ToString());
        }

        private static bool IsCUDADeviceSupported(CUDADevice cudaDev)
        {
            if (cudaDev == null) return false;
            // SM 5.2+ 
            return cudaDev.SM_major >= 6 || (cudaDev.SM_major == 5 && cudaDev.SM_minor > 0);
        }

        private static async Task DetectCUDADevices()
        {
            var cudaQueryResult = await CUDADetector.TryQueryCUDADevicesAsync();
            CUDADevice.RawDetectionOutput = cudaQueryResult.rawOutput;
            Logger.Info(Tag, $"TryQueryCUDADevicesAsync RAW: '{cudaQueryResult.rawOutput}'");
            var result = cudaQueryResult.parsed;
            if (result?.CudaDevices?.Count > 0)
            {
                // we got NVIDIA devices
                var cudaDevices = result.CudaDevices.Select(dev => CUDADetector.Transform(dev)).ToList();
                // filter out no supported SM versions
                DetectionResult.CUDADevices = cudaDevices.Where(IsCUDADeviceSupported).OrderBy(cudaDev => cudaDev.PCIeBusID).ToList();
                DetectionResult.UnsupportedCUDADevices = cudaDevices.Where(cudaDev => IsCUDADeviceSupported(cudaDev) == false).ToList();
                // NVIDIA drivers
                var nvmlLoaded = result?.NvmlLoaded ?? -1;
                DetectionResult.IsNvidiaNVMLLoadedError = nvmlLoaded == -1;
                DetectionResult.IsNvidiaNVMLInitializedError = result?.NvmlInitialized != 0;
                var smiVersion = NvidiaSmiDriver.ToNvidiaSmiDriver(result.DriverVersion);
                if (smiVersion.IsValid) DetectionResult.NvidiaDriver = smiVersion.Version;
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
            CUDADetector.LogDevices(stringBuilder, DetectionResult.UnsupportedCUDADevices, false);
            CUDADetector.LogDevices(stringBuilder, DetectionResult.CUDADevices, true);
            Logger.Info(Tag, stringBuilder.ToString());
        }

        private static async Task DetectAMDDevices(bool detectIntegrated)
        {
            var amdDevices = await AMD.AMDDetector.TryQueryAMDDevicesAsync(DetectionResult.AvailableVideoControllers.ToList(), detectIntegrated);
            if (amdDevices == null || amdDevices.Count == 0)
            {
                Logger.Info(Tag, "DetectAMDDevices ZERO Found.");
                return;
            }
            DetectionResult.AMDDevices = amdDevices;
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
                stringBuilder.AppendLine($"\t\tIsIntegrated: {amdDev.IsIntegrated}");
            }
            Logger.Info(Tag, stringBuilder.ToString());
        }

        private static async Task DetectIntelGPUs(bool detectIntegrated)
        {
            var intelDevices = await IntelGpuDetector.TryQueryIGCLDevicesAsync();
            var result = intelDevices.parsed;
            if (result?.IgclDevices?.Count > 0)
            {
                // we got INTEL devices
                List<IntelDevice> igclDevices = new List<IntelDevice>();
                if (detectIntegrated)
                {
                    igclDevices = result.IgclDevices
                        .Where(dev => !dev.IsIntegrated)
                        .Select(dev => IntelGpuDetector.Transform(dev)).ToList();
                }
                else igclDevices = result.IgclDevices.Select(dev => IntelGpuDetector.Transform(dev)).ToList();
                // filter out no supported SM versions

                DetectionResult.IntelDevices = igclDevices.OrderBy(igclDev => igclDev.PCIeBusID).ToList();
                // INTEL drivers
                var igclLoaded = result?.IgclLoaded ?? -1;
            }

            // log result
            var stringBuilder = new StringBuilder();
            stringBuilder.AppendLine("");
            stringBuilder.AppendLine("DetectIntelGPUDevices:");
            IntelGpuDetector.LogDevices(stringBuilder, DetectionResult.IntelDevices);
            Logger.Info(Tag, stringBuilder.ToString());
        }

        private static void DetectFAKE_Devices()
        {
            DetectionResult.FAKEDevices = Settings.Devices;
        }

        public static async Task DetectDevices(bool detectIntegrated, IProgress<DeviceDetectionStep> progress)
        {
            if (_initCalled) return;
            _initCalled = true;
            progress?.Report(DeviceDetectionStep.CPU);
            if (!Settings.FakeDevices) await DetectCPU();
            progress?.Report(DeviceDetectionStep.WMIVideoControllers);
            if (!Settings.FakeDevices) await DetectWMIVideoControllers();
            progress?.Report(DeviceDetectionStep.NVIDIA_CUDA);
            if (!Settings.FakeDevices) await DetectCUDADevices();
            progress?.Report(DeviceDetectionStep.AMD_OpenCL);
            if (!Settings.FakeDevices) await DetectAMDDevices(detectIntegrated);
            progress?.Report(DeviceDetectionStep.INTEL_GPU);
            if (!Settings.FakeDevices) await DetectIntelGPUs(detectIntegrated);
            progress?.Report(DeviceDetectionStep.FAKE);
            if (Settings.FakeDevices) DetectFAKE_Devices();
            // after we detect AMD we will have platforms and now we can check if NVIDIA OpenCL backend works
            if (DetectionResult.CUDADevices?.Count > 0 && AMD.AMDDetector.Platforms?.Count > 0)
            {
                var nvidiaOpenCL = AMD.AMDDetector.Platforms
                    .Where(p => p.PlatformName.Contains("NVIDIA") || p.PlatformVendor.Contains("NVIDIA"))
                    .SelectMany(p => p.Devices);
                // now check devices
                foreach (var cudaDev in DetectionResult.CUDADevices)
                {
                    var hasOpenCLBackend = nvidiaOpenCL.Any(dev => dev.BUS_ID == cudaDev.PCIeBusID);
                    cudaDev.SetIsOpenCLBackendEnabled(hasOpenCLBackend);
                }
            }
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
            // INTEL
            if (DetectionResult.IntelDevices != null)
            {
                foreach (var  intelDev in DetectionResult.IntelDevices)
                {
                    yield return intelDev;
                }
            }
            // FAKE
            if (DetectionResult.FAKEDevices != null)
            {
                foreach (var fakeDev in DetectionResult.FAKEDevices)
                {
                    yield return fakeDev;
                }
            }
            // simulate no devices
            //return Enumerable.Empty<BaseDevice>();
        }

        // We check only missing from inital detection. Case like device poping back is not covered (ultra rare case)
        public static async Task<(bool isMissing, List<string> uuids)> CheckIfMissingGPUs(bool detectIntegrated)
        {
            async Task<(bool isMissing, List<string> uuids)> anyMissingCUDA_Devices()
            {
                try
                {
                    if (!DetectionResult.HasCUDADevices) return (false, new List<string>());
                    var cudaQueryResult = await CUDADetector.TryQueryCUDADevicesAsync();
                    var supportedCudaDevices = cudaQueryResult.parsed.CudaDevices
                        .Select(CUDADetector.Transform)
                        .Where(IsCUDADeviceSupported)
                        .Select(dev => dev.UUID)
                        .ToArray();
                    var missing = DetectionResult.CUDADevices.Where(detected => !supportedCudaDevices.Contains(detected.UUID));
                    if (missing.Any())
                    {
                        Logger.Error(Tag, $"CUDA missing devices:\n{string.Join("\n", missing.Select(dev => $"\t{dev.UUID}"))}");
                        return (true, missing.Select(dev => dev.UUID).ToList());
                    }
                }
                catch (Exception e)
                {
                    Logger.Error(Tag, $"CUDA CheckIfMissingDevices error: {e}");
                }
                return (false, new List<string>());
            }
            async Task<(bool isMissing, List<string> uuids)> anyMissingAMD_Devices()
            {
                try
                {
                    if (!DetectionResult.HasAMDDevices) return (false, new List<string>());
                    var amdDevices = await AMD.AMDDetector.TryQueryAMDDevicesAsync(DetectionResult.AvailableVideoControllers.ToList(), detectIntegrated);
                    var amdDevicesUUIDs = amdDevices
                        .Select(dev => dev.UUID)
                        .ToArray();
                    var missing = DetectionResult.AMDDevices.Where(detected => !amdDevicesUUIDs.Contains(detected.UUID));
                    if (missing.Any())
                    {
                        Logger.Error(Tag, $"AMD missing devices:\n{string.Join("\n", missing.Select(dev => $"\t{dev.UUID}"))}");
                        return (true, missing.Select(dev => dev.UUID).ToList());
                    }
                }
                catch (Exception e)
                {
                    Logger.Error(Tag, $"AMD CheckIfMissingDevices error: {e}");
                }
                return (false, new List<string>());
            }
            async Task<(bool isMissing, List<string> uuids)> anyMissingIntel_Devices()
            {
                try
                {
                    if (!DetectionResult.HasIntelDevices) return (false, new List<string>());
                    var intelDevices = await IntelGpuDetector.TryQueryIGCLDevicesAsync();
                    var intelDevicesUUIDs = intelDevices.parsed.IgclDevices
                        .Select(IntelGpuDetector.Transform)
                        .Select(dev => dev.UUID)
                        .ToArray();
                    var missing = DetectionResult.IntelDevices.Where(detected => !intelDevicesUUIDs.Contains(detected.UUID));
                    if (missing.Any())
                    {
                        Logger.Error(Tag, $"IntelGPU missing devices:\n{string.Join("\n", missing.Select(dev => $"\t{dev.UUID}"))}");
                        return (true, missing.Select(dev => dev.UUID).ToList());
                    }
                }
                catch (Exception e)
                {
                    Logger.Error(Tag, $"AMD CheckIfMissingDevices error: {e}");
                }
                return (false, new List<string>());
            }
            var amdsMissing = await anyMissingAMD_Devices();
            var nvsMissing = await anyMissingCUDA_Devices();
            var intelMissing = await anyMissingIntel_Devices();
            if(!amdsMissing.isMissing && !nvsMissing.isMissing && !intelMissing.isMissing)
            {
                return (false, new List<string>());
            }
            return (true, nvsMissing.uuids.Concat(amdsMissing.uuids).Concat(intelMissing.uuids).ToList());
        }
    }
}

using NHM.Common;
using NHM.Common.Device;
using NHM.Common.Enums;
using NHM.DeviceDetection.OpenCL;
using NHM.DeviceDetection.OpenCL.Models;
using NHM.DeviceDetection.WMI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NHM.DeviceDetection.AMD
{
    using NHM.UUID;
    internal static class AMDDetector
    {
        private const string Tag = "AMDDetector";
        public static bool IsOpenClFallback { get; private set; }

        // Ok so this is kinda stupid and probably should be split into OpenCL class but since we currently QueryOpenCLDevices inside AMD we will check for NVIDIA OpenCL backend only after AMD Query is finished
        public static List<OpenCLPlatform> Platforms { get; private set; } = null;

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
                return Math.Round(size, 1) + units[i];
            }
            catch
            {
                return null;
            }
        }

        public static async Task<List<AMDDevice>> TryQueryAMDDevicesAsync(List<VideoControllerData> availableVideoControllers)
        {
            var amdDevices = new List<AMDDevice>();
            Logger.Info(Tag, "TryQueryAMDDevicesAsync START");
            var openCLResult = await OpenCLDetector.TryQueryOpenCLDevicesAsync();
            Logger.Info(Tag, $"TryQueryOpenCLDevicesAsync RAW: '{openCLResult.rawOutput}'");


            OpenCLDeviceDetectionResult result = null;
            // check if we have duplicated platforms
            Logger.Info(Tag, "Checking duplicate devices...");
            if (DuplicatedDevices(openCLResult.parsed))
            {
                Logger.Info(Tag, "Found duplicate devices. Trying fallback detection");
                var openCLResult2 = await OpenCLDetector.TryQueryOpenCLDevicesAsyncFallback();
                Logger.Info(Tag, $"TryQueryOpenCLDevicesAsyncFallback RAW: '{openCLResult2.rawOutput}'");
                IsOpenClFallback = true;
                if (DuplicatedDevices(openCLResult2.parsed))
                {
                    Logger.Info(Tag, $"TryQueryOpenCLDevicesAsyncFallback has duplicate files as well... Taking filtering lower platform devices");
                    // #3 try merging both results and take lower platform unique devices
                    result = MergeResults(openCLResult.parsed, openCLResult2.parsed);
                }
                else
                {
                    // #2 try good
                    result = openCLResult2.parsed;
                }
            }
            else
            {
                // #1 try good
                result = openCLResult.parsed;
            }

            if (result?.Platforms?.Count > 0)
            {
                Platforms = result.Platforms;
                var amdPlatforms = result.Platforms.Where(platform => IsAMDPlatform(platform)).ToList();
                foreach (var platform in amdPlatforms)
                {
                    var platformNum = platform.PlatformNum;
                    foreach (var oclDev in platform.Devices)
                    {
                        var infSection = "";
                        var name = oclDev._CL_DEVICE_BOARD_NAME_AMD;
                        var codename = oclDev._CL_DEVICE_NAME;
                        var gpuRAM = oclDev._CL_DEVICE_GLOBAL_MEM_SIZE;
                        var infoToHashed = $"{oclDev.DeviceID}--{DeviceType.AMD}--{gpuRAM}--{codename}--{name}";
                        // cross ref info from vid controllers with bus id
                        var vidCtrl = availableVideoControllers?.Where(vid => vid.PCI_BUS_ID == oclDev.BUS_ID).FirstOrDefault() ?? null;
                        if (vidCtrl != null)
                        {
                            infSection = vidCtrl.InfSection;
                            infoToHashed += vidCtrl.PnpDeviceID;
                        }
                        else
                        {
                            Logger.Info(Tag, $"TryQueryAMDDevicesAsync cannot find VideoControllerData with bus ID {oclDev.BUS_ID}");
                        }
                        var uuidHEX = UUID.GetHexUUID(infoToHashed);
                        var uuid = $"AMD-{uuidHEX}";
                        // append VRAM to distinguish AMD GPUs
                        var vramPart = convertSize(gpuRAM);
                        var setName = vramPart != null ? $"{name} {vramPart}" : name;
                        var bd = new BaseDevice(DeviceType.AMD, uuid, setName, (int)oclDev.DeviceID);
                        var amdDevice = new AMDDevice(bd, oclDev.BUS_ID, gpuRAM, codename, infSection, platformNum);
                        amdDevices.Add(amdDevice);
                    }
                }
            }
            Logger.Info(Tag, "TryQueryAMDDevicesAsync END");

            return amdDevices;
        }


        private static bool IsAMDPlatform(OpenCLPlatform platform)
        {
            return platform.PlatformName == "AMD Accelerated Parallel Processing"
                || platform.PlatformVendor == "Advanced Micro Devices, Inc."
                || platform.PlatformName.Contains("AMD");
        }

        private static bool DuplicatedDevices(OpenCLDeviceDetectionResult data)
        {
            if (data?.Platforms?.Count > 0)
            {
                var devicesWithBusID = new HashSet<int>();
                var amdPlatforms = data.Platforms.Where(platform => IsAMDPlatform(platform)).ToList();
                foreach (var platform in amdPlatforms)
                {
                    foreach (var oclDev in platform.Devices)
                    {
                        var id = oclDev.BUS_ID;
                        if (devicesWithBusID.Contains(id))
                        {
                            return true;
                        }
                        devicesWithBusID.Add(id);
                    }
                }
            }
            return false;
        }

        // take lower platform
        private static OpenCLDeviceDetectionResult MergeResults(OpenCLDeviceDetectionResult a, OpenCLDeviceDetectionResult b)
        {
            var addedDevicesWithBusID = new HashSet<int>();
            var platformDevices = new Dictionary<int, OpenCLPlatform>();
            Action<OpenCLDeviceDetectionResult> fillUniquePlatformDevices = (OpenCLDeviceDetectionResult r) =>
            {
                if (r?.Platforms?.Count > 0)
                {
                    var amdPlatforms = r.Platforms
                    .Where(platform => IsAMDPlatform(platform))
                    .OrderBy(p => p.PlatformNum)
                    .ToList();
                    foreach (var platform in amdPlatforms)
                    {
                        if (!platformDevices.ContainsKey(platform.PlatformNum))
                        {
                            platformDevices[platform.PlatformNum] = new OpenCLPlatform
                            {
                                PlatformNum = platform.PlatformNum,
                                //Devices = WE ADD DEVICES, 
                                PlatformName = platform.PlatformName,
                                PlatformVendor = platform.PlatformVendor,
                            };
                        }
                        var curPlatform = platformDevices[platform.PlatformNum];
                        foreach (var oclDev in platform.Devices)
                        {
                            if (!addedDevicesWithBusID.Contains(oclDev.BUS_ID))
                            {
                                addedDevicesWithBusID.Add(oclDev.BUS_ID);
                                curPlatform.Devices.Add(oclDev);
                            }
                        }
                    }
                }
            };

            fillUniquePlatformDevices(a);
            fillUniquePlatformDevices(b);

            var ret = new OpenCLDeviceDetectionResult
            {
                Platforms = platformDevices.Values.ToList(),
                ErrorString = "",
                Status = "",
            };

            return ret;
        }
    }
}

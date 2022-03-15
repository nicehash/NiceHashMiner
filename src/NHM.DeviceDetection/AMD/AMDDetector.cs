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
    using System.IO;

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
                var GBResult = Math.Round(size, 0);
                // if number is odd we can assume that free memory was presented and we can return the upper even...
                if(GBResult > 5.0)//1,3,5gb gpus exist
                {
                    if (GBResult % 2 != 0)
                    {
                        GBResult += 1;
                    }
                }
                return GBResult + units[i];
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
                        var infoToHashedNew = $"{oclDev.DeviceID}--{oclDev.BUS_ID}--{DeviceType.AMD}--{codename}--{name}";
                        var infoToHashedOld = $"{oclDev.DeviceID}--{DeviceType.AMD}--{gpuRAM}--{codename}--{name}";
                        // cross ref info from vid controllers with bus id
                        var vidCtrl = availableVideoControllers?.Where(vid => vid.PCI_BUS_ID == oclDev.BUS_ID).FirstOrDefault() ?? null;
                        if (vidCtrl != null)
                        {
                            infSection = vidCtrl.InfSection;
                            infoToHashedOld += vidCtrl.PnpDeviceID;
                            infoToHashedNew += vidCtrl.PnpDeviceID;
                        }
                        else
                        {
                            Logger.Info(Tag, $"TryQueryAMDDevicesAsync cannot find VideoControllerData with bus ID {oclDev.BUS_ID}");
                        }
                        var uuidHEXOld = UUID.GetHexUUID(infoToHashedOld);
                        var uuidHEXNew = UUID.GetHexUUID(infoToHashedNew);
                        var uuidOld = $"AMD-{uuidHEXOld}";
                        var uuidNew = $"AMD-{uuidHEXNew}";
                        // append VRAM to distinguish AMD GPUs

                        //transition from old uuid's to new
                        try
                        {                                
                            var cfgPathOld = Paths.ConfigsPath($"device_settings_{uuidOld}.json");
                            var cfgPathNew = Paths.ConfigsPath($"device_settings_{uuidNew}.json");
                            if (File.Exists(cfgPathOld) && !File.Exists(cfgPathNew))//rename file and rename first line
                            {
                                string configText = File.ReadAllText(cfgPathOld);
                                configText = configText.Replace(uuidOld, uuidNew);
                                File.WriteAllText(cfgPathNew, configText);
                                File.Delete(cfgPathOld);
                            }
                        }
                        catch(Exception e)
                        {
                            Logger.Info(Tag, $"Error when transitioning from old to new AMD GPU config file. " + e.Message);
                        }

                        var vramPart = convertSize(gpuRAM);
                        var setName = vramPart != null ? $"{name} {vramPart}" : name;
                        var bd = new BaseDevice(DeviceType.AMD, uuidNew, setName, (int)oclDev.DeviceID);
                        var amdDevice = new AMDDevice(bd, oclDev.BUS_ID, gpuRAM, codename, infSection, platformNum);
                        //// BUM HERE!!!!
                        //var thisDeviceDriverVersion = result.AMDBusIDVersionPairs.FirstOrDefault(ver => ver.BUS_ID == oclDev.BUS_ID).AdrenalinVersion;
                        //if(thisDeviceDriverVersion != "") amdDevice.DEVICE_AMD_DRIVER = new Version(thisDeviceDriverVersion);
                        amdDevices.Add(amdDevice);
                    }
                }
            }
            Logger.Info(Tag, "TryQueryAMDDevicesAsync END");

            return amdDevices;
        }


        private static bool IsAMDPlatform(OpenCLPlatform platform)
        {
            if (platform == null) return false; 
            return platform.PlatformName == "AMD Accelerated Parallel Processing"
                || platform.PlatformVendor == "Advanced Micro Devices, Inc."
                || platform.PlatformName.Contains("AMD");
        }

        private static bool DuplicatedDevices(OpenCLDeviceDetectionResult data)
        {
            var anyMultipleSameBusIDs = data?.Platforms?
                    .Where(platform => IsAMDPlatform(platform))
                    .SelectMany(platform => platform.Devices)
                    .Where(dev => dev != null)
                    .GroupBy(dev => dev.BUS_ID)
                    .Select(group => group.Count() > 1)
                    .Any(multipleSameBusIDs => multipleSameBusIDs);
            return anyMultipleSameBusIDs ?? false;
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

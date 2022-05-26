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
    using Newtonsoft.Json;
    using NHM.DeviceDetection.Models.AMDBusIDVersionResult;
    using NHM.UUID;
    using System.IO;

    internal static class AMDDetector
    {
        private const string Tag = "AMDDetector";
        public static bool IsOpenClFallback { get; private set; }
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
        public static async Task<List<AMDDevice>> TryQueryAMDDevicesAsync(List<VideoControllerData> availableVideoControllers)
        {
            var openCLResult = await OpenCLDetector.TryQueryOpenCLDevicesAsync();
            var result = ConvertOpenCLResultToList(availableVideoControllers, openCLResult);
            if (result.success)
            {
                AMDDevice.RawDetectionOutput = openCLResult.rawOutput;
                return result.list;
            }
            var openCLResultFallback = await OpenCLDetector.TryQueryOpenCLDevicesAsyncFallback();
            var result2 = ConvertOpenCLResultToListFallback(availableVideoControllers, openCLResult, openCLResultFallback);
            AMDDevice.RawDetectionOutput = openCLResult.rawOutput;
            return result2;
        }
        private static (bool success, List<AMDDevice> list) ConvertOpenCLResultToList(List<VideoControllerData> availableVideoControllers, (string rawOutput, OpenCLDeviceDetectionResult parsed) openCLResult)
        {
            var amdDevices = new List<AMDDevice>();
            Logger.Info(Tag, "TryQueryAMDDevicesAsync START");
            Logger.Info(Tag, $"TryQueryOpenCLDevicesAsync RAW: '{openCLResult.rawOutput}'");
            if (DuplicatedDevices(openCLResult.parsed)) return (false, amdDevices);
            Platforms = openCLResult.parsed.Platforms;
            if (openCLResult.parsed.Platforms.Count <= 0) return (true, amdDevices);
            amdDevices = PopulateAMDDeviceList(openCLResult.parsed, availableVideoControllers);
            Logger.Info(Tag, "TryQueryAMDDevicesAsync END");
            return (true, amdDevices);
        }
        private static List<AMDDevice> ConvertOpenCLResultToListFallback(List<VideoControllerData> availableVideoControllers, (string rawOutput, OpenCLDeviceDetectionResult parsed) openCLResultOriginal, (string rawOutput, OpenCLDeviceDetectionResult parsed) openCLResultFallback)
        {
            var amdDevices = new List<AMDDevice>();
            Logger.Info(Tag, "Found duplicate devices. Trying fallback detection");
            Logger.Info(Tag, $"TryQueryOpenCLDevicesAsyncFallback RAW: '{openCLResultFallback.rawOutput}'");
            IsOpenClFallback = true;
            var isDuplicate = DuplicatedDevices(openCLResultFallback.parsed);
            var result = isDuplicate ?
                MergeResults(openCLResultOriginal.parsed, openCLResultFallback.parsed) :
                openCLResultFallback.parsed;
            if (isDuplicate) Logger.Info(Tag, $"TryQueryOpenCLDevicesAsyncFallback has duplicate files as well... Taking filtering lower platform devices");
            Platforms = result.Platforms;
            if (result.Platforms.Count <= 0) return amdDevices;
            amdDevices = PopulateAMDDeviceList(result, availableVideoControllers);
            Logger.Info(Tag, "TryQueryAMDDevicesAsync END");
            return amdDevices;
        }
        private static List<AMDDevice> PopulateAMDDeviceList(OpenCLDeviceDetectionResult result, List<VideoControllerData> availableVideoControllers)
        {
            var amdDevices = new List<AMDDevice>();
            var amdPlatforms = result.Platforms.Where(IsAMDPlatform).ToList();
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
                    amdDevice.RawDeviceData = JsonConvert.SerializeObject(oclDev);
                    var thisDeviceExtraADLResult = result.AMDBusIDVersionPairs.FirstOrDefault(ver => ver.BUS_ID == oclDev.BUS_ID);
                    if(thisDeviceExtraADLResult != null && thisDeviceExtraADLResult.BUS_ID == oclDev.BUS_ID)
                    {
                        amdDevice.ADLFunctionCall = thisDeviceExtraADLResult.FunctionCall;
                        amdDevice.ADLReturnCode = thisDeviceExtraADLResult.ADLRetCode;
                        amdDevice.RawDriverVersion = thisDeviceExtraADLResult.AdrenalinVersion;
                        if (Version.TryParse(thisDeviceExtraADLResult.AdrenalinVersion, out var parsedVer)) amdDevice.DEVICE_AMD_DRIVER = parsedVer;
                    }
                    amdDevices.Add(amdDevice);
                }
            }
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
                    .Where(IsAMDPlatform)
                    .SelectMany(platform => platform.Devices)
                    .Where(dev => dev != null)
                    .GroupBy(dev => dev.BUS_ID)
                    .Select(group => group.Count() > 1)
                    .Any(multipleSameBusIDs => multipleSameBusIDs);
            return anyMultipleSameBusIDs ?? false;
        }
        private static OpenCLDeviceDetectionResult MergeResults(OpenCLDeviceDetectionResult a, OpenCLDeviceDetectionResult b)
        {
            var addedDevicesWithBusID = new HashSet<int>();
            var platformDevices = new Dictionary<int, OpenCLPlatform>();
            var AMDBusIDVersionPairs = new List<AMDBusIDVersionResult>();
            void fillUniquePlatformDevices(OpenCLDeviceDetectionResult r)
            {
                if (r?.Platforms?.Count > 0)
                {
                    var amdPlatforms = r.Platforms
                    .Where(IsAMDPlatform)
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
                        foreach (var oclDev in platform.Devices.Where(dev => !addedDevicesWithBusID.Contains(dev.BUS_ID)))
                        {
                            addedDevicesWithBusID.Add(oclDev.BUS_ID);
                            curPlatform.Devices.Add(oclDev);
                        }
                    }
                }

                if (r?.AMDBusIDVersionPairs?.Count > 0)
                {
                    foreach (var dvr in r.AMDBusIDVersionPairs)
                    {
                        if (!AMDBusIDVersionPairs.Any(d => d.BUS_ID == dvr.BUS_ID)) AMDBusIDVersionPairs.Add(dvr);
                    }
                }
            }

            fillUniquePlatformDevices(a);
            fillUniquePlatformDevices(b);

            var ret = new OpenCLDeviceDetectionResult
            {
                AMDBusIDVersionPairs = AMDBusIDVersionPairs,
                Platforms = platformDevices.Values.ToList(),
                ErrorString = "",
                Status = "",
            };

            return ret;
        }
    }
}

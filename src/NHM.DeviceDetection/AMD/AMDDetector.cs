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
    using NHM.UUID;
    using System.IO;

    internal static class AMDDetector
    {
        private const string Tag = nameof(AMDDetector);
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
            Logger.Info(Tag, "TryQueryAMDDevicesAsync START");
            var (rawOutput, parsed) = await OpenCLDetector.TryQueryOpenCLDevicesAsync();
            Logger.Info(Tag, $"TryQueryOpenCLDevicesAsync RAW: '{rawOutput}'");
            var result = AMDDeviceWithUniqueBUS_IDs(availableVideoControllers, parsed);
            Platforms = parsed.Platforms;
            AMDDevice.RawDetectionOutput = rawOutput;

            // migration of old config files get rid of this eventually 
            foreach (var (dev, uuidOld) in result)
            {
                try
                {
                    var uuidNew = dev.UUID;

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
                catch (Exception e)
                {
                    Logger.Info(Tag, $"Error when transitioning from old to new AMD GPU config file. " + e.Message);
                }
            }

            Logger.Info(Tag, "TryQueryAMDDevicesAsync END");
            return result.Select(p => p.dev).ToList();
        }

        internal static List<(AMDDevice dev, string oldUUID)> AMDDeviceWithUniqueBUS_IDs(List<VideoControllerData> availableVideoControllers, OpenCLDeviceDetectionResult result)
        {
            if (AnyDevicesWithSameBusID(result)) Logger.Warn(Tag, "Devices with SAME BUS ID Found");
            var amdDevices = new List<(AMDDevice dev, string oldUUID)>();
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
                        Logger.Warn(Tag, $"TryQueryAMDDevicesAsync cannot find VideoControllerData with bus ID {oclDev.BUS_ID}");
                    }
                    var uuidHEXOld = UUID.GetHexUUID(infoToHashedOld);
                    var uuidHEXNew = UUID.GetHexUUID(infoToHashedNew);
                    var uuidOld = $"AMD-{uuidHEXOld}";
                    var uuidNew = $"AMD-{uuidHEXNew}";

                    var vramPart = convertSize(gpuRAM);
                    var setName = vramPart != null ? $"{name} {vramPart}" : name;
                    var amdDevice = new AMDDevice
                    {
                        DeviceType = DeviceType.AMD,
                        UUID = uuidNew,
                        Name = setName,
                        ID = (int)oclDev.DeviceID,
                        PCIeBusID = oclDev.BUS_ID,
                        GpuRam = gpuRAM,
                        Codename = codename,
                        InfSection = infSection,
                        OpenCLPlatformID = platformNum,
                        RawDeviceData = JsonConvert.SerializeObject(oclDev),
                    };
                    var thisDeviceExtraADLResult = result.AMDBusIDVersionPairs.FirstOrDefault(ver => ver.BUS_ID == oclDev.BUS_ID);
                    if(thisDeviceExtraADLResult != null && thisDeviceExtraADLResult.BUS_ID == oclDev.BUS_ID)
                    {
                        amdDevice.ADLFunctionCall = thisDeviceExtraADLResult.FunctionCall;
                        amdDevice.ADLReturnCode = thisDeviceExtraADLResult.ADLRetCode;
                        amdDevice.RawDriverVersion = thisDeviceExtraADLResult.AdrenalinVersion;
                        if (Version.TryParse(thisDeviceExtraADLResult.AdrenalinVersion, out var parsedVer)) amdDevice.DEVICE_AMD_DRIVER = parsedVer;
                    }
                    amdDevices.Add((amdDevice, uuidOld));
                }
            }

            return amdDevices
                .GroupBy(p => p.dev.PCIeBusID)
                .Select(group => group.FirstOrDefault())
                .OrderBy(p => p.dev.PCIeBusID)
                .ToList();
        }

        private static bool IsAMDPlatform(OpenCLPlatform platform)
        {
            if (platform == null) return false;
            return platform.PlatformName == "AMD Accelerated Parallel Processing"
                || platform.PlatformVendor == "Advanced Micro Devices, Inc."
                || platform.PlatformName.Contains("AMD");
        }

        private static bool AnyDevicesWithSameBusID(OpenCLDeviceDetectionResult data)
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
    }
}

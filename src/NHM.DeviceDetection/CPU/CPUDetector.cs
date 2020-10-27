using NHM.Common;
using NHM.Common.Device;
using NHM.Common.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Management;
using System.Threading.Tasks;

namespace NHM.DeviceDetection.CPU
{
    using NHM.UUID;

    internal static class CPUDetector
    {
        private const string Tag = "CPUDetector";

        public static async Task<(string rawOutput, CpuID parsed)> TryQueryCPUDevicesAsync()
        {
            var execStr = "cpu -n";
            Logger.Info(Tag, $"TryQueryCUDADevicesAsync START {execStr}");
            var result = await DeviceDetectionPrinter.GetDeviceDetectionResultAsync<CpuID>(execStr, 30 * 1000);
            Logger.Info(Tag, $"TryQueryCUDADevicesAsync END {execStr}");

            return result;
        }

        public static Task<CPUDevice> TryQueryCPUDeviceTask()
        {
            return Task.Run(async () =>
            {
                var (cpuJSON, cpuID) = await TryQueryCPUDevicesAsync();
                Logger.Info(Tag, cpuJSON);
                if (cpuID == null || !CpuUtils.IsCpuMiningCapable(cpuID)) return null;
                var cpuDetectResult = QueryCPUDevice();
                // get all CPUs
                var cpuCount = cpuID.PhysicalProcessorCount;
                var name = cpuID.Name.Trim();
                // get all cores (including virtual - HT can benefit mining)
                var threadsPerCpu = cpuDetectResult.VirtualCoresCount / cpuCount;
                // TODO important move this to settings
                var threadsPerCpuMask = threadsPerCpu;
                if (threadsPerCpu * cpuCount > 64)
                {
                    // set lower 
                    threadsPerCpuMask = 64;
                }

                List<ulong> affinityMasks = null;
                // multiple CPUs are identified as a single CPU from nhm perspective, it is the miner plugins job to handle this correctly
                if (cpuCount > 1)
                {
                    name = $"({cpuCount}x){name}";
                    affinityMasks = new List<ulong>();
                    for (var i = 0; i < cpuCount; i++)
                    {
                        var affinityMask = CpuUtils.CreateAffinityMask(i, threadsPerCpuMask);
                        affinityMasks.Add(affinityMask);
                    }
                }
                var hashedInfo = $"{0}--{name}--{threadsPerCpu}";
                foreach (var cpuInfo in cpuDetectResult.CpuInfos)
                {
                    hashedInfo += $"{cpuInfo.Family}--{cpuInfo.ModelName}--{cpuInfo.NumberOfCores}--{cpuInfo.PhysicalID}--{cpuInfo.VendorID}";
                    hashedInfo += cpuJSON;
                }
                var uuidHEX = UUID.GetHexUUID(hashedInfo);
                var uuid = $"CPU-{uuidHEX}";

                // plugin device
                var bd = new BaseDevice(DeviceType.CPU, uuid, name, 0);
                var cpu = new CPUDevice(bd, cpuCount, threadsPerCpu, cpuDetectResult.IsHyperThreadingEnabled, affinityMasks, CpuUtils.SupportedExtensions(cpuID), cpuID);
                return cpu;
            });
        }

        // maybe this will come in handy
        private static CPUDetectionResult QueryCPUDevice()
        {
            var ret = new CPUDetectionResult
            {
                CpuInfos = GetCpuInfos(),
                VirtualCoresCount = GetVirtualCoresCount(),
                //NumberOfCPUCores = 0 // calculate from CpuInfos
            };
            ret.NumberOfCPUCores = ret.CpuInfos.Select(info => info.NumberOfCores).Sum();
            return ret;
        }

        private static List<CpuInfo> GetCpuInfos()
        {
            var ret = new List<CpuInfo>();
            try
            {
                using (var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_Processor"))
                using (var query = searcher.Get())
                {
                    foreach (var obj in query)
                    {
                        var numberOfCores = Convert.ToInt32(obj.GetPropertyValue("NumberOfCores"));
                        var info = new CpuInfo
                        {
                            Family = obj["Family"].ToString(),
                            VendorID = obj["Manufacturer"].ToString(),
                            ModelName = obj["Name"].ToString(),
                            PhysicalID = obj["ProcessorID"].ToString(),
                            NumberOfCores = numberOfCores
                        };
                        ret.Add(info);
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Error(Tag, $"GetCpuInfos error: {e.Message}");
            }
            return ret;
        }

        private static int GetVirtualCoresCount()
        {
            var virtualCoreCount = 0;
            try
            {
                using (var searcher = new ManagementObjectSearcher("SELECT NumberOfLogicalProcessors FROM Win32_ComputerSystem"))
                using (var query = searcher.Get())
                {
                    foreach (var item in query)
                    {
                        virtualCoreCount += Convert.ToInt32(item.GetPropertyValue("NumberOfLogicalProcessors"));
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Error(Tag, $"GetVirtualCoresCount error: {e.Message}");
            }
            return virtualCoreCount;
        }
    }
}

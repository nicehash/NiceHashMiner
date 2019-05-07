using NiceHashMiner.Configs;
using NiceHashMiner.Devices.Querying;
using NiceHashMiner.Devices.Querying.Amd;
using NiceHashMiner.Devices.Querying.Nvidia;
using NiceHashMiner.Utils;
using NiceHashMinerLegacy.Common.Enums;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using static NiceHashMiner.Translations;
using NiceHashMinerLegacy.Common;

namespace NiceHashMiner.Devices
{
    /// <summary>
    /// ComputeDeviceManager class is used to query ComputeDevices avaliable on the system.
    /// Query CPUs, GPUs [Nvidia, AMD]
    /// </summary>
    public static class ComputeDeviceManager
    {
        private const string Tag = "ComputeDeviceManager.Query";

        private static readonly NvidiaSmiDriver NvidiaRecomendedDriver = new NvidiaSmiDriver(372, 54); // 372.54;
        private static readonly NvidiaSmiDriver NvidiaMinDetectionDriver = new NvidiaSmiDriver(362, 61); // 362.61;
        
        public static async Task<QueryResult> QueryDevicesAsync(IProgress<string> progress, bool skipVideoControllerCheck)
        {
            // #0 get video controllers, used for cross checking
            //progress?.Report(TODO no CPU checking???);
            // Order important CPU Query must be first
            // #1 CPU         
            progress?.Report(Tr("Checking CPU Info"));
            await Task.Run(() => WindowsManagementObjectSearcher.QueryCPU_Info());
            var cpuDevs = await CpuQuery.QueryCpusAsync();
            AvailableDevices.AddDevices(cpuDevs);         

            // #2 CUDA
            progress?.Report(Tr("Querying CUDA devices"));
            var nv = new NvidiaQuery();
            var nvDevs = nv.QueryCudaDevices();

            if (nvDevs != null)
            {
                AvailableDevices.NumDetectedNvDevs = nvDevs.Count;

                if (ConfigManager.GeneralConfig.DeviceDetection.DisableDetectionNVIDIA)
                {
                    Logger.Info(Tag, "Skipping NVIDIA device detection, settings are set to disabled");
                }
                else
                {
                    AvailableDevices.AddDevices(nvDevs);
                }
            }

            // OpenCL and AMD

            var amd = new AmdQuery(AvailableDevices.NumDetectedNvDevs);
            // #3 OpenCL
            progress?.Report(Tr("Querying OpenCL devices"));
            await amd.QueryOpenCLDevicesAsync();
            // #4 AMD query AMD from OpenCL devices, get serial and add devices
            progress?.Report(Tr("Checking AMD OpenCL GPUs"));
            var amdDevs = await Task.Run(() => amd.QueryAmd());

            if (amdDevs != null)
            {
                AvailableDevices.NumDetectedAmdDevs = amdDevs.Count;

                if (ConfigManager.GeneralConfig.DeviceDetection.DisableDetectionAMD)
                {
                    Logger.Info(Tag, "Skipping AMD device detection, settings set to disabled");
                }
                else
                {
                    AvailableDevices.AddDevices(amdDevs);
                }
            }

            // #5 uncheck CPU if GPUs present, call it after we Query all devices
            AvailableDevices.UncheckCpuIfGpu();
            if (skipVideoControllerCheck) return null;
            // TODO update this to report undetected hardware
            // #6 check NVIDIA, AMD devices count
            bool nvCountMatched;
            {
                var amdCount = 0;
                var nvidiaCount = 0;
                foreach (var vidCtrl in WindowsManagementObjectSearcher.AvailableVideoControllers)
                {
                    if (vidCtrl.IsNvidia)
                    {
                        if (CudaUnsupported.IsSupported(vidCtrl.Name))
                        {
                            nvidiaCount++;
                        }
                        else
                        {
                            Logger.Info(Tag, $"Device not supported NVIDIA/CUDA device not supported {vidCtrl.Name}");

                        }
                    }
                    else if (vidCtrl.IsAmd)
                    {
                        amdCount++;
                    }
                }

                nvCountMatched = nvidiaCount == AvailableDevices.NumDetectedNvDevs;

                Logger.Info(Tag, nvCountMatched ? "Cuda NVIDIA/CUDA device count GOOD" : "Cuda NVIDIA/CUDA device count BAD!!!");
                Logger.Info(Tag, amdCount == amdDevs?.Count ? "AMD GPU device count GOOD" : "AMD GPU device count BAD!!!");
            }

            var result = new QueryResult(NvidiaMinDetectionDriver.ToString(), NvidiaRecomendedDriver.ToString());

            var ramOK = CheckRam();

            if (!ConfigManager.GeneralConfig.ShowDriverVersionWarning) return result;

            if (WindowsManagementObjectSearcher.HasNvidiaVideoController)
            {
                var currentDriver = WindowsManagementObjectSearcher.NvidiaDriver;

                result.CurrentDriverString = currentDriver.ToString();
                result.FailedMinNVDriver = !nvCountMatched && currentDriver < NvidiaMinDetectionDriver;

                result.FailedRecommendedNVDriver = currentDriver < NvidiaRecomendedDriver && currentDriver.LeftPart > -1;
            }

            result.NoDevices = AvailableDevices.Devices.Count <= 0;

            result.FailedRamCheck = !ramOK;

            var badVidCtrls = WindowsManagementObjectSearcher.BadVideoControllers;
            foreach (var failedVc in badVidCtrls)
            {
                result.FailedVidControllerStatus = true;
                result.FailedVidControllerInfo +=
                    $"{Tr("Name: {0}, Status {1}, PNPDeviceID {2}", failedVc.Name, failedVc.Status, failedVc.PnpDeviceID)}\n";
            }

            //result.FailedAmdDriverCheck = failedAmdDriverCheck;
            //result.FailedCpu64Bit = failed64Bit;
            //result.FailedCpuCount = failedCpuCount;

            return result;
        }

        #region Helpers

        private static bool CheckRam()
        {
            var nvRamSum = 0ul;
            var amdRamSum = 0ul;
            foreach (var dev in AvailableDevices.Devices)
            {
                if (dev.DeviceType == DeviceType.NVIDIA)
                {
                    nvRamSum += dev.GpuRam;
                }
                else if (dev.DeviceType == DeviceType.AMD)
                {
                    amdRamSum += dev.GpuRam;
                }
            }

            // Make gpu ram needed not larger than 4GB per GPU
            var totalGpuRam = Math.Min((ulong) ((nvRamSum + amdRamSum) * 0.6 / 1024),
                (ulong) AvailableDevices.AvailGpUs * 4 * 1024 * 1024);
            var totalSysRam = WindowsManagementObjectSearcher.FreePhysicalMemory + WindowsManagementObjectSearcher.FreeVirtualMemory;
            

            if (totalSysRam < totalGpuRam)
            {
                Logger.Info(Tag, "virtual memory size BAD");
                return false;
            }
            else
            {
                Logger.Info(Tag, "virtual memory size GOOD");
                return true;
            }
        }

        #endregion Helpers
    }
}

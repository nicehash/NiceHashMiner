using NiceHashMiner.Configs;
using NiceHashMiner.Devices.OpenCL;
using NiceHashMiner.Devices.Querying;
using NiceHashMinerLegacy.Common.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static NiceHashMiner.Translations;

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

        public static event EventHandler<string> OnProgressUpdate;
        
        public static async Task<QueryResult> QueryDevicesAsync()
        {
            // #0 get video controllers, used for cross checking
            var badVidCtrls = SystemSpecs.QueryVideoControllers();
            // Order important CPU Query must be first
            // #1 CPU
            Cpu.QueryCpus();
            // #2 CUDA
            var numDevs = 0;
            if (ConfigManager.GeneralConfig.DeviceDetection.DisableDetectionNVIDIA)
            {
                Helpers.ConsolePrint(Tag, "Skipping NVIDIA device detection, settings are set to disabled");
            }
            else
            {
                OnProgressUpdate?.Invoke(null, Tr("Querying CUDA devices"));
                numDevs = NvidiaQuery.QueryCudaDevices();
            }
            // OpenCL and AMD
            List<OpenCLDevice> amdDevs = null;
            var failedAmdDriverCheck = false;
            if (ConfigManager.GeneralConfig.DeviceDetection.DisableDetectionAMD)
            {
                Helpers.ConsolePrint(Tag, "Skipping AMD device detection, settings set to disabled");
                OnProgressUpdate?.Invoke(null, Tr("Skip check for AMD OpenCL GPUs"));
            }
            else
            {
                // #3 OpenCL
                OnProgressUpdate?.Invoke(null, Tr("Querying OpenCL devices"));
                var openCLQuerySuccess = QueryOpenCL.TryQueryOpenCLDevices(out var openCLResult);
                // #4 AMD query AMD from OpenCL devices, get serial and add devices
                OnProgressUpdate?.Invoke(null, Tr("Checking AMD OpenCL GPUs"));
                var amd = new AmdQuery(numDevs);
                amdDevs = amd.QueryAmd(openCLQuerySuccess, openCLResult, out failedAmdDriverCheck);
            }
            // #5 uncheck CPU if GPUs present, call it after we Query all devices
            AvailableDevices.UncheckCpuIfGpu();

            // TODO update this to report undetected hardware
            // #6 check NVIDIA, AMD devices count
            bool nvCountMatched;
            {
                var amdCount = 0;
                var nvidiaCount = 0;
                foreach (var vidCtrl in SystemSpecs.AvailableVideoControllers)
                {
                    if (vidCtrl.IsNvidia)
                    {
                        if (CudaUnsupported.IsSupported(vidCtrl.Name))
                        {
                            nvidiaCount++;
                        }
                        else
                        {
                            Helpers.ConsolePrint(Tag,
                                "Device not supported NVIDIA/CUDA device not supported " + vidCtrl.Name);
                        }
                    }
                    else if (vidCtrl.IsAmd)
                    {
                        amdCount++;
                    }
                }

                nvCountMatched = nvidiaCount == NvidiaQuery.CudaDevices?.Count;

                Helpers.ConsolePrint(Tag,
                    nvCountMatched
                        ? "Cuda NVIDIA/CUDA device count GOOD"
                        : "Cuda NVIDIA/CUDA device count BAD!!!");
                Helpers.ConsolePrint(Tag,
                    amdCount == amdDevs?.Count ? "AMD GPU device count GOOD" : "AMD GPU device count BAD!!!");
            }

            SortBusIDs(DeviceType.NVIDIA);
            SortBusIDs(DeviceType.AMD);

            var result = new QueryResult(NvidiaMinDetectionDriver.ToString(), NvidiaRecomendedDriver.ToString());

            var ramOK = CheckRam();

            if (!ConfigManager.GeneralConfig.ShowDriverVersionWarning) return result;

            if (SystemSpecs.HasNvidiaVideoController)
            {
                var currentDriver = await NvidiaQuery.GetNvSmiDriverAsync();

                result.CurrentDriverString = currentDriver.ToString();
                result.FailedMinNVDriver = !nvCountMatched && currentDriver < NvidiaMinDetectionDriver;

                result.FailedRecommendedNVDriver = currentDriver < NvidiaRecomendedDriver && currentDriver.LeftPart > -1;
            }

            result.NoDevices = AvailableDevices.Devices.Count <= 0;

            result.FailedRamCheck = !ramOK;

            foreach (var failedVc in badVidCtrls)
            {
                result.FailedVidControllerStatus = true;
                result.FailedVidControllerInfo +=
                    $"{Tr("Name: {0}, Status {1}, PNPDeviceID {2}", failedVc.Name, failedVc.Status, failedVc.PnpDeviceID)}\n";
            }

            result.FailedAmdDriverCheck = failedAmdDriverCheck;

            return result;
        }

        #region Helpers

        private static void SortBusIDs(DeviceType type)
        {
            var devs = AvailableDevices.Devices.Where(d => d.DeviceType == type);
            var sortedDevs = devs.OrderBy(d => d.BusID).ToList();

            for (var i = 0; i < sortedDevs.Count; i++)
            {
                sortedDevs[i].IDByBus = i;
            }
        }

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
            var totalSysRam = SystemSpecs.FreePhysicalMemory + SystemSpecs.FreeVirtualMemory;
            

            if (totalSysRam < totalGpuRam)
            {
                Helpers.ConsolePrint(Tag, "virtual memory size BAD");
                return false;
            }
            else
            {
                Helpers.ConsolePrint(Tag, "virtual memory size GOOD");
                return true;
            }
        }

        #endregion Helpers
    }
}

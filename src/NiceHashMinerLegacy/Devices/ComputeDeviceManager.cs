using Newtonsoft.Json;
using NiceHashMiner.Configs;
using NiceHashMiner.Interfaces;
using NVIDIA.NVAPI;
using ManagedCuda.Nvml;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Management;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using NiceHashMiner.Devices.Querying;
using NiceHashMinerLegacy.Common.Enums;
using static NiceHashMiner.Translations;
using NiceHashMiner.Devices.OpenCL;
using NiceHashMiner.PInvoke;

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

        private static void ShowMessageAndStep(string infoMsg)
        {
            MessageNotifier?.SetMessageAndIncrementStep(infoMsg);
        }

        public static IMessageNotifier MessageNotifier { get; private set; }

        public static bool CheckVideoControllersCountMismath()
        {
            // this function checks if count of CUDA devices is same as it was on application start, reason for that is
            // because of some reason (especially when algo switching occure) CUDA devices are dissapiring from system
            // creating tons of problems e.g. miners stop mining, lower rig hashrate etc.

            /* commented because when GPU is "lost" windows still see all of them
            // first check windows video controlers
            List<VideoControllerData> currentAvaliableVideoControllers = new List<VideoControllerData>();
            WindowsDisplayAdapters.QueryVideoControllers(currentAvaliableVideoControllers, false);
            

            int GPUsOld = AvailableVideoControllers.Count;
            int GPUsNew = currentAvaliableVideoControllers.Count;

            Helpers.ConsolePrint("ComputeDeviceManager.CheckCount", "Video controlers GPUsOld: " + GPUsOld.ToString() + " GPUsNew:" + GPUsNew.ToString());
            */

            // check CUDA devices

            // TODO
            return false;

            //var currentCudaDevices = new List<CudaDevice>();
            //if (!Nvidia.IsSkipNvidia())
            //    Nvidia.QueryCudaDevices(ref currentCudaDevices);

            //var gpusOld = _cudaDevices.Count;
            //var gpusNew = currentCudaDevices.Count;

            //Helpers.ConsolePrint("ComputeDeviceManager.CheckCount",
            //    "CUDA GPUs count: Old: " + gpusOld + " / New: " + gpusNew);

            //return (gpusNew < gpusOld);
        }

        public static async Task<QueryResult> QueryDevicesAsync(IMessageNotifier messageNotifier)
        {
            MessageNotifier = messageNotifier;
            // #0 get video controllers, used for cross checking
            SystemSpecs.QueryVideoControllers();
            // Order important CPU Query must be first
            // #1 CPU
            Cpu.QueryCpus();
            // #2 CUDA
            var numDevs = 0;
            NvidiaQuery nvQuery = null;
            if (ConfigManager.GeneralConfig.DeviceDetection.DisableDetectionNVIDIA)
            {
                Helpers.ConsolePrint(Tag, "Skipping NVIDIA device detection, settings are set to disabled");
            }
            else
            {
                ShowMessageAndStep(Tr("Querying CUDA devices"));
                nvQuery = new NvidiaQuery();
                nvQuery.QueryCudaDevices();
            }
            // OpenCL and AMD
            List<OpenCLDevice> amdDevs = null;
            if (ConfigManager.GeneralConfig.DeviceDetection.DisableDetectionAMD)
            {
                Helpers.ConsolePrint(Tag, "Skipping AMD device detection, settings set to disabled");
                ShowMessageAndStep(Tr("Skip check for AMD OpenCL GPUs"));
            }
            else
            {
                // #3 OpenCL
                ShowMessageAndStep(Tr("Querying OpenCL devices"));
                var openCLQuerySuccess = QueryOpenCL.TryQueryOpenCLDevices(out var openCLResult);
                // #4 AMD query AMD from OpenCL devices, get serial and add devices
                ShowMessageAndStep(Tr("Checking AMD OpenCL GPUs"));
                var amd = new AmdQuery(numDevs);
                amdDevs = amd.QueryAmd(openCLQuerySuccess, openCLResult);
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
                    if (vidCtrl.Name.ToLower().Contains("nvidia") && CudaUnsupported.IsSupported(vidCtrl.Name))
                    {
                        nvidiaCount += 1;
                    }
                    else if (vidCtrl.Name.ToLower().Contains("nvidia"))
                    {
                        Helpers.ConsolePrint(Tag,
                            "Device not supported NVIDIA/CUDA device not supported " + vidCtrl.Name);
                    }
                    amdCount += (vidCtrl.Name.ToLower().Contains("amd")) ? 1 : 0;
                }

                nvCountMatched = nvidiaCount == nvQuery?.CudaDevices?.Count;

                Helpers.ConsolePrint(Tag,
                    nvCountMatched
                        ? "Cuda NVIDIA/CUDA device count GOOD"
                        : "Cuda NVIDIA/CUDA device count BAD!!!");
                Helpers.ConsolePrint(Tag,
                    amdCount == amdDevs?.Count ? "AMD GPU device count GOOD" : "AMD GPU device count BAD!!!");
            }

            // #x remove reference
            MessageNotifier = null;

            SortBusIDs(DeviceType.NVIDIA);
            SortBusIDs(DeviceType.AMD);

            var result = new QueryResult(NvidiaMinDetectionDriver.ToString(), NvidiaRecomendedDriver.ToString());

            var ramBad = CheckRam();

            if (!ConfigManager.GeneralConfig.ShowDriverVersionWarning) return result;

            if (SystemSpecs.HasNvidiaVideoController)
            {
                var currentDriver = await NvidiaQuery.GetNvSmiDriverAsync();

                result.CurrentDriverString = currentDriver.ToString();
                result.FailedMinNVDriver = !nvCountMatched && currentDriver < NvidiaMinDetectionDriver;

                result.FailedRecommendedNVDriver = currentDriver < NvidiaRecomendedDriver && currentDriver.LeftPart > -1;
            }

            result.NoDevices = AvailableDevices.Devices.Count <= 0;

            result.FailedRamCheck = ramBad;

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
            var totalGpuRam = Math.Min((ulong)((nvRamSum + amdRamSum) * 0.6 / 1024),
                (ulong)AvailableDevices.AvailGpUs * 4 * 1024 * 1024);
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

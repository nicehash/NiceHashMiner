using ATI.ADL;
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
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;
using NiceHashMiner.Devices.Querying;
using NiceHashMiner.Forms;
using NiceHashMinerLegacy.Common.Enums;

namespace NiceHashMiner.Devices
{
    /// <summary>
    /// ComputeDeviceManager class is used to query ComputeDevices avaliable on the system.
    /// Query CPUs, GPUs [Nvidia, AMD]
    /// </summary>
    public class ComputeDeviceManager
    {
        public static class Query
        {
            private const string Tag = "ComputeDeviceManager.Query";

            // format 372.54;
            private class NvidiaSmiDriver
            {
                public NvidiaSmiDriver(int left, int right)
                {
                    LeftPart = left;
                    _rightPart = right;
                }

                public bool IsLesserVersionThan(NvidiaSmiDriver b)
                {
                    if (LeftPart < b.LeftPart)
                    {
                        return true;
                    }
                    return LeftPart == b.LeftPart && GetRightVal(_rightPart) < GetRightVal(b._rightPart);
                }

                public override string ToString()
                {
                    return $"{LeftPart}.{_rightPart}";
                }

                public readonly int LeftPart;
                private readonly int _rightPart;

                private static int GetRightVal(int val)
                {
                    if (val >= 10)
                    {
                        return val;
                    }
                    return val * 10;
                }
            }

            private static readonly NvidiaSmiDriver NvidiaRecomendedDriver = new NvidiaSmiDriver(372, 54); // 372.54;
            private static readonly NvidiaSmiDriver NvidiaMinDetectionDriver = new NvidiaSmiDriver(362, 61); // 362.61;
            private static NvidiaSmiDriver _currentNvidiaSmiDriver = new NvidiaSmiDriver(-1, -1);
            private static readonly NvidiaSmiDriver InvalidSmiDriver = new NvidiaSmiDriver(-1, -1);

            // naming purposes
            public static int CpuCount = 0;

            public static int GpuCount = 0;

            private static NvidiaSmiDriver GetNvidiaSmiDriver()
            {
                if (WindowsDisplayAdapters.HasNvidiaVideoController())
                {
                    string stdErr;
                    string args;
                    var stdOut = stdErr = args = string.Empty;
                    var smiPath = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles) +
                                  "\\NVIDIA Corporation\\NVSMI\\nvidia-smi.exe";
                    if (smiPath.Contains(" (x86)")) smiPath = smiPath.Replace(" (x86)", "");
                    try
                    {
                        var P = new Process
                        {
                            StartInfo =
                            {
                                FileName = smiPath,
                                UseShellExecute = false,
                                RedirectStandardOutput = true,
                                RedirectStandardError = true,
                                CreateNoWindow = true
                            }
                        };
                        P.Start();
                        P.WaitForExit(30 * 1000);

                        stdOut = P.StandardOutput.ReadToEnd();
                        stdErr = P.StandardError.ReadToEnd();

                        const string findString = "Driver Version: ";
                        using (var reader = new StringReader(stdOut))
                        {
                            var line = string.Empty;
                            do
                            {
                                line = reader.ReadLine();
                                if (line != null && line.Contains(findString))
                                {
                                    var start = line.IndexOf(findString);
                                    var driverVer = line.Substring(start, start + 7);
                                    driverVer = driverVer.Replace(findString, "").Substring(0, 7).Trim();
                                    var drVerDouble = double.Parse(driverVer, CultureInfo.InvariantCulture);
                                    var dot = driverVer.IndexOf(".");
                                    var leftPart = int.Parse(driverVer.Substring(0, 3));
                                    var rightPart = int.Parse(driverVer.Substring(4, 2));
                                    return new NvidiaSmiDriver(leftPart, rightPart);
                                }
                            } while (line != null);
                        }
                    }
                    catch (Exception ex)
                    {
                        Helpers.ConsolePrint(Tag, "GetNvidiaSMIDriver Exception: " + ex.Message);
                        return InvalidSmiDriver;
                    }
                }
                return InvalidSmiDriver;
            }

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
                

                int GPUsOld = AvaliableVideoControllers.Count;
                int GPUsNew = currentAvaliableVideoControllers.Count;

                Helpers.ConsolePrint("ComputeDeviceManager.CheckCount", "Video controlers GPUsOld: " + GPUsOld.ToString() + " GPUsNew:" + GPUsNew.ToString());
                */

                // check CUDA devices
                var currentCudaDevices = new List<CudaDevice>();
                if (!Nvidia.IsSkipNvidia())
                    Nvidia.QueryCudaDevices(ref currentCudaDevices);

                var gpusOld = _cudaDevices.Count;
                var gpusNew = currentCudaDevices.Count;

                Helpers.ConsolePrint("ComputeDeviceManager.CheckCount",
                    "CUDA GPUs count: Old: " + gpusOld + " / New: " + gpusNew);

                return (gpusNew < gpusOld);
            }

            public static void QueryDevices(IMessageNotifier messageNotifier)
            {
                // check NVIDIA nvml.dll and copy over scope
                {
                    var nvmlPath = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles) +
                                   "\\NVIDIA Corporation\\NVSMI\\nvml.dll";
                    if (nvmlPath.Contains(" (x86)")) nvmlPath = nvmlPath.Replace(" (x86)", "");
                    if (File.Exists(nvmlPath))
                    {
                        var copyToPath = Directory.GetCurrentDirectory() + "\\nvml.dll";
                        try
                        {
                            File.Copy(nvmlPath, copyToPath, true);
                            Helpers.ConsolePrint(Tag, $"Copy from {nvmlPath} to {copyToPath} done");
                        }
                        catch (Exception e)
                        {
                            Helpers.ConsolePrint(Tag, "Copy nvml.dll failed: " + e.Message);
                        }
                    }
                }


                MessageNotifier = messageNotifier;
                // #0 get video controllers, used for cross checking
                WindowsDisplayAdapters.QueryVideoControllers();
                // Order important CPU Query must be first
                // #1 CPU
                Cpu.QueryCpus();
                // #2 CUDA
                if (Nvidia.IsSkipNvidia())
                {
                    Helpers.ConsolePrint(Tag, "Skipping NVIDIA device detection, settings are set to disabled");
                }
                else
                {
                    ShowMessageAndStep(International.GetText("Compute_Device_Query_Manager_CUDA_Query"));
                    Nvidia.QueryCudaDevices();
                }
                // OpenCL and AMD
                if (ConfigManager.GeneralConfig.DeviceDetection.DisableDetectionAMD)
                {
                    Helpers.ConsolePrint(Tag, "Skipping AMD device detection, settings set to disabled");
                    ShowMessageAndStep(International.GetText("Compute_Device_Query_Manager_AMD_Query_Skip"));
                }
                else
                {
                    // #3 OpenCL
                    ShowMessageAndStep(International.GetText("Compute_Device_Query_Manager_OpenCL_Query"));
                    OpenCL.QueryOpenCLDevices();
                    // #4 AMD query AMD from OpenCL devices, get serial and add devices
                    ShowMessageAndStep(International.GetText("Compute_Device_Query_Manager_AMD_Query"));
                    var amd = new AmdQuery(AvaliableVideoControllers);
                    AmdDevices = amd.QueryAmd(_isOpenCLQuerySuccess, _openCLJsonData);
                }
                // #5 uncheck CPU if GPUs present, call it after we Query all devices
                Group.UncheckedCpu();

                // TODO update this to report undetected hardware
                // #6 check NVIDIA, AMD devices count
                var nvidiaCount = 0;
                {
                    var amdCount = 0;
                    foreach (var vidCtrl in AvaliableVideoControllers)
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
                    Helpers.ConsolePrint(Tag,
                        nvidiaCount == _cudaDevices.Count
                            ? "Cuda NVIDIA/CUDA device count GOOD"
                            : "Cuda NVIDIA/CUDA device count BAD!!!");
                    Helpers.ConsolePrint(Tag,
                        amdCount == AmdDevices.Count ? "AMD GPU device count GOOD" : "AMD GPU device count BAD!!!");
                }
                // allerts
                _currentNvidiaSmiDriver = GetNvidiaSmiDriver();
                // if we have nvidia cards but no CUDA devices tell the user to upgrade driver
                var isNvidiaErrorShown = false; // to prevent showing twice
                var showWarning = ConfigManager.GeneralConfig.ShowDriverVersionWarning &&
                                  WindowsDisplayAdapters.HasNvidiaVideoController();
                if (showWarning && _cudaDevices.Count != nvidiaCount &&
                    _currentNvidiaSmiDriver.IsLesserVersionThan(NvidiaMinDetectionDriver))
                {
                    isNvidiaErrorShown = true;
                    var minDriver = NvidiaMinDetectionDriver.ToString();
                    var recomendDrvier = NvidiaRecomendedDriver.ToString();
                    MessageBox.Show(string.Format(
                            International.GetText("Compute_Device_Query_Manager_NVIDIA_Driver_Detection"),
                            minDriver, recomendDrvier),
                        International.GetText("Compute_Device_Query_Manager_NVIDIA_RecomendedDriver_Title"),
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                // recomended driver
                if (showWarning && _currentNvidiaSmiDriver.IsLesserVersionThan(NvidiaRecomendedDriver) &&
                    !isNvidiaErrorShown && _currentNvidiaSmiDriver.LeftPart > -1)
                {
                    var recomendDrvier = NvidiaRecomendedDriver.ToString();
                    var nvdriverString = _currentNvidiaSmiDriver.LeftPart > -1
                        ? string.Format(
                            International.GetText("Compute_Device_Query_Manager_NVIDIA_Driver_Recomended_PART"),
                            _currentNvidiaSmiDriver)
                        : "";
                    MessageBox.Show(string.Format(
                            International.GetText("Compute_Device_Query_Manager_NVIDIA_Driver_Recomended"),
                            recomendDrvier, nvdriverString, recomendDrvier),
                        International.GetText("Compute_Device_Query_Manager_NVIDIA_RecomendedDriver_Title"),
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }

                // no devices found
                if (Available.Devices.Count <= 0)
                {
                    var result = MessageBox.Show(International.GetText("Compute_Device_Query_Manager_No_Devices"),
                        International.GetText("Compute_Device_Query_Manager_No_Devices_Title"),
                        MessageBoxButtons.OKCancel, MessageBoxIcon.Warning);
                    if (result == DialogResult.OK)
                    {
                        Process.Start(Links.NhmNoDevHelp);
                    }
                }

                // create AMD bus ordering for Claymore
                var amdDevices = Available.Devices.FindAll((a) => a.DeviceType == DeviceType.AMD);
                amdDevices.Sort((a, b) => a.BusID.CompareTo(b.BusID));
                for (var i = 0; i < amdDevices.Count; i++)
                {
                    amdDevices[i].IDByBus = i;
                }
                //create NV bus ordering for Claymore
                var nvDevices = Available.Devices.FindAll((a) => a.DeviceType == DeviceType.NVIDIA);
                nvDevices.Sort((a, b) => a.BusID.CompareTo(b.BusID));
                for (var i = 0; i < nvDevices.Count; i++)
                {
                    nvDevices[i].IDByBus = i;
                }

                // get GPUs RAM sum
                // bytes
                Available.NvidiaRamSum = 0;
                Available.AmdRamSum = 0;
                foreach (var dev in Available.Devices)
                {
                    if (dev.DeviceType == DeviceType.NVIDIA)
                    {
                        Available.NvidiaRamSum += dev.GpuRam;
                    }
                    else if (dev.DeviceType == DeviceType.AMD)
                    {
                        Available.AmdRamSum += dev.GpuRam;
                    }
                }
                // Make gpu ram needed not larger than 4GB per GPU
                var totalGpuRam = Math.Min((Available.NvidiaRamSum + Available.AmdRamSum) * 0.6 / 1024,
                    (double) Available.AvailGpUs * 4 * 1024 * 1024);
                double totalSysRam = SystemSpecs.FreePhysicalMemory + SystemSpecs.FreeVirtualMemory;
                // check
                if (ConfigManager.GeneralConfig.ShowDriverVersionWarning && totalSysRam < totalGpuRam)
                {
                    Helpers.ConsolePrint(Tag, "virtual memory size BAD");
                    MessageBox.Show(International.GetText("VirtualMemorySize_BAD"),
                        International.GetText("Warning_with_Exclamation"),
                        MessageBoxButtons.OK);
                }
                else
                {
                    Helpers.ConsolePrint(Tag, "virtual memory size GOOD");
                }

                // #x remove reference
                MessageNotifier = null;
            }

            #region Helpers

            private static readonly List<VideoControllerData> AvaliableVideoControllers =
                new List<VideoControllerData>();

            private static class WindowsDisplayAdapters
            {
                private static string SafeGetProperty(ManagementBaseObject mbo, string key)
                {
                    try
                    {
                        var o = mbo.GetPropertyValue(key);
                        if (o != null)
                        {
                            return o.ToString();
                        }
                    }
                    catch { }

                    return "key is null";
                }

                public static void QueryVideoControllers()
                {
                    QueryVideoControllers(AvaliableVideoControllers, true);
                }

                private static void QueryVideoControllers(List<VideoControllerData> avaliableVideoControllers,
                    bool warningsEnabled)
                {
                    var stringBuilder = new StringBuilder();
                    stringBuilder.AppendLine("");
                    stringBuilder.AppendLine("QueryVideoControllers: ");
                    var moc = new ManagementObjectSearcher("root\\CIMV2",
                        "SELECT * FROM Win32_VideoController WHERE PNPDeviceID LIKE 'PCI%'").Get();
                    var allVideoContollersOK = true;
                    foreach (var manObj in moc)
                    {
                        //Int16 ram_Str = manObj["ProtocolSupported"] as Int16; manObj["AdapterRAM"] as string
                        ulong.TryParse(SafeGetProperty(manObj, "AdapterRAM"), out var memTmp);
                        var vidController = new VideoControllerData
                        {
                            Name = SafeGetProperty(manObj, "Name"),
                            Description = SafeGetProperty(manObj, "Description"),
                            PnpDeviceID = SafeGetProperty(manObj, "PNPDeviceID"),
                            DriverVersion = SafeGetProperty(manObj, "DriverVersion"),
                            Status = SafeGetProperty(manObj, "Status"),
                            InfSection = SafeGetProperty(manObj, "InfSection"),
                            AdapterRam = memTmp
                        };
                        stringBuilder.AppendLine("\tWin32_VideoController detected:");
                        stringBuilder.AppendLine($"\t\tName {vidController.Name}");
                        stringBuilder.AppendLine($"\t\tDescription {vidController.Description}");
                        stringBuilder.AppendLine($"\t\tPNPDeviceID {vidController.PnpDeviceID}");
                        stringBuilder.AppendLine($"\t\tDriverVersion {vidController.DriverVersion}");
                        stringBuilder.AppendLine($"\t\tStatus {vidController.Status}");
                        stringBuilder.AppendLine($"\t\tInfSection {vidController.InfSection}");
                        stringBuilder.AppendLine($"\t\tAdapterRAM {vidController.AdapterRam}");

                        // check if controller ok
                        if (allVideoContollersOK && !vidController.Status.ToLower().Equals("ok"))
                        {
                            allVideoContollersOK = false;
                        }

                        avaliableVideoControllers.Add(vidController);
                    }
                    Helpers.ConsolePrint(Tag, stringBuilder.ToString());

                    if (warningsEnabled)
                    {
                        if (ConfigManager.GeneralConfig.ShowDriverVersionWarning && !allVideoContollersOK)
                        {
                            var msg = International.GetText("QueryVideoControllers_NOT_ALL_OK_Msg");
                            foreach (var vc in avaliableVideoControllers)
                            {
                                if (!vc.Status.ToLower().Equals("ok"))
                                {
                                    msg += Environment.NewLine
                                           + string.Format(
                                               International.GetText("QueryVideoControllers_NOT_ALL_OK_Msg_Append"),
                                               vc.Name, vc.Status, vc.PnpDeviceID);
                                }
                            }
                            MessageBox.Show(msg,
                                International.GetText("QueryVideoControllers_NOT_ALL_OK_Title"),
                                MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        }
                    }
                }

                public static bool HasNvidiaVideoController()
                {
                    return AvaliableVideoControllers.Any(vctrl => vctrl.Name.ToLower().Contains("nvidia"));
                }
            }

            private static class Cpu
            {
                public static void QueryCpus()
                {
                    Helpers.ConsolePrint(Tag, "QueryCpus START");
                    // get all CPUs
                    Available.CpusCount = CpuID.GetPhysicalProcessorCount();
                    Available.IsHyperThreadingEnabled = CpuID.IsHypeThreadingEnabled();

                    Helpers.ConsolePrint(Tag,
                        Available.IsHyperThreadingEnabled
                            ? "HyperThreadingEnabled = TRUE"
                            : "HyperThreadingEnabled = FALSE");

                    // get all cores (including virtual - HT can benefit mining)
                    var threadsPerCpu = CpuID.GetVirtualCoresCount() / Available.CpusCount;

                    if (!Helpers.Is64BitOperatingSystem)
                    {
                        if (ConfigManager.GeneralConfig.ShowDriverVersionWarning)
                        {
                            MessageBox.Show(International.GetText("Form_Main_msgbox_CPUMining64bitMsg"),
                                International.GetText("Warning_with_Exclamation"),
                                MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        }

                        Available.CpusCount = 0;
                    }

                    if (threadsPerCpu * Available.CpusCount > 64)
                    {
                        if (ConfigManager.GeneralConfig.ShowDriverVersionWarning)
                        {
                            MessageBox.Show(International.GetText("Form_Main_msgbox_CPUMining64CoresMsg"),
                                International.GetText("Warning_with_Exclamation"),
                                MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        }

                        Available.CpusCount = 0;
                    }

                    // TODO important move this to settings
                    var threadsPerCpuMask = threadsPerCpu;
                    Globals.ThreadsPerCpu = threadsPerCpu;

                    if (CpuUtils.IsCpuMiningCapable())
                    {
                        if (Available.CpusCount == 1)
                        {
                            Available.Devices.Add(
                                new CpuComputeDevice(0, "CPU0", CpuID.GetCpuName().Trim(), threadsPerCpu, 0,
                                    ++CpuCount)
                            );
                        }
                        else if (Available.CpusCount > 1)
                        {
                            for (var i = 0; i < Available.CpusCount; i++)
                            {
                                Available.Devices.Add(
                                    new CpuComputeDevice(i, "CPU" + i, CpuID.GetCpuName().Trim(), threadsPerCpu,
                                        CpuID.CreateAffinityMask(i, threadsPerCpuMask), ++CpuCount)
                                );
                            }
                        }
                    }

                    Helpers.ConsolePrint(Tag, "QueryCpus END");
                }
            }

            private static List<CudaDevice> _cudaDevices = new List<CudaDevice>();

            private static class Nvidia
            {
                private static string _queryCudaDevicesString = "";

                private static void QueryCudaDevicesOutputErrorDataReceived(object sender, DataReceivedEventArgs e)
                {
                    if (e.Data != null)
                    {
                        _queryCudaDevicesString += e.Data;
                    }
                }

                public static bool IsSkipNvidia()
                {
                    return ConfigManager.GeneralConfig.DeviceDetection.DisableDetectionNVIDIA;
                }

                public static void QueryCudaDevices()
                {
                    Helpers.ConsolePrint(Tag, "QueryCudaDevices START");
                    QueryCudaDevices(ref _cudaDevices);

                    if (_cudaDevices != null && _cudaDevices.Count != 0)
                    {
                        Available.HasNvidia = true;
                        var stringBuilder = new StringBuilder();
                        stringBuilder.AppendLine("");
                        stringBuilder.AppendLine("CudaDevicesDetection:");

                        // Enumerate NVAPI handles and map to busid
                        var idHandles = new Dictionary<int, NvPhysicalGpuHandle>();
                        if (NVAPI.IsAvailable)
                        {
                            var handles = new NvPhysicalGpuHandle[NVAPI.MAX_PHYSICAL_GPUS];
                            if (NVAPI.NvAPI_EnumPhysicalGPUs == null)
                            {
                                Helpers.ConsolePrint("NVAPI", "NvAPI_EnumPhysicalGPUs unavailable");
                            }
                            else
                            {
                                var status = NVAPI.NvAPI_EnumPhysicalGPUs(handles, out var _);
                                if (status != NvStatus.OK)
                                {
                                    Helpers.ConsolePrint("NVAPI", "Enum physical GPUs failed with status: " + status);
                                }
                                else
                                {
                                    foreach (var handle in handles)
                                    {
                                        var idStatus = NVAPI.NvAPI_GPU_GetBusID(handle, out var id);
                                        if (idStatus != NvStatus.EXPECTED_PHYSICAL_GPU_HANDLE)
                                        {
                                            if (idStatus != NvStatus.OK)
                                            {
                                                Helpers.ConsolePrint("NVAPI",
                                                    "Bus ID get failed with status: " + idStatus);
                                            }
                                            else
                                            {
                                                Helpers.ConsolePrint("NVAPI", "Found handle for busid " + id);
                                                idHandles[id] = handle;
                                            }
                                        }
                                    }
                                }
                            }
                        }

                        var nvmlInit = false;
                        try
                        {
                            var ret = NvmlNativeMethods.nvmlInit();
                            if (ret != nvmlReturn.Success)
                                throw new Exception($"NVML init failed with code {ret}");
                            nvmlInit = true;
                        }
                        catch (Exception e)
                        {
                            Helpers.ConsolePrint("NVML", e.ToString());
                        }

                        foreach (var cudaDev in _cudaDevices)
                        {
                            // check sm vesrions
                            bool isUnderSM21;
                            {
                                var isUnderSM2Major = cudaDev.SM_major < 2;
                                var isUnderSM1Minor = cudaDev.SM_minor < 1;
                                isUnderSM21 = isUnderSM2Major && isUnderSM1Minor;
                            }
                            //bool isOverSM6 = cudaDev.SM_major > 6;
                            var skip = isUnderSM21;
                            var skipOrAdd = skip ? "SKIPED" : "ADDED";
                            const string isDisabledGroupStr = ""; // TODO remove
                            var etherumCapableStr = cudaDev.IsEtherumCapable() ? "YES" : "NO";
                            stringBuilder.AppendLine($"\t{skipOrAdd} device{isDisabledGroupStr}:");
                            stringBuilder.AppendLine($"\t\tID: {cudaDev.DeviceID}");
                            stringBuilder.AppendLine($"\t\tBusID: {cudaDev.pciBusID}");
                            stringBuilder.AppendLine($"\t\tNAME: {cudaDev.GetName()}");
                            stringBuilder.AppendLine($"\t\tVENDOR: {cudaDev.VendorName}");
                            stringBuilder.AppendLine($"\t\tUUID: {cudaDev.UUID}");
                            stringBuilder.AppendLine($"\t\tSM: {cudaDev.SMVersionString}");
                            stringBuilder.AppendLine($"\t\tMEMORY: {cudaDev.DeviceGlobalMemory}");
                            stringBuilder.AppendLine($"\t\tETHEREUM: {etherumCapableStr}");

                            if (!skip)
                            {
                                DeviceGroupType group;
                                switch (cudaDev.SM_major)
                                {
                                    case 2:
                                        group = DeviceGroupType.NVIDIA_2_1;
                                        break;
                                    case 3:
                                        group = DeviceGroupType.NVIDIA_3_x;
                                        break;
                                    case 5:
                                        group = DeviceGroupType.NVIDIA_5_x;
                                        break;
                                    case 6:
                                        group = DeviceGroupType.NVIDIA_6_x;
                                        break;
                                    default:
                                        group = DeviceGroupType.NVIDIA_6_x;
                                        break;
                                }

                                var nvmlHandle = new nvmlDevice();

                                if (nvmlInit)
                                {
                                    var ret = NvmlNativeMethods.nvmlDeviceGetHandleByUUID(cudaDev.UUID, ref nvmlHandle);
                                    stringBuilder.AppendLine(
                                        "\t\tNVML HANDLE: " +
                                        $"{(ret == nvmlReturn.Success ? nvmlHandle.Pointer.ToString() : $"Failed with code ret {ret}")}");
                                }

                                idHandles.TryGetValue(cudaDev.pciBusID, out var handle);
                                Available.Devices.Add(
                                    new CudaComputeDevice(cudaDev, group, ++GpuCount, handle, nvmlHandle)
                                );
                            }
                        }
                        Helpers.ConsolePrint(Tag, stringBuilder.ToString());
                    }
                    Helpers.ConsolePrint(Tag, "QueryCudaDevices END");
                }

                public static void QueryCudaDevices(ref List<CudaDevice> cudaDevices)
                {
                    _queryCudaDevicesString = "";

                    var cudaDevicesDetection = new Process
                    {
                        StartInfo =
                        {
                            FileName = "CudaDeviceDetection.exe",
                            UseShellExecute = false,
                            RedirectStandardError = true,
                            RedirectStandardOutput = true,
                            CreateNoWindow = true
                        }
                    };
                    cudaDevicesDetection.OutputDataReceived += QueryCudaDevicesOutputErrorDataReceived;
                    cudaDevicesDetection.ErrorDataReceived += QueryCudaDevicesOutputErrorDataReceived;

                    const int waitTime = 30 * 1000; // 30seconds
                    try
                    {
                        if (!cudaDevicesDetection.Start())
                        {
                            Helpers.ConsolePrint(Tag, "CudaDevicesDetection process could not start");
                        }
                        else
                        {
                            cudaDevicesDetection.BeginErrorReadLine();
                            cudaDevicesDetection.BeginOutputReadLine();
                            if (cudaDevicesDetection.WaitForExit(waitTime))
                            {
                                cudaDevicesDetection.Close();
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        // TODO
                        Helpers.ConsolePrint(Tag, "CudaDevicesDetection threw Exception: " + ex.Message);
                    }
                    finally
                    {
                        if (_queryCudaDevicesString != "")
                        {
                            try
                            {
                                cudaDevices =
                                    JsonConvert.DeserializeObject<List<CudaDevice>>(_queryCudaDevicesString,
                                        Globals.JsonSettings);
                            }
                            catch { }

                            if (_cudaDevices == null || _cudaDevices.Count == 0)
                                Helpers.ConsolePrint(Tag,
                                    "CudaDevicesDetection found no devices. CudaDevicesDetection returned: " +
                                    _queryCudaDevicesString);
                        }
                    }
                }
            }

            private static List<OpenCLJsonData> _openCLJsonData = new List<OpenCLJsonData>();
            private static bool _isOpenCLQuerySuccess = false;

            private static class OpenCL
            {
                private static string _queryOpenCLDevicesString = "";

                private static void QueryOpenCLDevicesOutputErrorDataReceived(object sender, DataReceivedEventArgs e)
                {
                    if (e.Data != null)
                    {
                        _queryOpenCLDevicesString += e.Data;
                    }
                }

                public static void QueryOpenCLDevices()
                {
                    Helpers.ConsolePrint(Tag, "QueryOpenCLDevices START");
                    var openCLDevicesDetection = new Process
                    {
                        StartInfo =
                        {
                            FileName = "AMDOpenCLDeviceDetection.exe",
                            UseShellExecute = false,
                            RedirectStandardError = true,
                            RedirectStandardOutput = true,
                            CreateNoWindow = true
                        }
                    };
                    openCLDevicesDetection.OutputDataReceived += QueryOpenCLDevicesOutputErrorDataReceived;
                    openCLDevicesDetection.ErrorDataReceived += QueryOpenCLDevicesOutputErrorDataReceived;

                    const int waitTime = 30 * 1000; // 30seconds
                    try
                    {
                        if (!openCLDevicesDetection.Start())
                        {
                            Helpers.ConsolePrint(Tag, "AMDOpenCLDeviceDetection process could not start");
                        }
                        else
                        {
                            openCLDevicesDetection.BeginErrorReadLine();
                            openCLDevicesDetection.BeginOutputReadLine();
                            if (openCLDevicesDetection.WaitForExit(waitTime))
                            {
                                openCLDevicesDetection.Close();
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        // TODO
                        Helpers.ConsolePrint(Tag, "AMDOpenCLDeviceDetection threw Exception: " + ex.Message);
                    }
                    finally
                    {
                        if (_queryOpenCLDevicesString != "")
                        {
                            try
                            {
                                _openCLJsonData =
                                    JsonConvert.DeserializeObject<List<OpenCLJsonData>>(_queryOpenCLDevicesString,
                                        Globals.JsonSettings);
                            }
                            catch
                            {
                                _openCLJsonData = null;
                            }
                        }
                    }

                    if (_openCLJsonData == null)
                    {
                        Helpers.ConsolePrint(Tag,
                            "AMDOpenCLDeviceDetection found no devices. AMDOpenCLDeviceDetection returned: " +
                            _queryOpenCLDevicesString);
                    }
                    else
                    {
                        _isOpenCLQuerySuccess = true;
                        var stringBuilder = new StringBuilder();
                        stringBuilder.AppendLine("");
                        stringBuilder.AppendLine("AMDOpenCLDeviceDetection found devices success:");
                        foreach (var oclElem in _openCLJsonData)
                        {
                            stringBuilder.AppendLine($"\tFound devices for platform: {oclElem.PlatformName}");
                            foreach (var oclDev in oclElem.Devices)
                            {
                                stringBuilder.AppendLine("\t\tDevice:");
                                stringBuilder.AppendLine($"\t\t\tDevice ID {oclDev.DeviceID}");
                                stringBuilder.AppendLine($"\t\t\tDevice NAME {oclDev._CL_DEVICE_NAME}");
                                stringBuilder.AppendLine($"\t\t\tDevice TYPE {oclDev._CL_DEVICE_TYPE}");
                            }
                        }
                        Helpers.ConsolePrint(Tag, stringBuilder.ToString());
                    }
                    Helpers.ConsolePrint(Tag, "QueryOpenCLDevices END");
                }
            }

            public static List<OpenCLDevice> AmdDevices = new List<OpenCLDevice>();

            #endregion Helpers
        }

        public static class SystemSpecs
        {
            public static ulong FreePhysicalMemory;
            public static ulong FreeSpaceInPagingFiles;
            public static ulong FreeVirtualMemory;
            public static uint LargeSystemCache;
            public static uint MaxNumberOfProcesses;
            public static ulong MaxProcessMemorySize;

            public static uint NumberOfLicensedUsers;
            public static uint NumberOfProcesses;
            public static uint NumberOfUsers;
            public static uint OperatingSystemSKU;

            public static ulong SizeStoredInPagingFiles;

            public static uint SuiteMask;

            public static ulong TotalSwapSpaceSize;
            public static ulong TotalVirtualMemorySize;
            public static ulong TotalVisibleMemorySize;


            public static void QueryAndLog()
            {
                var winQuery = new ObjectQuery("SELECT * FROM Win32_OperatingSystem");

                var searcher = new ManagementObjectSearcher(winQuery);

                foreach (ManagementObject item in searcher.Get())
                {
                    if (item["FreePhysicalMemory"] != null)
                        ulong.TryParse(item["FreePhysicalMemory"].ToString(), out FreePhysicalMemory);
                    if (item["FreeSpaceInPagingFiles"] != null)
                        ulong.TryParse(item["FreeSpaceInPagingFiles"].ToString(), out FreeSpaceInPagingFiles);
                    if (item["FreeVirtualMemory"] != null)
                        ulong.TryParse(item["FreeVirtualMemory"].ToString(), out FreeVirtualMemory);
                    if (item["LargeSystemCache"] != null)
                        uint.TryParse(item["LargeSystemCache"].ToString(), out LargeSystemCache);
                    if (item["MaxNumberOfProcesses"] != null)
                        uint.TryParse(item["MaxNumberOfProcesses"].ToString(), out MaxNumberOfProcesses);
                    if (item["MaxProcessMemorySize"] != null)
                        ulong.TryParse(item["MaxProcessMemorySize"].ToString(), out MaxProcessMemorySize);
                    if (item["NumberOfLicensedUsers"] != null)
                        uint.TryParse(item["NumberOfLicensedUsers"].ToString(), out NumberOfLicensedUsers);
                    if (item["NumberOfProcesses"] != null)
                        uint.TryParse(item["NumberOfProcesses"].ToString(), out NumberOfProcesses);
                    if (item["NumberOfUsers"] != null)
                        uint.TryParse(item["NumberOfUsers"].ToString(), out NumberOfUsers);
                    if (item["OperatingSystemSKU"] != null)
                        uint.TryParse(item["OperatingSystemSKU"].ToString(), out OperatingSystemSKU);
                    if (item["SizeStoredInPagingFiles"] != null)
                        ulong.TryParse(item["SizeStoredInPagingFiles"].ToString(), out SizeStoredInPagingFiles);
                    if (item["SuiteMask"] != null) uint.TryParse(item["SuiteMask"].ToString(), out SuiteMask);
                    if (item["TotalSwapSpaceSize"] != null)
                        ulong.TryParse(item["TotalSwapSpaceSize"].ToString(), out TotalSwapSpaceSize);
                    if (item["TotalVirtualMemorySize"] != null)
                        ulong.TryParse(item["TotalVirtualMemorySize"].ToString(), out TotalVirtualMemorySize);
                    if (item["TotalVisibleMemorySize"] != null)
                        ulong.TryParse(item["TotalVisibleMemorySize"].ToString(), out TotalVisibleMemorySize);
                    // log
                    Helpers.ConsolePrint("SystemSpecs", $"FreePhysicalMemory = {FreePhysicalMemory}");
                    Helpers.ConsolePrint("SystemSpecs", $"FreeSpaceInPagingFiles = {FreeSpaceInPagingFiles}");
                    Helpers.ConsolePrint("SystemSpecs", $"FreeVirtualMemory = {FreeVirtualMemory}");
                    Helpers.ConsolePrint("SystemSpecs", $"LargeSystemCache = {LargeSystemCache}");
                    Helpers.ConsolePrint("SystemSpecs", $"MaxNumberOfProcesses = {MaxNumberOfProcesses}");
                    Helpers.ConsolePrint("SystemSpecs", $"MaxProcessMemorySize = {MaxProcessMemorySize}");
                    Helpers.ConsolePrint("SystemSpecs", $"NumberOfLicensedUsers = {NumberOfLicensedUsers}");
                    Helpers.ConsolePrint("SystemSpecs", $"NumberOfProcesses = {NumberOfProcesses}");
                    Helpers.ConsolePrint("SystemSpecs", $"NumberOfUsers = {NumberOfUsers}");
                    Helpers.ConsolePrint("SystemSpecs", $"OperatingSystemSKU = {OperatingSystemSKU}");
                    Helpers.ConsolePrint("SystemSpecs", $"SizeStoredInPagingFiles = {SizeStoredInPagingFiles}");
                    Helpers.ConsolePrint("SystemSpecs", $"SuiteMask = {SuiteMask}");
                    Helpers.ConsolePrint("SystemSpecs", $"TotalSwapSpaceSize = {TotalSwapSpaceSize}");
                    Helpers.ConsolePrint("SystemSpecs", $"TotalVirtualMemorySize = {TotalVirtualMemorySize}");
                    Helpers.ConsolePrint("SystemSpecs", $"TotalVisibleMemorySize = {TotalVisibleMemorySize}");
                }
            }
        }

        public static class Available
        {
            public static bool HasNvidia = false;
            public static bool HasAmd = false;
            public static bool HasCpu = false;
            public static int CpusCount = 0;

            public static int AvailCpus
            {
                get { return Devices.Count(d => d.DeviceType == DeviceType.CPU); }
            }

            public static int AvailNVGpus
            {
                get { return Devices.Count(d => d.DeviceType == DeviceType.NVIDIA); }
            }

            public static int AvailAmdGpus
            {
                get { return Devices.Count(d => d.DeviceType == DeviceType.AMD); }
            }

            public static int AvailGpUs => AvailAmdGpus + AvailNVGpus;
            public static int AmdOpenCLPlatformNum = -1;
            public static bool IsHyperThreadingEnabled = false;

            public static ulong NvidiaRamSum = 0;
            public static ulong AmdRamSum = 0;

            public static readonly List<ComputeDevice> Devices = new List<ComputeDevice>();

            // methods
            public static ComputeDevice GetDeviceWithUuid(string uuid)
            {
                return Devices.FirstOrDefault(dev => uuid == dev.Uuid);
            }

            public static List<ComputeDevice> GetSameDevicesTypeAsDeviceWithUuid(string uuid)
            {
                var compareDev = GetDeviceWithUuid(uuid);
                return (from dev in Devices
                    where uuid != dev.Uuid && compareDev.DeviceType == dev.DeviceType
                    select GetDeviceWithUuid(dev.Uuid)).ToList();
            }

            public static ComputeDevice GetCurrentlySelectedComputeDevice(int index, bool unique)
            {
                return Devices[index];
            }

            public static int GetCountForType(DeviceType type)
            {
                return Devices.Count(device => device.DeviceType == type);
            }
        }

        public static class Group
        {
            public static void DisableCpuGroup()
            {
                foreach (var device in Available.Devices)
                {
                    if (device.DeviceType == DeviceType.CPU)
                    {
                        device.Enabled = false;
                    }
                }
            }

            public static bool ContainsAmdGpus
            {
                get { return Available.Devices.Any(device => device.DeviceType == DeviceType.AMD); }
            }

            public static bool ContainsGpus
            {
                get
                {
                    return Available.Devices.Any(device =>
                        device.DeviceType == DeviceType.NVIDIA || device.DeviceType == DeviceType.AMD);
                }
            }

            public static void UncheckedCpu()
            {
                // Auto uncheck CPU if any GPU is found
                if (ContainsGpus) DisableCpuGroup();
            }
        }
    }
}

using ATI.ADL;
using Newtonsoft.Json;
using NiceHashMiner.Configs;
using NiceHashMiner.Enums;
using NiceHashMiner.Interfaces;
using NVIDIA.NVAPI;
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
using NiceHashMiner.Forms;

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
            private static int _cpuCount = 0;

            private static int _gpuCount = 0;

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
                        P.WaitForExit();

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

            private static DialogResult showMessageBox(string text, string caption, MessageBoxButtons buttons, MessageBoxIcon icon = MessageBoxIcon.None) {
                if (MessageNotifier != null) return MessageNotifier.ShowMessageBox(text, caption, buttons, icon);
                return DialogResult.Cancel;
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
                    Amd.QueryAmd();
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
                    showMessageBox(string.Format(
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
                    showMessageBox(string.Format(
                            International.GetText("Compute_Device_Query_Manager_NVIDIA_Driver_Recomended"),
                            recomendDrvier, nvdriverString, recomendDrvier),
                        International.GetText("Compute_Device_Query_Manager_NVIDIA_RecomendedDriver_Title"),
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }

                // no devices found
                if (Avaliable.AllAvaliableDevices.Count <= 0)
                {
                    var result = showMessageBox(International.GetText("Compute_Device_Query_Manager_No_Devices"),
                        International.GetText("Compute_Device_Query_Manager_No_Devices_Title"),
                        MessageBoxButtons.OKCancel, MessageBoxIcon.Warning);
                    if (result == DialogResult.OK)
                    {
                        Process.Start(Links.NhmNoDevHelp);
                    }
                }

                // create AMD bus ordering for Claymore
                var amdDevices = Avaliable.AllAvaliableDevices.FindAll((a) => a.DeviceType == DeviceType.AMD);
                amdDevices.Sort((a, b) => a.BusID.CompareTo(b.BusID));
                for (var i = 0; i < amdDevices.Count; i++)
                {
                    amdDevices[i].IDByBus = i;
                }
                //create NV bus ordering for Claymore
                var nvDevices = Avaliable.AllAvaliableDevices.FindAll((a) => a.DeviceType == DeviceType.NVIDIA);
                nvDevices.Sort((a, b) => a.BusID.CompareTo(b.BusID));
                for (var i = 0; i < nvDevices.Count; i++)
                {
                    nvDevices[i].IDByBus = i;
                }

                // get GPUs RAM sum
                // bytes
                Avaliable.NvidiaRamSum = 0;
                Avaliable.AmdRamSum = 0;
                foreach (var dev in Avaliable.AllAvaliableDevices)
                {
                    if (dev.DeviceType == DeviceType.NVIDIA)
                    {
                        Avaliable.NvidiaRamSum += dev.GpuRam;
                    }
                    else if (dev.DeviceType == DeviceType.AMD)
                    {
                        Avaliable.AmdRamSum += dev.GpuRam;
                    }
                }
                // Make gpu ram needed not larger than 4GB per GPU
                var totalGpuRam = Math.Min((Avaliable.NvidiaRamSum + Avaliable.AmdRamSum) * 0.6 / 1024,
                    (double) Avaliable.AvailGpUs * 4 * 1024 * 1024);
                double totalSysRam = SystemSpecs.FreePhysicalMemory + SystemSpecs.FreeVirtualMemory;
                // check
                if (ConfigManager.GeneralConfig.ShowDriverVersionWarning && totalSysRam < totalGpuRam)
                {
                    Helpers.ConsolePrint(Tag, "virtual memory size BAD");
                    showMessageBox(International.GetText("VirtualMemorySize_BAD"),
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

            private class VideoControllerData
            {
                public string Name { get; set; }
                public string Description { get; set; }
                public string PnpDeviceID { get; set; }
                public string DriverVersion { get; set; }
                public string Status { get; set; }
                public string InfSection { get; set; } // get arhitecture
                public ulong AdapterRam { get; set; }
            }

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
                            showMessageBox(msg,
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
                    Avaliable.CpusCount = CpuID.GetPhysicalProcessorCount();
                    Avaliable.IsHyperThreadingEnabled = CpuID.IsHypeThreadingEnabled();

                    Helpers.ConsolePrint(Tag,
                        Avaliable.IsHyperThreadingEnabled
                            ? "HyperThreadingEnabled = TRUE"
                            : "HyperThreadingEnabled = FALSE");

                    // get all cores (including virtual - HT can benefit mining)
                    var threadsPerCpu = CpuID.GetVirtualCoresCount() / Avaliable.CpusCount;

                    if (!Helpers.Is64BitOperatingSystem)
                    {
                        showMessageBox(International.GetText("Form_Main_msgbox_CPUMining64bitMsg"),
                            International.GetText("Warning_with_Exclamation"),
                            MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        Avaliable.CpusCount = 0;
                    }

                    if (threadsPerCpu * Avaliable.CpusCount > 64)
                    {
                        showMessageBox(International.GetText("Form_Main_msgbox_CPUMining64CoresMsg"),
                            International.GetText("Warning_with_Exclamation"),
                            MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        Avaliable.CpusCount = 0;
                    }

                    // TODO important move this to settings
                    var threadsPerCpuMask = threadsPerCpu;
                    Globals.ThreadsPerCpu = threadsPerCpu;

                    if (CpuUtils.IsCpuMiningCapable())
                    {
                        if (Avaliable.CpusCount == 1)
                        {
                            Avaliable.AllAvaliableDevices.Add(
                                new CpuComputeDevice(0, "CPU0", CpuID.GetCpuName().Trim(), threadsPerCpu, 0,
                                    ++_cpuCount)
                            );
                        }
                        else if (Avaliable.CpusCount > 1)
                        {
                            for (var i = 0; i < Avaliable.CpusCount; i++)
                            {
                                Avaliable.AllAvaliableDevices.Add(
                                    new CpuComputeDevice(i, "CPU" + i, CpuID.GetCpuName().Trim(), threadsPerCpu,
                                        CpuID.CreateAffinityMask(i, threadsPerCpuMask), ++_cpuCount)
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
                        Avaliable.HasNvidia = true;
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
                                idHandles.TryGetValue(cudaDev.pciBusID, out var handle);
                                Avaliable.AllAvaliableDevices.Add(
                                    new CudaComputeDevice(cudaDev, group, ++_gpuCount, handle)
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

            private class OpenCLJsonData_t
            {
                public string PlatformName = "NONE";
                public int PlatformNum = 0;
                public List<OpenCLDevice> Devices = new List<OpenCLDevice>();
            }

            private static List<OpenCLJsonData_t> _openCLJsonData = new List<OpenCLJsonData_t>();
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
                                    JsonConvert.DeserializeObject<List<OpenCLJsonData_t>>(_queryOpenCLDevicesString,
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

            private static class Amd
            {
                public static void QueryAmd()
                {
                    const int amdVendorID = 1002;
                    Helpers.ConsolePrint(Tag, "QueryAMD START");

                    #region AMD driver check, ADL returns 0

                    // check the driver version bool EnableOptimizedVersion = true;
                    var deviceDriverOld = new Dictionary<string, bool>();
                    var deviceDriverNoNeoscryptLyra2RE = new Dictionary<string, bool>();
                    var showWarningDialog = false;

                    foreach (var vidContrllr in AvaliableVideoControllers)
                    {
                        Helpers.ConsolePrint(Tag,
                            $"Checking AMD device (driver): {vidContrllr.Name} ({vidContrllr.DriverVersion})");

                        deviceDriverOld[vidContrllr.Name] = false;
                        deviceDriverNoNeoscryptLyra2RE[vidContrllr.Name] = false;
                        var sgminerNoNeoscryptLyra2RE = new Version("21.19.164.1");
                        // TODO checking radeon drivers only?
                        if ((vidContrllr.Name.Contains("AMD") || vidContrllr.Name.Contains("Radeon")) &&
                            showWarningDialog == false)
                        {
                            var amdDriverVersion = new Version(vidContrllr.DriverVersion);

                            if (!ConfigManager.GeneralConfig.ForceSkipAMDNeoscryptLyraCheck)
                            {
                                var greaterOrEqual = amdDriverVersion.CompareTo(sgminerNoNeoscryptLyra2RE) >= 0;
                                if (greaterOrEqual)
                                {
                                    deviceDriverNoNeoscryptLyra2RE[vidContrllr.Name] = true;
                                    Helpers.ConsolePrint(Tag,
                                        "Driver version seems to be " + sgminerNoNeoscryptLyra2RE +
                                        " or higher. NeoScrypt and Lyra2REv2 will be removed from list");
                                }
                            }


                            if (amdDriverVersion.Major < 15)
                            {
                                showWarningDialog = true;
                                deviceDriverOld[vidContrllr.Name] = true;
                                Helpers.ConsolePrint(Tag,
                                    "WARNING!!! Old AMD GPU driver detected! All optimized versions disabled, mining " +
                                    "speed will not be optimal. Consider upgrading AMD GPU driver. Recommended AMD GPU driver version is 15.7.1.");
                            }
                        }
                    }
                    if (ConfigManager.GeneralConfig.ShowDriverVersionWarning && showWarningDialog)
                    {
                        Form warningDialog = new DriverVersionConfirmationDialog();
                        warningDialog.ShowDialog();
                        warningDialog = null;
                    }

                    #endregion // AMD driver check

                    // get platform version
                    ShowMessageAndStep(International.GetText("Compute_Device_Query_Manager_AMD_Query"));
                    var amdOclDevices = new List<OpenCLDevice>();
                    if (_isOpenCLQuerySuccess)
                    {
                        var amdPlatformNumFound = false;
                        foreach (var oclEl in _openCLJsonData)
                        {
                            if (!oclEl.PlatformName.Contains("AMD") && !oclEl.PlatformName.Contains("amd")) continue;
                            amdPlatformNumFound = true;
                            var amdOpenCLPlatformStringKey = oclEl.PlatformName;
                            Avaliable.AmdOpenCLPlatformNum = oclEl.PlatformNum;
                            amdOclDevices = oclEl.Devices;
                            Helpers.ConsolePrint(Tag,
                                $"AMD platform found: Key: {amdOpenCLPlatformStringKey}, Num: {Avaliable.AmdOpenCLPlatformNum}");
                            break;
                        }
                        if (amdPlatformNumFound)
                        {
                            // get only AMD gpus
                            {
                                foreach (var oclDev in amdOclDevices)
                                {
                                    if (oclDev._CL_DEVICE_TYPE.Contains("GPU"))
                                    {
                                        AmdDevices.Add(oclDev);
                                    }
                                }
                            }

                            if (AmdDevices.Count == 0)
                            {
                                Helpers.ConsolePrint(Tag, "AMD GPUs count is 0");
                            }
                            else
                            {
                                Helpers.ConsolePrint(Tag, "AMD GPUs count : " + AmdDevices.Count);
                                Helpers.ConsolePrint(Tag, "AMD Getting device name and serial from ADL");
                                // ADL
                                var isAdlInit = true;
                                // ADL does not get devices in order map devices by bus number
                                // bus id, <name, uuid>
                                var busIDsInfo = new Dictionary<int, Tuple<string, string, string, int>>();
                                var amdDeviceName = new List<string>();
                                var amdDeviceUuid = new List<string>();
                                try
                                {
                                    var adlRet = -1;
                                    var numberOfAdapters = 0;
                                    if (null != ADL.ADL_Main_Control_Create)
                                        // Second parameter is 1: Get only the present adapters
                                        adlRet = ADL.ADL_Main_Control_Create(ADL.ADL_Main_Memory_Alloc, 1);
                                    if (ADL.ADL_SUCCESS == adlRet)
                                    {
                                        ADL.ADL_Adapter_NumberOfAdapters_Get?.Invoke(ref numberOfAdapters);
                                        Helpers.ConsolePrint(Tag, "Number Of Adapters: " + numberOfAdapters);

                                        if (0 < numberOfAdapters)
                                        {
                                            // Get OS adpater info from ADL
                                            var osAdapterInfoData = new ADLAdapterInfoArray();

                                            if (null != ADL.ADL_Adapter_AdapterInfo_Get)
                                            {
                                                var size = Marshal.SizeOf(osAdapterInfoData);
                                                var adapterBuffer = Marshal.AllocCoTaskMem(size);
                                                Marshal.StructureToPtr(osAdapterInfoData, adapterBuffer, false);

                                                if (null != ADL.ADL_Adapter_AdapterInfo_Get)
                                                {
                                                    adlRet = ADL.ADL_Adapter_AdapterInfo_Get(adapterBuffer, size);
                                                    if (ADL.ADL_SUCCESS == adlRet)
                                                    {
                                                        osAdapterInfoData =
                                                            (ADLAdapterInfoArray) Marshal.PtrToStructure(adapterBuffer,
                                                                osAdapterInfoData.GetType());
                                                        var isActive = 0;

                                                        for (var i = 0; i < numberOfAdapters; i++)
                                                        {
                                                            // Check if the adapter is active
                                                            if (null != ADL.ADL_Adapter_Active_Get)
                                                                adlRet = ADL.ADL_Adapter_Active_Get(
                                                                    osAdapterInfoData.ADLAdapterInfo[i].AdapterIndex,
                                                                    ref isActive);

                                                            if (ADL.ADL_SUCCESS == adlRet)
                                                            {
                                                                // we are looking for amd
                                                                // TODO check discrete and integrated GPU separation
                                                                var vendorID = osAdapterInfoData.ADLAdapterInfo[i]
                                                                    .VendorID;
                                                                var devName = osAdapterInfoData.ADLAdapterInfo[i]
                                                                    .AdapterName;
                                                                if (vendorID == amdVendorID
                                                                    || devName.ToLower().Contains("amd")
                                                                    || devName.ToLower().Contains("radeon")
                                                                    || devName.ToLower().Contains("firepro"))
                                                                {
                                                                    var pnpStr = osAdapterInfoData.ADLAdapterInfo[i]
                                                                        .PNPString;
                                                                    // find vi controller pnp
                                                                    var infSection = "";
                                                                    foreach (var vCtrl in AvaliableVideoControllers)
                                                                    {
                                                                        if (vCtrl.PnpDeviceID == pnpStr)
                                                                        {
                                                                            infSection = vCtrl.InfSection;
                                                                        }
                                                                    }

                                                                    var backSlashLast = pnpStr.LastIndexOf('\\');
                                                                    var serial = pnpStr.Substring(backSlashLast,
                                                                        pnpStr.Length - backSlashLast);
                                                                    var end0 = serial.IndexOf('&');
                                                                    var end1 = serial.IndexOf('&', end0 + 1);
                                                                    // get serial
                                                                    serial = serial.Substring(end0 + 1,
                                                                        (end1 - end0) - 1);

                                                                    var udid = osAdapterInfoData.ADLAdapterInfo[i].UDID;
                                                                    const int pciVenIDStrSize =
                                                                        21; // PCI_VEN_XXXX&DEV_XXXX
                                                                    var uuid = udid.Substring(0, pciVenIDStrSize) +
                                                                               "_" + serial;
                                                                    var budId = osAdapterInfoData.ADLAdapterInfo[i]
                                                                        .BusNumber;
                                                                    var index = osAdapterInfoData.ADLAdapterInfo[i]
                                                                        .AdapterIndex;
                                                                    if (!amdDeviceUuid.Contains(uuid))
                                                                    {
                                                                        try
                                                                        {
                                                                            Helpers.ConsolePrint(Tag,
                                                                                $"ADL device added BusNumber:{budId}  NAME:{devName}  UUID:{uuid}");
                                                                        }
                                                                        catch (Exception e)
                                                                        {
                                                                            Helpers.ConsolePrint(Tag, e.Message);
                                                                        }

                                                                        amdDeviceUuid.Add(uuid);
                                                                        //_busIds.Add(OSAdapterInfoData.ADLAdapterInfo[i].BusNumber);
                                                                        amdDeviceName.Add(devName);
                                                                        if (!busIDsInfo.ContainsKey(budId))
                                                                        {
                                                                            var nameUuid =
                                                                                new Tuple<string, string, string, int>(
                                                                                    devName, uuid, infSection, index);
                                                                            busIDsInfo.Add(budId, nameUuid);
                                                                        }
                                                                    }
                                                                }
                                                            }
                                                        }
                                                    }
                                                    else
                                                    {
                                                        Helpers.ConsolePrint(Tag,
                                                            "ADL_Adapter_AdapterInfo_Get() returned error code " +
                                                            adlRet);
                                                        isAdlInit = false;
                                                    }
                                                }
                                                // Release the memory for the AdapterInfo structure
                                                if (IntPtr.Zero != adapterBuffer)
                                                    Marshal.FreeCoTaskMem(adapterBuffer);
                                            }
                                        }
                                        if (null != ADL.ADL_Main_Control_Destroy && numberOfAdapters <= 0)
                                            // Close ADL if it found no AMD devices
                                            ADL.ADL_Main_Control_Destroy();
                                    }
                                    else
                                    {
                                        // TODO
                                        Helpers.ConsolePrint(Tag,
                                            "ADL_Main_Control_Create() returned error code " + adlRet);
                                        Helpers.ConsolePrint(Tag, "Check if ADL is properly installed!");
                                        isAdlInit = false;
                                    }
                                }
                                catch (Exception ex)
                                {
                                    Helpers.ConsolePrint(Tag, "AMD ADL exception: " + ex.Message);
                                    isAdlInit = false;
                                }

                                var isBusIDOk = true;
                                // check if buss ids are unique and different from -1
                                {
                                    var busIDs = new HashSet<int>();
                                    // Override AMD bus IDs
                                    var overrides = ConfigManager.GeneralConfig.OverrideAMDBusIds.Split(',');
                                    for (var i = 0; i < AmdDevices.Count; i++)
                                    {
                                        var amdOclDev = AmdDevices[i];
                                        if (overrides.Count() > i &&
                                            int.TryParse(overrides[i], out var overrideBus) &&
                                            overrideBus >= 0)
                                        {
                                            amdOclDev.AMD_BUS_ID = overrideBus;
                                        }
                                        if (amdOclDev.AMD_BUS_ID < 0 || !busIDsInfo.ContainsKey(amdOclDev.AMD_BUS_ID))
                                        {
                                            isBusIDOk = false;
                                            break;
                                        }
                                        busIDs.Add(amdOclDev.AMD_BUS_ID);
                                    }
                                    // check if unique
                                    isBusIDOk = isBusIDOk && busIDs.Count == AmdDevices.Count;
                                }
                                // print BUS id status
                                Helpers.ConsolePrint(Tag,
                                    isBusIDOk
                                        ? "AMD Bus IDs are unique and valid. OK"
                                        : "AMD Bus IDs IS INVALID. Using fallback AMD detection mode");

                                ///////
                                // AMD device creation (in NHM context)
                                if (isAdlInit && isBusIDOk)
                                {
                                    Helpers.ConsolePrint(Tag, "Using AMD device creation DEFAULT Reliable mappings");
                                    Helpers.ConsolePrint(Tag,
                                        AmdDevices.Count == amdDeviceUuid.Count
                                            ? "AMD OpenCL and ADL AMD query COUNTS GOOD/SAME"
                                            : "AMD OpenCL and ADL AMD query COUNTS DIFFERENT/BAD");
                                    var stringBuilder = new StringBuilder();
                                    stringBuilder.AppendLine("");
                                    stringBuilder.AppendLine("QueryAMD [DEFAULT query] devices: ");
                                    foreach (var dev in AmdDevices)
                                    {
                                        Avaliable.HasAmd = true;

                                        var busID = dev.AMD_BUS_ID;
                                        if (busID != -1 && busIDsInfo.ContainsKey(busID))
                                        {
                                            var deviceName = busIDsInfo[busID].Item1;
                                            var newAmdDev = new AmdGpuDevice(dev, deviceDriverOld[deviceName],
                                                busIDsInfo[busID].Item3, deviceDriverNoNeoscryptLyra2RE[deviceName])
                                            {
                                                DeviceName = deviceName,
                                                UUID = busIDsInfo[busID].Item2,
                                                AdapterIndex = busIDsInfo[busID].Item4
                                            };
                                            var isDisabledGroup = ConfigManager.GeneralConfig.DeviceDetection
                                                .DisableDetectionAMD;
                                            var skipOrAdd = isDisabledGroup ? "SKIPED" : "ADDED";
                                            var isDisabledGroupStr = isDisabledGroup ? " (AMD group disabled)" : "";
                                            var etherumCapableStr = newAmdDev.IsEtherumCapable() ? "YES" : "NO";

                                            Avaliable.AllAvaliableDevices.Add(
                                                new AmdComputeDevice(newAmdDev, ++_gpuCount, false));
                                            // just in case 
                                            try
                                            {
                                                stringBuilder.AppendLine($"\t{skipOrAdd} device{isDisabledGroupStr}:");
                                                stringBuilder.AppendLine($"\t\tID: {newAmdDev.DeviceID}");
                                                stringBuilder.AppendLine($"\t\tNAME: {newAmdDev.DeviceName}");
                                                stringBuilder.AppendLine($"\t\tCODE_NAME: {newAmdDev.Codename}");
                                                stringBuilder.AppendLine($"\t\tUUID: {newAmdDev.UUID}");
                                                stringBuilder.AppendLine(
                                                    $"\t\tMEMORY: {newAmdDev.DeviceGlobalMemory}");
                                                stringBuilder.AppendLine($"\t\tETHEREUM: {etherumCapableStr}");
                                            }
                                            catch { }
                                        }
                                        else
                                        {
                                            stringBuilder.AppendLine($"\tDevice not added, Bus No. {busID} not found:");
                                        }
                                    }
                                    Helpers.ConsolePrint(Tag, stringBuilder.ToString());
                                }
                                else
                                {
                                    Helpers.ConsolePrint(Tag, "Using AMD device creation FALLBACK UnReliable mappings");
                                    var stringBuilder = new StringBuilder();
                                    stringBuilder.AppendLine("");
                                    stringBuilder.AppendLine("QueryAMD [FALLBACK query] devices: ");

                                    // get video AMD controllers and sort them by RAM
                                    // (find a way to get PCI BUS Numbers from PNPDeviceID)
                                    var amdVideoControllers = AvaliableVideoControllers.Where(vcd =>
                                        vcd.Name.ToLower().Contains("amd") || vcd.Name.ToLower().Contains("radeon") ||
                                        vcd.Name.ToLower().Contains("firepro")).ToList();
                                    // sort by ram not ideal 
                                    amdVideoControllers.Sort((a, b) => (int) (a.AdapterRam - b.AdapterRam));
                                    AmdDevices.Sort((a, b) =>
                                        (int) (a._CL_DEVICE_GLOBAL_MEM_SIZE - b._CL_DEVICE_GLOBAL_MEM_SIZE));
                                    var minCount = Math.Min(amdVideoControllers.Count, AmdDevices.Count);

                                    for (var i = 0; i < minCount; ++i)
                                    {
                                        Avaliable.HasAmd = true;

                                        var deviceName = amdVideoControllers[i].Name;
                                        if (amdVideoControllers[i].InfSection == null)
                                            amdVideoControllers[i].InfSection = "";
                                        var newAmdDev = new AmdGpuDevice(AmdDevices[i], deviceDriverOld[deviceName],
                                            amdVideoControllers[i].InfSection,
                                            deviceDriverNoNeoscryptLyra2RE[deviceName])
                                        {
                                            DeviceName = deviceName,
                                            UUID = "UNUSED"
                                        };
                                        var isDisabledGroup = ConfigManager.GeneralConfig.DeviceDetection
                                            .DisableDetectionAMD;
                                        var skipOrAdd = isDisabledGroup ? "SKIPED" : "ADDED";
                                        var isDisabledGroupStr = isDisabledGroup ? " (AMD group disabled)" : "";
                                        var etherumCapableStr = newAmdDev.IsEtherumCapable() ? "YES" : "NO";

                                        Avaliable.AllAvaliableDevices.Add(
                                            new AmdComputeDevice(newAmdDev, ++_gpuCount, true));
                                        // just in case 
                                        try
                                        {
                                            stringBuilder.AppendLine($"\t{skipOrAdd} device{isDisabledGroupStr}:");
                                            stringBuilder.AppendLine($"\t\tID: {newAmdDev.DeviceID}");
                                            stringBuilder.AppendLine($"\t\tNAME: {newAmdDev.DeviceName}");
                                            stringBuilder.AppendLine($"\t\tCODE_NAME: {newAmdDev.Codename}");
                                            stringBuilder.AppendLine($"\t\tUUID: {newAmdDev.UUID}");
                                            stringBuilder.AppendLine(
                                                $"\t\tMEMORY: {newAmdDev.DeviceGlobalMemory}");
                                            stringBuilder.AppendLine($"\t\tETHEREUM: {etherumCapableStr}");
                                        }
                                        catch { }
                                    }
                                    Helpers.ConsolePrint(Tag, stringBuilder.ToString());
                                }
                            }
                        } // end is amdPlatformNumFound
                    } // end is OpenCLSuccess
                    Helpers.ConsolePrint(Tag, "QueryAMD END");
                }
            }

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

        public static class Avaliable
        {
            public static bool HasNvidia = false;
            public static bool HasAmd = false;
            public static bool HasCpu = false;
            public static int CpusCount = 0;

            public static int AvailCpus
            {
                get { return AllAvaliableDevices.Count(d => d.DeviceType == DeviceType.CPU); }
            }

            public static int AvailNVGpus
            {
                get { return AllAvaliableDevices.Count(d => d.DeviceType == DeviceType.NVIDIA); }
            }

            public static int AvailAmdGpus
            {
                get { return AllAvaliableDevices.Count(d => d.DeviceType == DeviceType.AMD); }
            }

            public static int AvailGpUs => AvailAmdGpus + AvailNVGpus;
            public static int AmdOpenCLPlatformNum = -1;
            public static bool IsHyperThreadingEnabled = false;

            public static ulong NvidiaRamSum = 0;
            public static ulong AmdRamSum = 0;

            public static List<ComputeDevice> AllAvaliableDevices = new List<ComputeDevice>();

            // methods
            public static ComputeDevice GetDeviceWithUuid(string uuid)
            {
                return AllAvaliableDevices.FirstOrDefault(dev => uuid == dev.Uuid);
            }

            public static List<ComputeDevice> GetSameDevicesTypeAsDeviceWithUuid(string uuid)
            {
                var compareDev = GetDeviceWithUuid(uuid);
                return (from dev in AllAvaliableDevices
                    where uuid != dev.Uuid && compareDev.DeviceType == dev.DeviceType
                    select GetDeviceWithUuid(dev.Uuid)).ToList();
            }

            public static ComputeDevice GetCurrentlySelectedComputeDevice(int index, bool unique)
            {
                return AllAvaliableDevices[index];
            }

            public static int GetCountForType(DeviceType type)
            {
                return AllAvaliableDevices.Count(device => device.DeviceType == type);
            }
        }

        public static class Group
        {
            public static void DisableCpuGroup()
            {
                foreach (var device in Avaliable.AllAvaliableDevices)
                {
                    if (device.DeviceType == DeviceType.CPU)
                    {
                        device.Enabled = false;
                    }
                }
            }

            public static bool ContainsAmdGpus
            {
                get { return Avaliable.AllAvaliableDevices.Any(device => device.DeviceType == DeviceType.AMD); }
            }

            public static bool ContainsGpus
            {
                get
                {
                    return Avaliable.AllAvaliableDevices.Any(device =>
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

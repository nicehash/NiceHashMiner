using ManagedCuda.Nvml;
using Newtonsoft.Json;
using NiceHashMiner.Configs;
using NiceHashMiner.PInvoke;
using NiceHashMinerLegacy.Common.Enums;
using NVIDIA.NVAPI;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace NiceHashMiner.Devices.Querying
{
    internal static class NvidiaQuery
    {
        private const string Tag = "NvidiaQuery";

        private static bool tryAddNvmlToEnvPathCalled = false;

        public static IReadOnlyList<CudaDevice> CudaDevices { get; private set; }

        private static Timer _cudaCheckTimer;
        private static bool _isInit;

        static NvidiaQuery()
        {
        }

        #region Query devices

        public static int QueryCudaDevices()
        {
            Helpers.ConsolePrint(Tag, "QueryCudaDevices START");

            if (!TryQueryCudaDevices(out var cudaDevs))
            {
                Helpers.ConsolePrint(Tag, "QueryCudaDevices END");
                return 0;
            }
            
            var stringBuilder = new StringBuilder();
            stringBuilder.AppendLine("");
            stringBuilder.AppendLine("CudaDevicesDetection:");

            // Enumerate NVAPI handles and map to busid
            var idHandles = InitNvapi();

            tryAddNvmlToEnvPath();
            var nvmlInit = InitNvml();

            var numDevs = 0;

            foreach (var cudaDev in cudaDevs)
            {
                var skip = cudaDev.SM_major < 2 || cudaDev.SM_minor < 1;
                var skipOrAdd = skip ? "SKIPED" : "ADDED";
                var etherumCapableStr = cudaDev.IsEtherumCapable() ? "YES" : "NO";
                stringBuilder.AppendLine($"\t{skipOrAdd} device:");
                stringBuilder.AppendLine($"\t\tID: {cudaDev.DeviceID}");
                stringBuilder.AppendLine($"\t\tBusID: {cudaDev.pciBusID}");
                stringBuilder.AppendLine($"\t\tNAME: {cudaDev.GetName()}");
                stringBuilder.AppendLine($"\t\tVENDOR: {cudaDev.VendorName}");
                stringBuilder.AppendLine($"\t\tUUID: {cudaDev.UUID}");
                stringBuilder.AppendLine($"\t\tSM: {cudaDev.SM_major}.{cudaDev.SM_minor}");
                stringBuilder.AppendLine($"\t\tMEMORY: {cudaDev.DeviceGlobalMemory}");
                stringBuilder.AppendLine($"\t\tETHEREUM: {etherumCapableStr}");

                if (skip) continue;

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
                AvailableDevices.AddDevice(
                    new CudaComputeDevice(cudaDev, group, ++numDevs, handle, nvmlHandle)
                );
            }

            Helpers.ConsolePrint(Tag, stringBuilder.ToString());

            CudaDevices = cudaDevs;
            _isInit = true;
            
            Helpers.ConsolePrint(Tag, "QueryCudaDevices END");

            return numDevs;
        }

        private static Dictionary<int, NvPhysicalGpuHandle> InitNvapi()
        {
            var idHandles = new Dictionary<int, NvPhysicalGpuHandle>();
            if (!NVAPI.IsAvailable)
            {
                return idHandles;
            }
            
            var handles = new NvPhysicalGpuHandle[NVAPI.MAX_PHYSICAL_GPUS];
            if (NVAPI.NvAPI_EnumPhysicalGPUs == null)
            {
                Helpers.ConsolePrint("NVAPI", "NvAPI_EnumPhysicalGPUs unavailable");
            }
            else
            {
                var status = NVAPI.NvAPI_EnumPhysicalGPUs(handles, out _);
                if (status != NvStatus.OK)
                {
                    Helpers.ConsolePrint("NVAPI", "Enum physical GPUs failed with status: " + status);
                }
                else
                {
                    foreach (var handle in handles)
                    {
                        var idStatus = NVAPI.NvAPI_GPU_GetBusID(handle, out var id);

                        if (idStatus == NvStatus.EXPECTED_PHYSICAL_GPU_HANDLE) continue;

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
            
            return idHandles;
        }

        private static bool InitNvml()
        {
            try
            {
                var ret = NvmlNativeMethods.nvmlInit();
                if (ret != nvmlReturn.Success)
                    throw new Exception($"NVML init failed with code {ret}");
                return true;
            }
            catch (Exception e)
            {
                Helpers.ConsolePrint("NVML", e.ToString());
                return false;
            }
        }

        private static bool TryQueryCudaDevices(out List<CudaDevice> cudaDevices)
        {
            try
            {
                var queryCudaDevicesString = DeviceDetection.GetCUDADevices();
                var cudaQueryResult = JsonConvert.DeserializeObject<CudaDeviceDetectionResult>(queryCudaDevicesString,
                                Globals.JsonSettings);
                cudaDevices = cudaQueryResult.CudaDevices;

                if (cudaDevices != null && cudaDevices.Count != 0) return true;

                Helpers.ConsolePrint(Tag,
                    "CudaDevicesDetection found no devices. CudaDevicesDetection returned: " +
                    queryCudaDevicesString);

                return false;
            }
            catch (Exception ex)
            {
                // TODO
                Helpers.ConsolePrint(Tag, "CudaDevicesDetection threw Exception: " + ex.Message);
                cudaDevices = null;
                return false;
            }
        }

        private static void tryAddNvmlToEnvPath()
        {
            if (tryAddNvmlToEnvPathCalled) return;
            tryAddNvmlToEnvPathCalled = true; // call this ONLY ONCE AND NEVER AGAIN
            var nvmlRootPath = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles) +
                               "\\NVIDIA Corporation\\NVSMI";
            if (Directory.Exists(nvmlRootPath))
            {
                // Add to env so it can find nvml.dll
                var pathVar = Environment.GetEnvironmentVariable("PATH");
                pathVar += ";" + nvmlRootPath;
                Environment.SetEnvironmentVariable("PATH", pathVar);
            }
        }

        #endregion

        public static async Task<NvidiaSmiDriver> GetNvSmiDriverAsync()
        {
            var smiPath = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles) +
                          "\\NVIDIA Corporation\\NVSMI\\nvidia-smi.exe";
            if (smiPath.Contains(" (x86)")) smiPath = smiPath.Replace(" (x86)", "");
            try
            {
                using (var p = new Process
                {
                    StartInfo =
                    {
                        FileName = smiPath,
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        CreateNoWindow = true
                    }
                })
                {
                    p.Start();
                    p.WaitForExit(15 * 1000);

                    const string findString = "Driver Version: ";

                    string line;

                    do
                    {
                        line = await p.StandardOutput.ReadLineAsync();

                        if (line == null || !line.Contains(findString)) continue;

                        var start = line.IndexOf(findString);
                        var driverVer = line.Substring(start, start + 7);
                        driverVer = driverVer.Replace(findString, "").Substring(0, 7).Trim();
                        var leftPart = int.Parse(driverVer.Substring(0, 3));
                        var rightPart = int.Parse(driverVer.Substring(4, 2));
                        return new NvidiaSmiDriver(leftPart, rightPart);
                    } while (line != null);
                }
            }
            catch (Exception ex)
            {
                Helpers.ConsolePrint(Tag, "GetNvidiaSMIDriver Exception: " + ex.Message);
            }

            return new NvidiaSmiDriver(-1, -1);
        }

        public static void StartDeviceCheck()
        {
            if (!ConfigManager.GeneralConfig.RunScriptOnCUDA_GPU_Lost)
                return;

            _cudaCheckTimer = new Timer(60 * 1000);
            _cudaCheckTimer.Elapsed += CudaCheckTimerOnElapsed;
            _cudaCheckTimer.Start();
        }

        public static void StopDeviceCheck()
        {
            _cudaCheckTimer.Stop();
        }

        private static bool CheckDevicesMistmatch()
        {
            // this function checks if count of CUDA devices is same as it was on application start, reason for that is
            // because of some reason (especially when algo switching occure) CUDA devices are dissapiring from system
            // creating tons of problems e.g. miners stop mining, lower rig hashrate etc.

            var gpusOld = CudaDevices.Count;

            var querySuccess = TryQueryCudaDevices(out var currentDevs);
            
            var gpusNew = currentDevs.Count;

            Helpers.ConsolePrint("ComputeDeviceManager.CheckCount",
                "CUDA GPUs count: Old: " + gpusOld + " / New: " + gpusNew);

            return gpusNew < gpusOld || !querySuccess;
        }

        private static void CudaCheckTimerOnElapsed(object sender, ElapsedEventArgs e)
        {
            if (!CheckDevicesMistmatch()) return;

            try
            {
                var onGpusLost = new ProcessStartInfo(Directory.GetCurrentDirectory() + "\\OnGPUsLost.bat")
                {
                    WindowStyle = ProcessWindowStyle.Minimized
                };
                Process.Start(onGpusLost);
            }
            catch (Exception ex)
            {
                Helpers.ConsolePrint("NICEHASH", "OnGPUsMismatch.bat error: " + ex.Message);
            }
        }
    }

    // format 372.54;
    internal struct NvidiaSmiDriver : IComparable<NvidiaSmiDriver>
    {
        public int LeftPart { get; }

        private readonly int _rightPart;
        public int RightPart
        {
            get
            {
                if (_rightPart >= 10)
                {
                    return _rightPart;
                }

                return _rightPart * 10;
            }
        }

        public NvidiaSmiDriver(int left, int right)
        {
            LeftPart = left;
            _rightPart = right;
        }

        public override string ToString()
        {
            return $"{LeftPart}.{RightPart}";
        }

        #region IComparable implementation

        public int CompareTo(NvidiaSmiDriver other)
        {
            var leftPartComparison = LeftPart.CompareTo(other.LeftPart);
            if (leftPartComparison != 0) return leftPartComparison;
            return RightPart.CompareTo(other.RightPart);
        }

        public static bool operator <(NvidiaSmiDriver left, NvidiaSmiDriver right)
        {
            return left.CompareTo(right) < 0;
        }

        public static bool operator >(NvidiaSmiDriver left, NvidiaSmiDriver right)
        {
            return left.CompareTo(right) > 0;
        }

        public static bool operator <=(NvidiaSmiDriver left, NvidiaSmiDriver right)
        {
            return left.CompareTo(right) <= 0;
        }

        public static bool operator >=(NvidiaSmiDriver left, NvidiaSmiDriver right)
        {
            return left.CompareTo(right) >= 0;
        }

        #endregion
    }
}

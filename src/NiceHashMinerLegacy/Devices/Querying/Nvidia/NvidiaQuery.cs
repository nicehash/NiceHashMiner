using ManagedCuda.Nvml;
using NiceHashMinerLegacy.Common.Enums;
using NVIDIA.NVAPI;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace NiceHashMiner.Devices.Querying.Nvidia
{
    internal class NvidiaQuery : QueryGpu
    {
        private const string Tag = "NvidiaQuery";

        private static bool tryAddNvmlToEnvPathCalled = false;

        protected CudaQuery CudaQuery;

        public NvidiaQuery()
        {
            CudaQuery = new CudaQuery();
        }

        #region Query devices

        public List<CudaComputeDevice> QueryCudaDevices()
        {
            Helpers.ConsolePrint(Tag, "QueryCudaDevices START");

            var compDevs = new List<CudaComputeDevice>();

            if (!CudaQuery.TryQueryCudaDevices(out var cudaDevs))
            {
                Helpers.ConsolePrint(Tag, "QueryCudaDevices END");
                return compDevs;
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
                // We support SM3.0+
                var skip = cudaDev.SM_major < 3;
                var skipOrAdd = skip ? "SKIPED" : "ADDED";
                stringBuilder.AppendLine($"\t{skipOrAdd} device:");
                stringBuilder.AppendLine($"\t\tID: {cudaDev.DeviceID}");
                stringBuilder.AppendLine($"\t\tBusID: {cudaDev.pciBusID}");
                stringBuilder.AppendLine($"\t\tNAME: {cudaDev.GetName()}");
                stringBuilder.AppendLine($"\t\tVENDOR: {cudaDev.VendorName}");
                stringBuilder.AppendLine($"\t\tUUID: {cudaDev.UUID}");
                stringBuilder.AppendLine($"\t\tSM: {cudaDev.SM_major}.{cudaDev.SM_minor}");
                stringBuilder.AppendLine($"\t\tMEMORY: {cudaDev.DeviceGlobalMemory}");

                if (skip) continue;

                DeviceGroupType group;
                switch (cudaDev.SM_major)
                {
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
                compDevs.Add(new CudaComputeDevice(cudaDev, group, ++numDevs, handle, nvmlHandle));
            }

            Helpers.ConsolePrint(Tag, stringBuilder.ToString());
            
            Helpers.ConsolePrint(Tag, "QueryCudaDevices END");

            SortBusIDs(compDevs);

            return compDevs;
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
    }

}

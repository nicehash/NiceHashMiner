using ManagedCuda.Nvml;
using Newtonsoft.Json;
using NiceHashMiner.PInvoke;
using NiceHashMinerLegacy.Common.Enums;
using NVIDIA.NVAPI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace NiceHashMiner.Devices.Querying
{
    internal class NvidiaQuery
    {
        private const string Tag = "NvidiaQuery";

        public IReadOnlyList<CudaDevice> CudaDevices { get; private set; }

        private static bool tryAddNvmlToEnvPathCalled = false;
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

        public void QueryCudaDevices()
        {
            Helpers.ConsolePrint(Tag, "QueryCudaDevices START");

            if (!TryQueryCudaDevices(out var cudaDevs))
            {
                Helpers.ConsolePrint(Tag, "QueryCudaDevices END");
                return;
            }
            
            var stringBuilder = new StringBuilder();
            stringBuilder.AppendLine("");
            stringBuilder.AppendLine("CudaDevicesDetection:");

            // Enumerate NVAPI handles and map to busid
            var idHandles = InitNvapi();

            tryAddNvmlToEnvPath();
            var nvmlInit = InitNvml();

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
                AvailableDevices.Devices.Add(
                    new CudaComputeDevice(cudaDev, group, ++ComputeDeviceManager.Query.GpuCount, handle, nvmlHandle)
                );
            }

            Helpers.ConsolePrint(Tag, stringBuilder.ToString());

            CudaDevices = cudaDevs;
            
            Helpers.ConsolePrint(Tag, "QueryCudaDevices END");
        }

        private Dictionary<int, NvPhysicalGpuHandle> InitNvapi()
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

        public static bool TryQueryCudaDevices(out List<CudaDevice> cudaDevices)
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
    }
}

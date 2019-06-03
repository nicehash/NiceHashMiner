using ManagedCuda.Nvml;
using NiceHashMinerLegacy.Common;
using NiceHashMinerLegacy.Common.Enums;
using NVIDIA.NVAPI;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Management;
using System.Text;
using System.Threading.Tasks;

namespace NiceHashMiner.Devices.Querying.Nvidia
{
    internal class NvidiaQuery
    {
        private const string Tag = "NvidiaQuery";

        private static bool tryAddNvmlToEnvPathCalled = false;

        protected CudaQuery CudaQuery;

        public NvidiaQuery()
        {
            CudaQuery = new CudaQuery();
        }

        public List<CudaComputeDevice> QueryCudaDevices()
        {
            Logger.Info(Tag, "QueryCudaDevices START");

            var compDevs = new List<CudaComputeDevice>();

            if (!CudaQuery.TryQueryCudaDevices(out var cudaDevs))
            {
                Logger.Info(Tag, "QueryCudaDevices END");
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

                var nvmlHandle = new nvmlDevice();

                if (nvmlInit)
                {
                    var ret = NvmlNativeMethods.nvmlDeviceGetHandleByUUID(cudaDev.UUID, ref nvmlHandle);
                    stringBuilder.AppendLine(
                        "\t\tNVML HANDLE: " +
                        $"{(ret == nvmlReturn.Success ? nvmlHandle.Pointer.ToString() : $"Failed with code ret {ret}")}");
                }

                idHandles.TryGetValue(cudaDev.pciBusID, out var handle);
                compDevs.Add(new CudaComputeDevice(cudaDev, ++numDevs, handle, nvmlHandle));
            }

            Logger.Info(Tag, stringBuilder.ToString());
            Logger.Info(Tag, "QueryCudaDevices END");

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
                Logger.Debug("NVAPI", "NvAPI_EnumPhysicalGPUs unavailable");
            }
            else
            {
                var status = NVAPI.NvAPI_EnumPhysicalGPUs(handles, out _);
                if (status != NvStatus.OK)
                {
                    Logger.Debug("NVAPI", $"Enum physical GPUs failed with status: {status}");
                }
                else
                {
                    foreach (var handle in handles)
                    {
                        var idStatus = NVAPI.NvAPI_GPU_GetBusID(handle, out var id);

                        if (idStatus == NvStatus.EXPECTED_PHYSICAL_GPU_HANDLE) continue;

                        if (idStatus != NvStatus.OK)
                        {
                            Logger.Debug("NVAPI",
                                "Bus ID get failed with status: " + idStatus);
                        }
                        else
                        {
                            Logger.Debug("NVAPI", "Found handle for busid " + id);
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
                Logger.Error("NVML", e.Message);
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
    }

}

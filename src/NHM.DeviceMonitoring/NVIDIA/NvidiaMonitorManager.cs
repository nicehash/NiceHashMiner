using ManagedCuda.Nvml;
using NiceHashMinerLegacy.Common;
using NVIDIA.NVAPI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NHM.DeviceMonitoring.NVIDIA
{
    internal static class NvidiaMonitorManager
    {
        private const string Tag = "NvidiaMonitorManager";
        private static bool tryAddNvmlToEnvPathCalled = false;

        public static List<NvapiNvmlInfo> Init(Dictionary<string, int> nvidiaUUIDAndBusIds, bool useNvmlFallback)
        {
            // Enumerate NVAPI handles and map to busid
            var idHandles = InitNvapi();
            if (!useNvmlFallback)
            {
                Logger.Info(Tag, "tryAddNvmlToEnvPath");
                tryAddNvmlToEnvPath();
            }
            else
            {
                Logger.Info(Tag, "tryAddNvmlToEnvPathFallback");
                tryAddNvmlToEnvPathFallback();
            }
            var nvmlInit = InitNvml();
            var ret = new List<NvapiNvmlInfo>();
            foreach (var pair in nvidiaUUIDAndBusIds)
            {
                var uuid = pair.Key;
                var busID = pair.Value;

               var nvmlHandle = new nvmlDevice();
                if (nvmlInit)
                {
                    var nvmlRet = NvmlNativeMethods.nvmlDeviceGetHandleByUUID(uuid, ref nvmlHandle);
                    Logger.Info(Tag, "NVML HANDLE:" + $"{(nvmlRet == nvmlReturn.Success ? nvmlHandle.Pointer.ToString() : $"Failed with code ret {ret}")}");
                }
                idHandles.TryGetValue(busID, out var handle);
                var info = new NvapiNvmlInfo
                {
                    UUID = uuid,
                    BusID = busID,
                    nvHandle = handle,
                    nvmlHandle = nvmlHandle
                };
                ret.Add(info);
            }
            return ret;
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

        private static void tryAddNvmlToEnvPathFallback()
        {
            if (tryAddNvmlToEnvPathCalled) return;
            tryAddNvmlToEnvPathCalled = true; // call this ONLY ONCE AND NEVER AGAIN
            var nvmlRootPath = Paths.RootPath("NVIDIA");
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

using ManagedCuda.Nvml;
using NHM.Common;
using NVIDIA.NVAPI;
using System;
using System.Collections.Generic;
using System.IO;

namespace NHM.DeviceMonitoring.NVIDIA
{
    internal static class NvidiaMonitorManager
    {
        private const string Tag = "NvidiaMonitorManager";
        private static bool _tryAddNvmlToEnvPathCalled = false;

        internal static bool InitalNVMLInitSuccess = false;

        internal static Dictionary<string, int> _nvidiaUUIDAndBusIds;


        internal static void Init(Dictionary<string, int> nvidiaUUIDAndBusIds)
        {
            TryAddNvmlToEnvPath();
            _nvidiaUUIDAndBusIds = nvidiaUUIDAndBusIds;
            InitalNVMLInitSuccess = InitNvml();
            LogNvidiaMonitorManagerState();
        }

        internal static void LogNvidiaMonitorManagerState()
        {
            // Enumerate NVAPI handles and map to busid
            var idHandles = InitNvapi();
            foreach (var pair in _nvidiaUUIDAndBusIds)
            {
                var uuid = pair.Key;
                var busID = pair.Value;


                var nvmlResultStr = "InitalNVMLInitSuccess==FALSE";
                if (InitalNVMLInitSuccess)
                {
                    var nvmlHandle = new nvmlDevice();
                    var nvmlRet = NvmlNativeMethods.nvmlDeviceGetHandleByUUID(uuid, ref nvmlHandle);
                    if (nvmlRet != nvmlReturn.Success)
                    {
                        nvmlResultStr = $"Failed with code ret {nvmlRet}";
                    }
                    else
                    {
                        nvmlResultStr = nvmlHandle.Pointer.ToString();
                    }
                }
                var nvapiResultStr = "NVAPI found no handle";
                var nvHandle = new NvPhysicalGpuHandle();
                if (idHandles.TryGetValue(busID, out nvHandle))
                {
                    nvapiResultStr = nvHandle.ptr.ToString();
                }
                Logger.Info($"{Tag}.Init", $"UUID({uuid})-BusID({busID}): NVML_HANDLE({nvmlResultStr}) NVAPI_HANDLE({nvapiResultStr})");
            }
        }

        private static void TryAddNvmlToEnvPath()
        {
            if (_tryAddNvmlToEnvPathCalled) return;
            _tryAddNvmlToEnvPathCalled = true; // call this ONLY ONCE AND NEVER AGAIN

            // default path
            var nvmlRootPath = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles) +
                               "\\NVIDIA Corporation\\NVSMI";
            var nvmlRootPathTag = "DEFAULT";
            if (!File.Exists(Path.Combine(nvmlRootPath, "nvml.dll")))
            {
                nvmlRootPath = Environment.GetFolderPath(Environment.SpecialFolder.Windows) +
                               "\\System32";
                nvmlRootPathTag = "DHC";
            }

            Logger.Info(Tag, $"Adding NVML to PATH. {nvmlRootPathTag} path='{nvmlRootPath}'");
            if (Directory.Exists(nvmlRootPath))
            {
                // Add to env so it can find nvml.dll
                var pathVar = Environment.GetEnvironmentVariable("PATH");
                pathVar += ";" + nvmlRootPath;
                Environment.SetEnvironmentVariable("PATH", pathVar);
            }
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

        internal static bool InitNvml()
        {
            try
            {
                var ret = NvmlNativeMethods.nvmlInitWithFlags(0);
                if (ret != nvmlReturn.Success)
                    throw new Exception($"NVML init failed with code {ret}");
                return true;
            }
            catch (Exception e)
            {
                Logger.Error(Tag, e.Message);
                return false;
            }
        }

        internal static bool ShutdownNvml()
        {
            try
            {
                var ret = NvmlNativeMethods.nvmlShutdown();
                if (ret != nvmlReturn.Success)
                    throw new Exception($"NVML shutdown failed with code {ret}");
                return true;
            }
            catch (Exception e)
            {
                Logger.Error(Tag, e.Message);
                return false;
            }
        }

        #region NVML RESTART 
        private static object _restartLock = new object();
        internal static bool IsNVMLRestarting
        {
            get
            {
                using (var tryLock = new TryLock(_restartLock))
                {
                    return !tryLock.HasAcquiredLock;
                }
            }
        }

        internal static void AttemptRestartNVML()
        {
            using (var tryLock = new TryLock(_restartLock))
            {
                if (!tryLock.HasAcquiredLock) return;
                Logger.Info(Tag, $"Attempting to restart NVML");
                // restart
                var shutdownRet = ShutdownNvml();
                var initRet = InitNvml();
                LogNvidiaMonitorManagerState();
            }
        }
        #endregion NVML RESTART
    }
}

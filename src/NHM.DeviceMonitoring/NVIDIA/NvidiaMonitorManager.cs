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
            InitalNVMLInitSuccess = InitNvidiaLib();
        }

        private static void TryAddNvmlToEnvPath()
        {
            if (_tryAddNvmlToEnvPathCalled) return;
            _tryAddNvmlToEnvPathCalled = true; // call this ONLY ONCE AND NEVER AGAIN

            // default path
            var nvmlRootPath = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles) +
                               "\\NVIDIA Corporation\\NVSMI";
            var nvmlRootPathTag = "DEFAULT";
            if(!File.Exists(Path.Combine(nvmlRootPath, "nvml.dll")))
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

        internal static bool InitNvidiaLib()
        {
            try
            {
                NVIDIA_MON.nhm_nvidia_init();
                return true;
            }
            catch (Exception e)
            {
                Logger.Error(Tag, e.Message);
                return false;
            }
        }

        internal static bool DeInitNvidiaLib()
        {
            try
            {
                NVIDIA_MON.nhm_nvidia_deinit();
                return true;
            }
            catch (Exception e)
            {
                Logger.Error(Tag, e.Message);
                return false;
            }
        }
    }
}

using NHM.Common;
using System;
using System.Collections.Generic;
using System.IO;

namespace NHM.DeviceMonitoring.NVIDIA
{
    internal static class NvidiaMonitorManager
    {
        private const string Tag = "NvidiaMonitorManager";
        private static bool _tryAddNvmlToEnvPathCalled = false;

        internal static bool InitalNVIDIALibInitSuccess = false;

        internal static Dictionary<string, int> _nvidiaUUIDAndBusIds;

        internal static void Init(Dictionary<string, int> nvidiaUUIDAndBusIds, bool useNvmlFallback)
        {
            TryAddNvmlToEnvPath(useNvmlFallback);
            _nvidiaUUIDAndBusIds = nvidiaUUIDAndBusIds;
            InitalNVIDIALibInitSuccess = InitNvidiaLib();
        }


        private static void TryAddNvmlToEnvPath(bool useNvmlFallback)
        {
            if (_tryAddNvmlToEnvPathCalled) return;
            _tryAddNvmlToEnvPathCalled = true; // call this ONLY ONCE AND NEVER AGAIN

            // default path
            var nvmlRootPath = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles) +
                               "\\NVIDIA Corporation\\NVSMI";
            var nvmlRootPathTag = "DEFAULT";
            if (useNvmlFallback)
            {
                nvmlRootPath = Paths.AppRootPath("NVIDIA");
                nvmlRootPathTag = "FALLBACK";
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

        internal static bool ShutdownNvidiaLib()
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

        #region NVIDIA LIB RESTART 
        private static object _restartLock = new object();
        internal static bool IsNVIDIALibRestarting
        {
            get
            {
                using (var tryLock = new TryLock(_restartLock))
                {
                    return !tryLock.HasAcquiredLock;
                }
            }
        }

        internal static void AttemptRestartNVIDIALib()
        {
            using (var tryLock = new TryLock(_restartLock))
            {
                if (!tryLock.HasAcquiredLock) return;
                Logger.Info(Tag, $"Attempting to restart NVML");
                // restart
                var shutdownRet = ShutdownNvidiaLib();
                var initRet = InitNvidiaLib();
            }
        }
        #endregion NVIDIA LIB RESTART
    }
}

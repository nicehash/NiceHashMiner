using NHM.Common;
using NHM.Common.Device;
using NHM.DeviceMonitoring.AMD;
using NHM.DeviceMonitoring.NVIDIA;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace NHM.DeviceMonitoring
{
    public static class DeviceMonitorManager
    {
        public static bool DisableDeviceStatusMonitoring { get; set; } = false;
        public static bool DisableDevicePowerModeSettings { get; set; } = true;

        static DeviceMonitorManager()
        {
            try
            {
                string customSettingsFile = Paths.InternalsPath("AMD_ODN_LOG.txt");
                if (File.Exists(customSettingsFile))
                {
                    string read = File.ReadAllText(customSettingsFile);
                    _amdDebugLogLevel = int.Parse(read);
                }
            }
            catch (Exception e)
            {
                Logger.Error("DeviceMonitorManager", $"Constructor {e.Message}");
            }
        }


        private static int _amdDebugLogLevel = 0;
        private static readonly AMD_ODN.log_cb _amdLog = new AMD_ODN.log_cb(LogAMD_ODN);
        private static void LogAMD_ODN(string logStr)
        {
#warning AMD monitoring will attempt to run on each adapter hence the delay log fix this inside AMD monitoring library
            Logger.InfoDelayed("AMD_ODN", logStr, TimeSpan.FromSeconds(10));
        }

        public static Task<List<DeviceMonitor>> GetDeviceMonitors(IEnumerable<BaseDevice> devices)
        {
            return Task.Run(() =>
            {
                var ret = new List<DeviceMonitor>();

                var cpus = devices.Where(dev => dev is CPUDevice).Cast<CPUDevice>().ToList();
                var amds = devices.Where(dev => dev is AMDDevice).Cast<AMDDevice>().ToList();
                var nvidias = devices.Where(dev => dev is CUDADevice).Cast<CUDADevice>().ToList();

                foreach (var cpu in cpus)
                {
                    ret.Add(new DeviceMonitorCPU(cpu.UUID));
                }
                if (amds.Count > 0)
                {
                    AMD_ODN.nhm_amd_set_debug_log_level(_amdDebugLogLevel);
                    AMD_ODN.nhm_amd_reg_log_cb(_amdLog);
                    var amdInit = AMD_ODN.nhm_amd_init();
                    if (0 == amdInit)
                    {
                        foreach (var amd in amds)
                        {
                            var hasRet = AMD_ODN.nhm_amd_has_adapter(amd.PCIeBusID);
                            if (0 == hasRet)
                            {
                                ret.Add(new DeviceMonitorAMD(amd.UUID, amd.PCIeBusID));
                            }
                            else
                            {
                                Logger.Info("DeviceMonitorManager", $"AMD nhm_amd_has_adapter {hasRet} for BusID {amd.PCIeBusID}");
                            }
                        }
                    }
                    else
                    {
                        Logger.Info("DeviceMonitorManager", $"AMD nhm_amd_init {amdInit}");
                    }
                }
                if (nvidias.Count > 0)
                {
                    var initialNvmlRestartTimeWait = Math.Min(500 * nvidias.Count, 5000); // 500ms per GPU or initial MAX of 5seconds
                    var firstMaxTimeoutAfterNvmlRestart = TimeSpan.FromMilliseconds(initialNvmlRestartTimeWait);
                    var nvidiaUUIDAndBusIds = nvidias.ToDictionary(nvidia => nvidia.UUID, nvidia => nvidia.PCIeBusID);
                    NvidiaMonitorManager.Init(nvidiaUUIDAndBusIds);
                    foreach (var nvidia in nvidias)
                    {
                        var deviceMonitorNVIDIA = new DeviceMonitorNVIDIA(nvidia.UUID, nvidia.PCIeBusID, firstMaxTimeoutAfterNvmlRestart);
                        ret.Add(deviceMonitorNVIDIA);
                    }
                }

                return ret;
            });
        }
    }
}

using NHM.DeviceMonitoring.AMD;
using NHM.DeviceMonitoring.NVIDIA;
using NHM.Common.Device;
using NHM.Common;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System;

namespace NHM.DeviceMonitoring
{
    public static class DeviceMonitorManager
    {
        public static bool DisableDeviceStatusMonitoring { get; set; } = false;
        public static bool DisableDevicePowerModeSettings { get; set; } = false;
        public static Task<List<DeviceMonitor>> GetDeviceMonitors(IEnumerable<BaseDevice> devices, bool isDCHDriver)
        {
            return Task.Run(() => {
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
                    var amdBusIdAndUuids = amds.ToDictionary(amd => amd.PCIeBusID, amd => amd.UUID);
                    var (_, amdInfos) = QueryAdl.TryQuery(amdBusIdAndUuids);
                    foreach (var amdInfo in amdInfos)
                    {
                        ret.Add(new DeviceMonitorAMD(amdInfo));
                    }
                }
                if (nvidias.Count > 0)
                {
                    _isDCHDriver = isDCHDriver;
                    var nvidiaUUIDAndBusIds = nvidias.ToDictionary(nvidia => nvidia.UUID, nvidia => nvidia.PCIeBusID);
                    _nvidiaUUIDAndBusIds = nvidiaUUIDAndBusIds;
                    var nvidiaInfos = NvidiaMonitorManager.Init(nvidiaUUIDAndBusIds, isDCHDriver && UseNvmlFallback.Enabled);
                    foreach(var nvidiaInfo in nvidiaInfos)
                    {
                        var deviceMonitorNVIDIA = new DeviceMonitorNVIDIA(nvidiaInfo);
                        _deviceMonitorNVIDIAs.Add(deviceMonitorNVIDIA);
                        ret.Add(deviceMonitorNVIDIA);
                    }
                }

                return ret;
            });
        }

        #region NVIDIA
        private static object _RestartDeviceMonitorNVIDIALock = new object();
        private static bool _isDCHDriver;
        private static Dictionary<string, int> _nvidiaUUIDAndBusIds = new Dictionary<string, int>();
        private static List<DeviceMonitorNVIDIA> _deviceMonitorNVIDIAs = new List<DeviceMonitorNVIDIA>();
        private static void RestartNVIDIAMonitoring()
        {
            lock (DeviceMonitorNVIDIA._lock)
            {
                NvidiaMonitorManager.ShutdownNvml();
                var nvidiaInfos = NvidiaMonitorManager.Init(_nvidiaUUIDAndBusIds, _isDCHDriver && UseNvmlFallback.Enabled);
                foreach (var nvidiaInfo in nvidiaInfos)
                {
                    var deviceMonitorNVIDIA = _deviceMonitorNVIDIAs.Where(devMon => devMon.UUID == nvidiaInfo.UUID).FirstOrDefault();
                    if (deviceMonitorNVIDIA == null) continue;
                    deviceMonitorNVIDIA.ResetHandles(nvidiaInfo);
                }
            }
        }

        internal static void TryRestartNVIDIAMonitoring()
        {
            using (var tryLock = new TryLock(_RestartDeviceMonitorNVIDIALock))
            {
                if (tryLock.HasLock)
                {
                    Logger.Info("NHM.DeviceMonitoring.DeviceMonitorManager", "RestartNVIDIAMonitoring START");
                    int delay = Math.Min(500 * _deviceMonitorNVIDIAs.Count, 5000);
                    Logger.Info("NHM.DeviceMonitoring.DeviceMonitorManager", $"Waiting {delay}ms before NVIDIA device monitor re-init");
                    Thread.Sleep(delay);
                    RestartNVIDIAMonitoring();
                    Logger.Info("NHM.DeviceMonitoring.DeviceMonitorManager", "RestartNVIDIAMonitoring END");
                }
            }
        }

        #endregion NVIDIA
    }
}

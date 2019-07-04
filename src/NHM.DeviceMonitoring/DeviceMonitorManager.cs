using NHM.DeviceMonitoring.AMD;
using NHM.DeviceMonitoring.NVIDIA;
using NiceHashMinerLegacy.Common.Device;
using NiceHashMinerLegacy.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
                    var nvidiaUUIDAndBusIds = nvidias.ToDictionary(nvidia => nvidia.UUID, nvidia => nvidia.PCIeBusID);
                    var nvidiaInfos = NvidiaMonitorManager.Init(nvidiaUUIDAndBusIds, isDCHDriver && UseNvmlFallback.Enabled);
                    foreach(var nvidiaInfo in nvidiaInfos)
                    {
                        ret.Add(new DeviceMonitorNVIDIA(nvidiaInfo));
                    }
                }

                return ret;
            });
        }
    }
}

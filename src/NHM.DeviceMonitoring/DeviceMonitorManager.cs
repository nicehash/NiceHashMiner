using NHM.Common;
using NHM.Common.Device;
using NHM.DeviceMonitoring.AMD;
using NHM.DeviceMonitoring.NVIDIA;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NHM.DeviceMonitoring
{
    public static class DeviceMonitorManager
    {
        public static bool DisableDeviceStatusMonitoring { get; set; } = false;
        public static bool DisableDevicePowerModeSettings { get; set; } = true;
        public static Task<List<DeviceMonitor>> GetDeviceMonitors(IEnumerable<BaseDevice> devices)
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
                    if (0 == AMD_ODN.nhm_amd_init()) {
                        foreach (var amd in amds) {
                            if (0 == AMD_ODN.nhm_amd_has_adapter(amd.PCIeBusID)) {
                                ret.Add(new DeviceMonitorAMD(amd.UUID, amd.PCIeBusID));
                            }
                        }
                    }
                }
                if (nvidias.Count > 0)
                {
                    var initialNvmlRestartTimeWait = Math.Min(500 * nvidias.Count, 5000); // 500ms per GPU or initial MAX of 5seconds
                    var firstMaxTimeoutAfterNvmlRestart = TimeSpan.FromMilliseconds(initialNvmlRestartTimeWait);
                    var nvidiaUUIDAndBusIds = nvidias.ToDictionary(nvidia => nvidia.UUID, nvidia => nvidia.PCIeBusID);
                    NvidiaMonitorManager.Init(nvidiaUUIDAndBusIds);
                    foreach(var nvidia in nvidias)
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

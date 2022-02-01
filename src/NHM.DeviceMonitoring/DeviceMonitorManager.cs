using LibreHardwareMonitor.Hardware;
using NHM.Common;
using NHM.Common.Device;
using NHM.DeviceMonitoring.AMD;
using NHM.DeviceMonitoring.NVIDIA;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Principal;
using System.Threading.Tasks;

namespace NHM.DeviceMonitoring
{
    public static class DeviceMonitorManager
    {
        public static bool DisableDeviceStatusMonitoring { get; set; } = false;
        public static bool DisableDevicePowerModeSettings { get; set; } = true;

        internal static readonly bool IsElevated;

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

            try
            {
                using (var identity = WindowsIdentity.GetCurrent())
                {
                    var principal = new WindowsPrincipal(identity);
                    IsElevated = principal.IsInRole(WindowsBuiltInRole.Administrator);
                }
            }
            catch (Exception e)
            {
                Logger.Error("DeviceMonitorManager", $"Constructor IsElevated {e.Message}");
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
                    var nvidiaUUIDAndBusIds = nvidias.ToDictionary(nvidia => nvidia.UUID, nvidia => nvidia.PCIeBusID);
                    var nvidiaInit = NVIDIA_MON.nhm_nvidia_init();
                    DeviceMonitorNVIDIA.Init();
                    if (nvidiaInit == 0)
                    {
                        foreach (var nvidia in nvidias)
                        {
                            var deviceMonitorNVIDIA = new DeviceMonitorNVIDIA(nvidia.UUID, nvidia.PCIeBusID);
                            ret.Add(deviceMonitorNVIDIA);
                        }
                    }
                    else
                    {
                        Logger.Info("DeviceMonitorManager", $"AMD nhm_amd_init {nvidiaInit}");
                    }
                }
                return ret;
            });
        }

        public static bool IsMotherboardCompatible()
        {
            var isCompatible = true;
            try
            {
                var computer = LibreHardwareMonitorManager.Instance.Computer;
                var updateVisitor = new UpdateVisitor();
                computer.Accept(updateVisitor);
                var cpu = computer.Hardware.FirstOrDefault(hw => hw.HardwareType == HardwareType.Cpu);
                var mainboard = computer.Hardware.FirstOrDefault(hw => hw.HardwareType == HardwareType.Motherboard);
                var cpuSensors = cpu.Sensors.Where(s => s.SensorType == SensorType.Temperature);
                var cpuSensor = cpuSensors.FirstOrDefault(s => s.Name == "CPU Package" || s.Name.Contains("(Tdie)"));
                if (cpuSensor == null) cpuSensor = cpuSensors.FirstOrDefault(s => s.Name.Contains("(Tctl/Tdie)"));
                if (cpuSensor == null) cpuSensor = cpuSensors.FirstOrDefault();
                foreach (var subHW in mainboard.SubHardware)
                {
                    var groupedSensors = subHW.Sensors
                        .Where(s => (s.SensorType == SensorType.Fan || s.SensorType == SensorType.Control) && s.Value != 0).OrderBy(s => s.Name)
                        .Select(s => new { id = s.Identifier.ToString().Replace("fan", "*").Replace("control", "*"), s })
                        .GroupBy(p => p.id)
                        .Select(g => g.ToArray().Select(p => p.s).OrderBy(s => s.SensorType))
                        .ToArray();

                    ISensor sensor = null;
                    if (groupedSensors.Any(sg => sg.Count() == 2)) sensor = groupedSensors.FirstOrDefault(sg => sg.Count() == 2).FirstOrDefault(s => s.SensorType == SensorType.Control);
                    if (!sensor.Value.HasValue || sensor == null) isCompatible = false;
                }
                if (cpuSensor == null || !cpuSensor.Value.HasValue) isCompatible =  false;
            }
            catch(Exception e)
            {
                Logger.Error("DeviceMonitorManager", "Error when getting CPU fan speed and temperature: " + e.Message);
            }

            return isCompatible;
        }

        public static void CloseComputer()
        {
            LibreHardwareMonitorManager.Instance.Computer.Close();
        }

        public static void OpenComputer()
        {
            LibreHardwareMonitorManager.Instance.Computer.Open();
        }
    }
}

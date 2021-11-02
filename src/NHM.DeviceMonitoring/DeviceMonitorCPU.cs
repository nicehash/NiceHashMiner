using LibreHardwareMonitor.Hardware;
using NHM.Common;
using System;
using System.Diagnostics;
using System.Linq;

namespace NHM.DeviceMonitoring
{
    internal class DeviceMonitorCPU : DeviceMonitor, ILoad, ITemp, IGetFanSpeedPercentage
    {
        private PerformanceCounter _cpuCounter { get; set; }

        private static readonly TimeSpan _delayedLogging = TimeSpan.FromMinutes(5);

        internal DeviceMonitorCPU(string uuid)
        {
            UUID = uuid;
            _cpuCounter = new PerformanceCounter
            {
                CategoryName = "Processor",
                CounterName = "% Processor Time",
                InstanceName = "_Total"
            };
        }

        public float Load
        {
            get
            {
                try
                {
                    if (_cpuCounter != null) return _cpuCounter.NextValue();
                }
                catch (Exception e)
                {
                    Logger.ErrorDelayed("CPUDIAG", e.ToString(), _delayedLogging);
                }
                return -1;
            }
        }

        public float Temp
        {
            get
            {
                if (!DeviceMonitorManager.IsElevated) return -1;
                var temperature = -1;
                try
                {
                    var computer = LibreHardwareMonitorManager.Instance.Computer;
                    var updateVisitor = new UpdateVisitor();
                    computer.Accept(updateVisitor);
                    var cpu = computer.Hardware.FirstOrDefault(hw => hw.HardwareType == HardwareType.Cpu);
                    var cpuSensors = cpu.Sensors.Where(s => s.SensorType == SensorType.Temperature);
                    var cpuSensor = cpuSensors.FirstOrDefault(s => s.Name == "CPU Package" || s.Name.Contains("(Tdie)"));
                    if (cpuSensor == null) cpuSensor = cpuSensors.FirstOrDefault(s => s.Name.Contains("(Tctl/Tdie)"));
                    if (cpuSensor == null) cpuSensor = cpuSensors.FirstOrDefault();
                    if (cpuSensor != null) temperature = Convert.ToInt32(cpuSensor.Value);
                }
                catch(Exception e)
                {
                    Logger.ErrorDelayed("DeviceMonitorCPU", "Error when getting CPU temperature: " + e.Message, _delayedLogging);
                }

                return temperature;
            }
        }

        (int status, int percentage) IGetFanSpeedPercentage.GetFanSpeedPercentage()
        {
            if (!DeviceMonitorManager.IsElevated) return (-1, -1);
            var percentage = -1;
            var ok = 0;
            try
            {
                var computer = LibreHardwareMonitorManager.Instance.Computer;
                var updateVisitor = new UpdateVisitor();
                computer.Accept(updateVisitor);
                var mainboard = computer.Hardware.FirstOrDefault(hw => hw.HardwareType == HardwareType.Motherboard);
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

                    if (sensor != null) percentage = Convert.ToInt32(sensor.Value);
                }
            }
            catch(Exception e)
            {
                Logger.ErrorDelayed("DeviceMonitorCPU", "Error when getting CPU fan speed: " + e.Message, _delayedLogging);
                ok = -1;
            }

            return (ok, percentage);
        }
    }
}

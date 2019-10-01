using NHM.Common;
using System;
using System.Diagnostics;

namespace NHM.DeviceMonitoring
{
    internal class DeviceMonitorCPU : DeviceMonitor, ILoad
    {
        private PerformanceCounter _cpuCounter { get; set; }
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
                    Logger.ErrorDelayed("CPUDIAG", e.ToString(), TimeSpan.FromMinutes(5));
                }
                return -1;
            }
        }
    }
}

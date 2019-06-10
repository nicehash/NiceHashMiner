using NiceHashMinerLegacy.Common;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
                    // TODO add delayed logger
                    Logger.Error("CPUDIAG", e.ToString());
                }
                return -1;
            }
        }
    }
}

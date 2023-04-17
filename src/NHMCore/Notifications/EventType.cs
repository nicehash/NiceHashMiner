using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NHMCore.Notifications
{
    public enum EventType
    {
        Unknown = 0,
        RigStarted = 1,
        RigStopped = 2,
        DeviceEnabled = 3,
        DeviceDisabled = 4,
        RigRestart = 5,
        Unknown1 = 6,
        PluginFailiure = 7,
        MissingFiles = 8,
        VirtualMemory = 9,
        GeneralConfigErr = 10,
        Unknown2 = 11,
        DriverCrash = 12,
        BenchmarkStarted = 1000,
        AlgoSwitch = 1001,
        TestOverClockApplied = 1002,
        TestOverClockFailed = 1003,
        MissingDev = 1004,
        BundleApplied = 1005,
        BenchmarkFailed = 1006,
        AlgoEnabled = 1007,
        AlgoDisabled = 1008,
    }
}

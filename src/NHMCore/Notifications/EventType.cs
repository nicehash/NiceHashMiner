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
        DeviceOverheat = 13,
        MissingDevice = 14,
        AutoSwitch = 15,
        AlgoEnabled = 16,
        AlgoDisabled = 17,
        TestOCApplied = 18,
        TestOCFailed = 19,
        BundleApplied = 20,
        Unknown3 = 21,
        BenchmarkFailed = 22,
    }
}

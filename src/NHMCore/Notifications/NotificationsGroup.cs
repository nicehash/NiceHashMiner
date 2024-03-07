﻿using System;

namespace NHMCore.Notifications
{
    public enum NotificationsGroup
    {
        Misc,
        Market,
        Profit,
        MonitoringNvidiaElevate,
        ConnectionLost,
        NoEnabledDevice,
        DemoMining,
        NoSma,
        [Obsolete]
        NoDeviceSelectedBenchmark,
        [Obsolete]
        NothingToBenchmark,
        FailedBenchmarks,
        NoSupportedDevices,
        MissingMiners,
        MissingMinerBins,
        [Obsolete]
        FailedVideoController,
        [Obsolete]
        WmiEnabled,
        [Obsolete]
        Net45,
        [Obsolete]
        BitOS64,
        NhmUpdate,
        NhmUpdateFailed,
        NhmWasUpdated,
        PluginUpdate,
        WindowsDefenderException,
        ComputeModeAMD,
        LargePages,
        VirtualMemory,
        NoAvailableAlgorithms,
        LogArchiveUpload,
        MissingGPUs,
        NVMLLoadInitFail,
        WrongChecksumBinary,
        WrongChecksumDll,
        MinerRestart,
        NullChecksum,
        [Obsolete]
        GamingStarted,
        [Obsolete]
        GamingFinished,
        AdminRunRequired,
        MotherboardNotCompatible,
        DriverVersionProblem,
        OptimizationWithProfilesDisabled,
        OptimizationProfilesElevate,
        RequireAdminForLHR,
        NoPowerInfo,
        NoOptimalDrivers,
        RigManagementElevate,
        OverclockingIsOff
    }
}

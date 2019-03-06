using System;
using System.Collections.Generic;
using System.Linq;
using NiceHashMiner.Algorithms;
using NiceHashMiner.Devices;
using NiceHashMiner.Miners.Grouping;
using NiceHashMinerLegacy.Common.Enums;

namespace NiceHashMiner.Miners
{
    // Phoenix has an almost identical interface to CD, so reuse all that code
    public class Phoenix : ClaymoreDual
    {
        public Phoenix() : base(AlgorithmType.NONE)
        {
            LookForStart = "main eth speed: ";
            DevFee = 0.65;
        }

        protected override void _Stop(MinerStopType willSwitch)
        {
            ShutdownMiner(true);
        }

        protected override string BenchmarkCreateCommandLine(Algorithm algorithm, int time)
        {
            var cl = base.BenchmarkCreateCommandLine(algorithm, time);
            BenchmarkTimeWait = Math.Max(time, 60);
            return cl;
        }

        protected override IEnumerable<MiningPair> SortDeviceList(IEnumerable<MiningPair> startingList)
        {
            // first by dev type (NV first), then by bus ID
            return startingList
                .OrderBy(pair => pair.Device.DeviceType)
                .ThenBy(pair => pair.Device.IDByBus);
        }

        protected override int GetIDOffsetForType(DeviceType type)
        {
            return type == DeviceType.AMD ? AvailableDevices.NumDetectedNvDevs : 0;
        }

        protected override string GetBenchmarkOption()
        {
            // Phoenix sets a bad DAG epoch when -benchmark switch is used
            return "";
        }
    }
}

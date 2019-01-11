using System;
using NiceHashMiner.Algorithms;
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
    }
}

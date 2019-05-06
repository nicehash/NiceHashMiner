using NiceHashMiner.Algorithms;
using NiceHashMiner.Configs;
using NiceHashMiner.Devices;
using NiceHashMiner.Miners.Grouping;
using NiceHashMiner.Miners.IntegratedPlugins;
using NiceHashMinerLegacy.Common.Enums;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;

using AlgorithmCommon = NiceHashMinerLegacy.Common.Algorithm;

namespace NiceHashMiner.Miners
{
    public static class EthlargementOld
    {
        public static void Stop()
        {
            EthlargementIntegratedPlugin.Instance.Stop();
        }
    }
}

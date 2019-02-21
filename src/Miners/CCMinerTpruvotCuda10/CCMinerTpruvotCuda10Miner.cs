using System;
using CCMinerBase;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using MinerPlugin;
using MinerPlugin.Toolkit;
using NiceHashMinerLegacy.Common.Enums;
using static NiceHashMinerLegacy.Common.StratumServiceHelpers;
using static MinerPlugin.Toolkit.MinersApiPortsManager;
using System.Globalization;
using System.IO;
using NiceHashMinerLegacy.Common;

namespace CCMinerTpruvotCuda10
{
    public class CCMinerTpruvotCuda10Miner : CCMinerBase.CCMinerBase
    {
        protected override string AlgorithmName(AlgorithmType algorithmType)
        {
            switch (algorithmType)
            {
                case AlgorithmType.NeoScrypt: return "neoscrypt";
                case AlgorithmType.Lyra2REv2: return "lyra2v2";
                case AlgorithmType.Decred: return "decred";
                case AlgorithmType.Lbry: return "lbry";
                case AlgorithmType.X11Gost: return "sib";
                case AlgorithmType.Blake2s: return "blake2s";
                case AlgorithmType.Sia: return "sia";
                case AlgorithmType.Keccak: return "keccak";
                case AlgorithmType.Skunk: return "skunk";
                case AlgorithmType.Lyra2z: return "lyra2z";
                case AlgorithmType.X16R: return "x16r";
                case AlgorithmType.Lyra2REv3: return "lyra2v3";
            }
            // TODO throw exception
            return "";
        }

        protected override (string, string) GetBinAndCwdPaths()
        {
            var pluginRoot = Path.Combine(Paths.MinerPlugins, Shared.UUID);
            var pluginRootBins = Path.Combine(pluginRoot, "bins");
            var binPath = Path.Combine(pluginRootBins, "ccminer.exe");
            var binCwd = pluginRootBins;
            return (binPath, binCwd);
        }
    }
}

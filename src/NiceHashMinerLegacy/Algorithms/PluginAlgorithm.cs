using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NiceHashMinerLegacy.Common.Enums;
using AlgorithmCommon = NiceHashMinerLegacy.Common.Algorithm;

namespace NiceHashMiner.Algorithms
{

    public class PluginAlgorithm : Algorithm
    {
        public AlgorithmCommon.Algorithm BaseAlgo;

        public PluginAlgorithm(AlgorithmCommon.Algorithm algorithm) : base(MinerBaseType.PLUGIN, algorithm.FirstAlgorithmType, "", algorithm.Enabled)
        {
            BaseAlgo = algorithm;
        }

    }
}

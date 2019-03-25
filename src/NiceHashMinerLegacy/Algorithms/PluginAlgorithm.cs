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

        public readonly string PluginName;

        public PluginAlgorithm(string pluginName, AlgorithmCommon.Algorithm algorithm) : base(MinerBaseType.PLUGIN, algorithm.FirstAlgorithmType, "", algorithm.Enabled)
        {
            PluginName = pluginName;
            BaseAlgo = algorithm;
        }

        public override string ExtraLaunchParameters {
            get
            {
                if (BaseAlgo == null) return ""; 
                return BaseAlgo.ExtraLaunchParameters;
            }
            set
            {
                if (BaseAlgo != null) BaseAlgo.ExtraLaunchParameters = value;
            }
        }

        public override bool Enabled
        {
            get
            {
                if (BaseAlgo == null) return false;
                return BaseAlgo.Enabled;
            }
            set
            {
                if (BaseAlgo != null) BaseAlgo.Enabled = value;
            }
        }
    }
}

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

        public Version ConfigVersion = new Version(1, 0);
        public Version PluginVersion { get; private set; } = new Version(1, 0);

        public PluginAlgorithm(string pluginName, AlgorithmCommon.Algorithm algorithm, Version pluginVersion) : base(MinerBaseType.PLUGIN, algorithm.FirstAlgorithmType, "", algorithm.Enabled)
        {
            PluginName = pluginName;
            BaseAlgo = algorithm;
            PluginVersion = pluginVersion;
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

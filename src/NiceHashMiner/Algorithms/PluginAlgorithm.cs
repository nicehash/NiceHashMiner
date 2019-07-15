using System;
using System.Collections.Generic;
using System.Linq;
using NiceHashMiner.Mining.Plugins;
using NHM.Common.Enums;
using AlgorithmCommon = NHM.Common.Algorithm;

namespace NiceHashMiner.Algorithms
{

    public class PluginAlgorithm : Algorithm
    {
        public AlgorithmCommon.Algorithm BaseAlgo { get; private set; }

        public string PluginName { get; private set; }

        public Version ConfigVersion { get; set; } = new Version(1, 0);
        public Version PluginVersion { get; private set; } = new Version(1, 0);

        public PluginAlgorithm(string pluginName, AlgorithmCommon.Algorithm algorithm, Version pluginVersion)
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

        public override string MinerBaseTypeName
        {
            get
            {
                if (BaseAlgo == null) return "";
                var plugin = MinerPluginsManager.GetPluginWithUuid(BaseAlgo.MinerID);
                if (plugin == null) return "";
                var isIntegrated = plugin.IsIntegrated;
                var minerName = plugin.Name + (isIntegrated ? "" : "(PLUGIN)");
                return minerName;
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

        public override bool IsDual => BaseAlgo.IDs.Count > 1;


        public override string MinerUUID => BaseAlgo?.MinerID;

        public override string AlgorithmName => BaseAlgo?.AlgorithmName ?? "";

        public override string AlgorithmStringID => BaseAlgo?.AlgorithmStringID ?? "";

        public override AlgorithmType[] IDs => BaseAlgo.IDs.ToArray();

        public override double BenchmarkSpeed
        {
            get
            {
                return BaseAlgo.Speeds[0];
            }
            set
            {
                BaseAlgo.Speeds[0] = value;
            }
        }
        public override double SecondaryBenchmarkSpeed
        {
            get
            {
                if (IsDual) return BaseAlgo.Speeds[1];
                return 0d;
            }
            set
            {
                if (IsDual) BaseAlgo.Speeds[1] = value;
            }
        }
        public override List<double> Speeds
        {
            get
            {
                return BaseAlgo.Speeds.ToList();
            }
            set
            {
                for (var i = 0; i < BaseAlgo.Speeds.Count && i < value.Count; i++)
                {
                    BaseAlgo.Speeds[i] = value[i];
                }
            }
        }
    }
}

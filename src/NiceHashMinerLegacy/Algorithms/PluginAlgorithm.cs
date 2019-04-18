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

        public override AlgorithmType AlgorithmUUID
        {
            get
            {
                if (IsDual)
                {
                    return Helpers.DualAlgoFromAlgos(BaseAlgo.FirstAlgorithmType, BaseAlgo.SecondAlgorithmType);
                }
                return BaseAlgo.FirstAlgorithmType;
            }
        }


        public override string MinerUUID => BaseAlgo?.MinerID;

        public override string AlgorithmName => AlgorithmUUID.ToString();

        public override string AlgorithmStringID => $"{AlgorithmName}_{MinerUUID}";

        public override AlgorithmType[] IDs => BaseAlgo.IDs.ToArray();

        public override double BenchmarkSpeed
        {
            get
            {
                return Speeds[0];
            }
            set
            {
                Speeds[0] = value;
            }
        }
        public override double SecondaryBenchmarkSpeed
        {
            get
            {
                if (IsDual) return Speeds[1];
                return 0d;
            }
            set
            {
                if (IsDual) Speeds[1] = value;
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

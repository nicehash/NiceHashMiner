using NiceHashMinerLegacy.Common.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NiceHashMiner.Configs.Data
{
    [Serializable]
    public class PluginAlgorithmConfig
    {
        public string Name = ""; // Used as an indicator for easier user interaction
        public string PluginUUID;
        public string PluginVersion;
        public List<AlgorithmType> AlgorithmIDs;
        public List<double> Speeds;
        public string ExtraLaunchParameters = "";
        public bool Enabled = true;
        public double PowerUsage = 0;
    }
}

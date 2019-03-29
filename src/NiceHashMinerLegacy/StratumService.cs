using NiceHashMiner.Configs;
using NiceHashMinerLegacy.Common.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NiceHashMiner
{
    public static class StratumService
    {

        // TODO consider using this instead of int index
        //// EU by default
        public static string SelectedService {
            get {
                if (ConfigManager.GeneralConfig.ServiceLocation > MiningLocations.Count || ConfigManager.GeneralConfig.ServiceLocation < 0)
                {
                    return "eu";
                }
                return MiningLocations[ConfigManager.GeneralConfig.ServiceLocation];
            }
        }

        // Constants
        public static IReadOnlyList<string> MiningLocations { get; } = 
            new [] { "eu", "usa", "hk", "jp", "in", "br" };

        public static readonly object[] MiningLocationNames = new object[] {
            "Europe - Amsterdam",
            "USA - San Jose",
            "China - Hong Kong",
            "Japan - Tokyo",
            "India - Chennai",
            "Brazil - Sao Paulo"
        };

        private static string GetAlgorithmUrlName(AlgorithmType algorithmType)
        {
            if (algorithmType < 0)
            {
                return "";
            }
            var name = AlgorithmNiceHashNames.GetName(algorithmType);
            if (name == AlgorithmNiceHashNames.NOT_FOUND)
            {
                return "";
            }
            // strip out the _UNUSED
            name = name.Replace("_UNUSED", "");
            return name.ToLower();
        }

        // make this private when/if SelectedService gets implemented
        public static string GetLocationUrl(AlgorithmType algorithmType, string miningLocation, NhmConectionType conectionType)
        {
            var name = GetAlgorithmUrlName(algorithmType);
            // if name is empty return
            if (name == "") return "";

            var nPort = 3333 + algorithmType;
            var sslPort = 30000 + nPort;

            // NHMConectionType.NONE
            var prefix = "";
            var port = nPort;
            switch (conectionType)
            {
                case NhmConectionType.LOCKED:
                    return miningLocation;
                case NhmConectionType.STRATUM_TCP:
                    prefix = "stratum+tcp://";
                    break;
                case NhmConectionType.STRATUM_SSL:
                    prefix = "stratum+ssl://";
                    port = sslPort;
                    break;
            }

#if TESTNET
            return prefix
                   + name
                   + "-test." + miningLocation
                   + ".nicehash.com:"
                   + port;
#elif TESTNETDEV
            return prefix
                   + "stratum-test." + miningLocation
                   + ".nicehash.com:"
                   + port;
#else
            return prefix
                   + name
                   + "." + miningLocation
                   + ".nicehash.com:"
                   + port;
#endif
        }
    }
}

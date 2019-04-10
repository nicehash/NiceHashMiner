using System;
using Newtonsoft.Json;
using System.Collections.Generic;
using NiceHashMiner.Configs;
using NiceHashMiner.Switching;
using NiceHashMiner.Utils.Guid;
using NiceHashMinerLegacy.Common.Enums;
using NiceHashMinerLegacy.Extensions;

namespace NiceHashMiner
{
    public static class Globals
    {
#if TESTNET
        public static readonly string DemoUser = "2N6ibfrTwUSSvzAz1esPe1gYULG82asTHiS";
#elif TESTNETDEV
        public static readonly string DemoUser = "2N2e2ET1jMY9r5is9KaTKnU3bkCFaYHEEEx"; // TODO
#else
        public static readonly string DemoUser = "33hGFJZQAfbdzyHGqhJPvZwncDjUBdZqjW";
#endif

        // Constants
        public static string[] MiningLocation = { "eu", "usa", "hk", "jp", "in", "br" };

        // change this if TOS changes
        public const int CurrentTosVer = 4;

        // Variables
        public static JsonSerializerSettings JsonSettings = null;

        public static readonly string RigID;

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

        public static string GetBitcoinUser()
        {
            return BitcoinAddress.ValidateBitcoinAddress(Configs.ConfigManager.GeneralConfig.BitcoinAddress.Trim())
                ? Configs.ConfigManager.GeneralConfig.BitcoinAddress.Trim()
                : DemoUser;
        }
    }
}

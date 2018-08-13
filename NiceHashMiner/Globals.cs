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
        // Constants
        public static string[] MiningLocation = {"eu", "usa", "hk", "jp", "in", "br"};

        public static readonly string DemoUser = "33hGFJZQAfbdzyHGqhJPvZwncDjUBdZqjW";

        // change this if TOS changes
        public const int CurrentTosVer = 4;

        // Variables
        public static JsonSerializerSettings JsonSettings = null;

        public static int ThreadsPerCpu;

        // quickfix guard for checking internet conection
        public static bool IsFirstNetworkCheckTimeout = true;

        public static int FirstNetworkCheckTimeoutTimeMs = 500;
        public static int FirstNetworkCheckTimeoutTries = 10;

        public static readonly string RigID;

        static Globals()
        {
            var guid = Helpers.GetMachineGuid();
            if (guid == null)
            {
                // TODO
                RigID = $"{0}-{Guid.NewGuid()}";
                return;
            }

            var uuid = UUID.V5(UUID.Nil().AsGuid(), $"NHML{guid}");
            RigID = $"{0}-{uuid.AsGuid().ToByteArray().ToBase64String()}";
        }

        public static string GetLocationUrl(AlgorithmType algorithmType, string miningLocation, NhmConectionType conectionType)
        {
            if (!NHSmaData.TryGetSma(algorithmType, out var sma)) return "";

            var name = sma.Name;
            var nPort = sma.Port;
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

            return prefix
                   //+ name
                   + "stratum-test" //+ miningLocation
                   + ".nicehash.com:"
                   + port;
        }

        public static string GetBitcoinUser()
        {
            return BitcoinAddress.ValidateBitcoinAddress(ConfigManager.GeneralConfig.BitcoinAddress.Trim())
                ? ConfigManager.GeneralConfig.BitcoinAddress.Trim()
                : DemoUser;
        }

        public static string GetWorkerName()
        {
            var workername = BitcoinAddress.ValidateWorkerName(ConfigManager.GeneralConfig.WorkerName.Trim())
                ? ConfigManager.GeneralConfig.WorkerName.Trim()
                : "";
            return $"{workername}:{RigID}";
        }
    }
}

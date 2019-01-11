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

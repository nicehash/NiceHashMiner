using Newtonsoft.Json;
using System.Collections.Generic;
using NiceHashMiner.Switching;
using NiceHashMinerLegacy.Common.Enums;

namespace NiceHashMiner
{
    public class Globals
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

        public static string GetBitcoinUser()
        {
            return BitcoinAddress.ValidateBitcoinAddress(Configs.ConfigManager.GeneralConfig.BitcoinAddress.Trim())
                ? Configs.ConfigManager.GeneralConfig.BitcoinAddress.Trim()
                : DemoUser;
        }
    }
}

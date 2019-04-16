using System;
using Newtonsoft.Json;
using System.Collections.Generic;
using NiceHashMiner.Configs;
using NiceHashMiner.Switching;
using NiceHashMiner.Utils.Guid;
using NiceHashMinerLegacy.Common;
using NiceHashMinerLegacy.Common.Enums;
using NiceHashMinerLegacy.Extensions;

namespace NiceHashMiner
{
    public static partial class Globals
    {
        public static string GetBitcoinUser()
        {
            return BitcoinAddress.ValidateBitcoinAddress(Configs.ConfigManager.GeneralConfig.BitcoinAddress.Trim())
                ? Configs.ConfigManager.GeneralConfig.BitcoinAddress.Trim()
                : DemoUser.BTC;
        }
    }
}

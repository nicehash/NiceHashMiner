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
    public static partial class Globals
    {
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

        public static string GetUsername()
        {
            var btc = ConfigManager.GeneralConfig.BitcoinAddress?.Trim();
            var worker = ConfigManager.GeneralConfig.WorkerName?.Trim();
            if (worker.Length > 0 && BitcoinAddress.ValidateWorkerName(worker))
            {
                return $"{btc}.{worker}${RigID}";
            }

            return $"{btc}${RigID}";
        }
    }
}

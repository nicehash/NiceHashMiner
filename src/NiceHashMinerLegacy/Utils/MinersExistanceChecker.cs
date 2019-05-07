using NiceHashMiner.Plugin;
using NiceHashMinerLegacy.Common;
using System.IO;
using System.Linq;

namespace NiceHashMiner.Utils
{
    public static class MinersExistanceChecker
    {
        public static bool IsMinersBinsInit()
        {
            // OLD remove when we have all miners with Integrated plugins 
            string[] ALL_FILES_BINS =
            {
                @"vc_redist.x64.exe",
            };
            foreach (var filePath in ALL_FILES_BINS)
            {
                var checkPath = Path.Combine("miner_plugins", filePath);
                if (!File.Exists(checkPath))
                {
                    Logger.Info("MinersExistanceChecker", $"Bin '{checkPath}' doesn't exist! Warning");
                    return false;
                }
            }
            // Integrated plugins
            var missingFiles = MinerPluginsManager.GetMissingMiners(false);
            if (missingFiles.Count > 0)
            {
                Logger.Info("MinersExistanceChecker", $"Open Source Plugin file: '{missingFiles.First()}' doesn't exist! Warning");
                return false;
            }
            return true;
        }

        public static bool IsMiners3rdPartyBinsInit()
        {
            var missingFiles = MinerPluginsManager.GetMissingMiners(true);
            if (missingFiles.Count > 0)
            {
                Logger.Info("MinersExistanceChecker", $"3rdparty Plugin file: '{missingFiles.First()}' doesn't exist! Warning");
                return false;
            }
            return true;
        }
    }
}

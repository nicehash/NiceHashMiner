using System.IO;

namespace NiceHashMiner.Utils
{
    public static class MinersExistanceChecker
    {
        public static bool IsMinersBins_ALL_Init()
        {
            foreach (var filePath in Bins_Data.ALL_FILES_BINS)
            {
                if (!File.Exists($"bin{filePath}"))
                {
                    Helpers.ConsolePrint("MinersExistanceChecker", $"bin{filePath} doesn't exist! Warning");
                    return false;
                }
            }
            return true;
        }

        public static bool IsMiners3rdPartyBinsInit()
        {
            foreach (var filePath in Bins_Data_3rd.ALL_FILES_BINS)
            {
                if (!File.Exists($"bin_3rdparty{filePath}"))
                {
                    Helpers.ConsolePrint("MinersExistanceChecker", $"bin_3rdparty{filePath} doesn't exist! Warning");
                    return false;
                }
            }
            return true;
        }

        public static bool IsMinersBinsInit()
        {
            //return isOk;
            return IsMinersBins_ALL_Init();
        }
    }
}

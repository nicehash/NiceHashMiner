﻿namespace NiceHashMiner.Utils
{
    public static class MinersDownloadManager
    {
        public static DownloadSetup StandardDlSetup = new DownloadSetup(
            "http://github.com/NiceHash/NiceHashMinerLegacy/releases/download/1.9.0.1/bin_1_9_0_1.zip",
            "bins.zip",
            "bin");

        public static DownloadSetup ThirdPartyDlSetup = new DownloadSetup(
            "https://github.com/NiceHash/NiceHashMinerLegacy/releases/download/1.9.0.1/bin_3rdparty_1_9_0_1.zip",
            "bins_3rdparty.zip",
            "bin_3rdparty");
    }
}

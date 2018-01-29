namespace NiceHashMiner.Utils
{
    public static class MinersDownloadManager
    {
        public static DownloadSetup StandardDlSetup = new DownloadSetup(
            "http://github.com/NiceHash/NiceHashMinerLegacy/releases/download/1.8.1.7/bin_1_8_1_7.zip",
            "bins.zip",
            "bin");

        public static DownloadSetup ThirdPartyDlSetup = new DownloadSetup(
            "http://github.com/NiceHash/NiceHashMinerLegacy/releases/download/1.8.1.7/bin_3rdparty_1_8_1_7.zip",
            "bins_3rdparty.zip",
            "bin_3rdparty");
    }
}

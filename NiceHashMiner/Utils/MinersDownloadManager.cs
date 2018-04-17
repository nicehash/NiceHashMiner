namespace NiceHashMiner.Utils
{
    public static class MinersDownloadManager
    {
        public static DownloadSetup StandardDlSetup = new DownloadSetup(
            "https://github.com/NiceHash/NiceHashMinerLegacy/releases/download/1.8.1.11/bin_1_8_1_11.zip",
            "bins.zip",
            "bin");

        public static DownloadSetup ThirdPartyDlSetup = new DownloadSetup(
            "https://github.com/NiceHash/NiceHashMinerLegacy/releases/download/1.8.1.11/bin_3rdparty_1_8_1_11.zip",
            "bins_3rdparty.zip",
            "bin_3rdparty");
    }
}

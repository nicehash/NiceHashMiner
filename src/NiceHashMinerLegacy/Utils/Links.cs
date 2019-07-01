namespace NiceHashMiner
{
    public static class Links
    {
#if TESTNET
        public const string VisitUrl = "https://test.nicehash.com";
        public const string CheckStats = "https://test.nicehash.com/mining/stats";
#elif TESTNETDEV
        public const string VisitUrl = "https://test-dev.nicehash.com";
        public const string CheckStats = "https://test-dev.nicehash.com/mining/stats";
#elif PRODUCTION_NEW
        public const string VisitUrl = "https://new.nicehash.com";
        public const string CheckStats = "https://new.nicehash.com/mining/stats";
#else
        public const string VisitUrl = "https://www.nicehash.com";
        public const string CheckStats = "https://www.nicehash.com/index.jsp?p=miners&addr=";
#endif
        // add version
        public const string VisitReleasesUrl = "https://github.com/NiceHash/NiceHashMinerLegacy/releases/";
        public const string VisitNewVersionReleaseUrl = "https://github.com/NiceHash/NiceHashMinerLegacy/releases/tag/";

        // add btc adress as parameter

        // help and faq
        public const string NhmHelp = "https://github.com/nicehash/NiceHashMinerLegacy/";
        public const string NhmNoDevHelp = "https://github.com/nicehash/NiceHashMinerLegacy/wiki/Troubleshooting#nosupportdev";

        // faq
        //public const string NhmBtcWalletFaq = "https://www.nicehash.com/help/how-to-create-the-bitcoin-addresswallet";
        public const string NhmPayingFaq = "https://www.nicehash.com/help/when-and-how-do-you-get-paid";

        // API
        // btc adress as parameter
        //public const string NhmApiStats = "https://api.nicehash.com/api?method=stats.provider&addr=";
        //public const string NhmApiInfo = "https://api.nicehash.com/api?method=simplemultialgo.info";
        //public const string NhmApiVersion = "https://api.nicehash.com/nicehashminer?method=version&legacy";
        //public static string NHM_API_stats_provider_workers = "https://api.nicehash.com/api?method=stats.provider.workers&addr=";

        // device profits
        //public const string NhmProfitCheck = "https://api.nicehash.com/p=calc&name=";
    }
}

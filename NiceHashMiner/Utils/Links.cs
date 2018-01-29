namespace NiceHashMiner
{
    public static class Links
    {
        public static string VisitUrl = "https://www.nicehash.com?utm_source=NHM";

        // add version
        public static string VisitUrlNew = "https://github.com/NiceHash/NiceHashMinerLegacy/releases/tag/";

        // add btc adress as parameter
        public static string CheckStats = "https://www.nicehash.com/index.jsp?utm_source=NHM&p=miners&addr=";

        // help and faq
        public static string NhmHelp = "https://github.com/nicehash/NiceHashMinerLegacy/";
        public static string NhmNoDevHelp = "https://github.com/nicehash/NiceHashMinerLegacy/wiki/Troubleshooting#nosupportdev";

        // faq
        public static string NhmBtcWalletFaq = "https://www.nicehash.com/help/how-to-create-the-bitcoin-addresswallet?utm_source=NHM";
        public static string NhmPayingFaq = "https://www.nicehash.com/help/when-and-how-do-you-get-paid?utm_source=NHM";

        // API
        // btc adress as parameter
        public static string NhmApiStats = "https://api.nicehash.com/api?method=stats.provider&addr=";
        public static string NhmApiInfo = "https://api.nicehash.com/api?method=simplemultialgo.info";
        public static string NhmApiVersion = "https://api.nicehash.com/nicehashminer?method=version&legacy";
        //public static string NHM_API_stats_provider_workers = "https://api.nicehash.com/api?method=stats.provider.workers&addr=";

        // device profits
        public static string NhmProfitCheck = "https://api.nicehash.com/?utm_source=NHM&p=calc&name=";

        // SMA Socket
        public static string NhmSocketAddress = "wss://nhmws.nicehash.com/v2/nhm";
    }
}

namespace NiceHashMiner.Utils
{
    public static class Links
    {
#if TESTNET
        public const string VisitUrl = "https://test.nicehash.com";
        public const string CheckStats = "https://test.nicehash.com/mining/stats";
        public const string Register = "NO_URL";
        public const string Login = "NO_URL";
        public const string PluginsJsonApiUrl = "https://miner-plugins-test-dev.nicehash.com/api/plugins";
        public const string NhmPayingFaq = "https://www.nicehash.com/support/mining-help/earnings-and-payments/when-and-how-do-you-get-paid"; // ADD TESTNET
#elif TESTNETDEV
        public const string VisitUrl = "https://test-dev.nicehash.com";
        public const string CheckStats = "https://test-dev.nicehash.com/mining/stats";
        public const string Register = "NO_URL";
        public const string Login = "NO_URL";
        public const string PluginsJsonApiUrl = "https://miner-plugins-test-dev.nicehash.com/api/plugins";
        public const string NhmPayingFaq = "https://www.nicehash.com/support/mining-help/earnings-and-payments/when-and-how-do-you-get-paid";  // ADD TESTNETDEV
#elif PRODUCTION_NEW
        public const string VisitUrl = "https://nicehash.com";
        public const string CheckStats = "https://nicehash.com/my/mining/stats";
        public const string Register = "https://nicehash.com/my/register";
        public const string Login = "NO_URL";
        public const string PluginsJsonApiUrl = "https://miner-plugins.nicehash.com/api/plugins";
        public const string NhmPayingFaq = "https://www.nicehash.com/support/mining-help/earnings-and-payments/when-and-how-do-you-get-paid";
#else
        public const string VisitUrl = "https://old.nicehash.com";
        public const string CheckStats = "https://old.nicehash.com/index.jsp?p=miners&addr=";
        public const string Register = "https://old.nicehash.com/register";
        public const string Login = "https://old.nicehash.com/login-app?back";
        public const string PluginsJsonApiUrl = "https://miner-plugins.nicehash.com/api/plugins";
        public const string NhmPayingFaq = "https://old.nicehash.com/help/when-and-how-do-you-get-paid";
#endif
        // add version
        public const string VisitReleasesUrl = "https://github.com/NiceHash/NiceHashMiner/releases/";
        public const string VisitNewVersionReleaseUrl = "https://github.com/NiceHash/NiceHashMiner/releases/tag/";

        public const string UpdaterUrlTemplate = "https://github.com/nicehash/NiceHashMiner/releases/download/{VERSION_TAG}/nhm_windows_updater_{VERSION_TAG}.exe";


        // add btc adress as parameter

        // help and faq
        public const string NhmHelp = "https://github.com/nicehash/NiceHashMiner/";
        public const string NhmNoDevHelp = "https://github.com/nicehash/NiceHashMiner/wiki/Troubleshooting#nosupportdev";
    }
}

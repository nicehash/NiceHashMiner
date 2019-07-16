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
#elif TESTNETDEV
        public const string VisitUrl = "https://test-dev.nicehash.com";
        public const string CheckStats = "https://test-dev.nicehash.com/mining/stats";
        public const string Register = "NO_URL";
        public const string Login = "NO_URL";
        public const string PluginsJsonApiUrl = "https://miner-plugins-test-dev.nicehash.com/api/plugins";
#elif PRODUCTION_NEW
        public const string VisitUrl = "https://new.nicehash.com";
        public const string CheckStats = "https://new.nicehash.com/my/mining/stats";
        public const string Register = "https://new.nicehash.com/my/register";
        public const string Login = "NO_URL";
        public const string PluginsJsonApiUrl = "https://miner-plugins.nicehash.com/api/plugins";
#else
        public const string VisitUrl = "https://www.nicehash.com";
        public const string CheckStats = "https://www.nicehash.com/index.jsp?p=miners&addr=";
        public const string Register = "https://www.nicehash.com/register";
        public const string Login = "https://www.nicehash.com/login-app?back";
        public const string PluginsJsonApiUrl = "https://miner-plugins.nicehash.com/api/plugins";
#endif
        // add version
        public const string VisitReleasesUrl = "https://github.com/NiceHash/NiceHashMiner/releases/";
        public const string VisitNewVersionReleaseUrl = "https://github.com/NiceHash/NiceHashMiner/releases/tag/";

        // add btc adress as parameter

        // help and faq
        public const string NhmHelp = "https://github.com/nicehash/NiceHashMiner/";
        public const string NhmNoDevHelp = "https://github.com/nicehash/NiceHashMiner/wiki/Troubleshooting#nosupportdev";

        // faq
        public const string NhmPayingFaq = "https://www.nicehash.com/help/when-and-how-do-you-get-paid";
    }
}

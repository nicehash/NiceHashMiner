using NHM.Common;
using NHM.Common.Enums;

namespace NHMCore.Utils
{
    public static class Links
    {
// TESTNET
        public const string VisitUrl_TESTNET = "https://test.nicehash.com";
        public const string CheckStats_TESTNET = "https://test.nicehash.com/mining/stats";
        public const string CheckStatsRig_TESTNET = "https://test.nicehash.com/my/mining/rigs/{RIG_ID}";
        public const string Register_TESTNET = "NO_URL";
        public const string Login_TESTNET = "https://test.nicehash.com/my/login";
        public const string NhmPayingFaq_TESTNET = "https://www.nicehash.com/support/mining-help/earnings-and-payments/when-and-how-do-you-get-paid";
        public const string AMDComputeModeHelp_TESTNET = "https://www.nicehash.com/blog/post/how-to-enable-compute-mode-on-amd-cards-and-double-your-hash-rate%3F";
        public const string LoginNHM_TESTNET = "NO_URL";
        // TESTNETDEV
        public const string VisitUrl_TESTNETDEV = "https://test-dev.nicehash.com";
        public const string CheckStats_TESTNETDEV = "https://test-dev.nicehash.com/mining/stats";
        public const string CheckStatsRig_TESTNETDEV = "https://test-dev.nicehash.com/my/mining/rigs/{RIG_ID}";
        public const string Register_TESTNETDEV = "NO_URL";
        public const string Login_TESTNETDEV = "https://test-dev.nicehash.com/my/login";
        public const string NhmPayingFaq_TESTNETDEV = "https://www.nicehash.com/support/mining-help/earnings-and-payments/when-and-how-do-you-get-paid";
        public const string AMDComputeModeHelp_TESTNETDEV = "https://www.nicehash.com/blog/post/how-to-enable-compute-mode-on-amd-cards-and-double-your-hash-rate%3F";
        public const string LoginNHM_TESTNETDEV = "https://test-dev.nicehash.com/my/login?nhm=1";
        // PRODUCTION
        public const string VisitUrl_PRODUCTION = "https://nicehash.com";
        public const string CheckStats_PRODUCTION = "https://nicehash.com/my/mining/stats";
        public const string CheckStatsRig_PRODUCTION = "https://www.nicehash.com/my/mining/rigs/{RIG_ID}";
        public const string Register_PRODUCTION = "https://nicehash.com/my/register";
        public const string Login_PRODUCTION = "https://www.nicehash.com/my/login";
        public const string NhmPayingFaq_PRODUCTION = "https://www.nicehash.com/support/mining-help/earnings-and-payments/when-and-how-do-you-get-paid";
        public const string AMDComputeModeHelp_PRODUCTION = "https://www.nicehash.com/blog/post/how-to-enable-compute-mode-on-amd-cards-and-double-your-hash-rate%3F";
        public const string AddWDExclusionHelp_PRODUCTION = "https://www.nicehash.com/blog/post/how-to-add-nicehash-miner-folder-to-windows-defender-exclusion%3F";
        public const string LoginNHM_PRODUCTION = "https://www.nicehash.com/my/login?nhm=1"; //TODO MUST GET LINK FOR PRODUCTION

        public static string VisitUrl
        {
            get
            {
                if (BuildOptions.BUILD_TAG == BuildTag.TESTNET) return VisitUrl_TESTNET;
                if (BuildOptions.BUILD_TAG == BuildTag.TESTNETDEV) return VisitUrl_TESTNETDEV;
                //BuildTag.PRODUCTION
                return VisitUrl_PRODUCTION;
            }
        }

        public static string CheckStats
        {
            get
            {
                if (BuildOptions.BUILD_TAG == BuildTag.TESTNET) return CheckStats_TESTNET;
                if (BuildOptions.BUILD_TAG == BuildTag.TESTNETDEV) return CheckStats_TESTNETDEV;
                //BuildTag.PRODUCTION
                return CheckStats_PRODUCTION;
            }
        }
        public static string CheckStatsRig
        {
            get
            {
                if (BuildOptions.BUILD_TAG == BuildTag.TESTNET) return CheckStatsRig_TESTNET;
                if (BuildOptions.BUILD_TAG == BuildTag.TESTNETDEV) return CheckStatsRig_TESTNETDEV;
                //BuildTag.PRODUCTION
                return CheckStatsRig_PRODUCTION;
            }
        }

        public static string Register
        {
            get
            {
                if (BuildOptions.BUILD_TAG == BuildTag.TESTNET) return Register_TESTNET;
                if (BuildOptions.BUILD_TAG == BuildTag.TESTNETDEV) return Register_TESTNETDEV;
                //BuildTag.PRODUCTION
                return Register_PRODUCTION;
            }
        }
        public static string Login
        {
            get
            {
                if (BuildOptions.BUILD_TAG == BuildTag.TESTNET) return Login_TESTNET;
                if (BuildOptions.BUILD_TAG == BuildTag.TESTNETDEV) return Login_TESTNETDEV;
                //BuildTag.PRODUCTION
                return Login_PRODUCTION;
            }
        }
        public static string LoginNHM
        {
            get
            {
                if (BuildOptions.BUILD_TAG == BuildTag.TESTNET) return LoginNHM_TESTNET;
                if (BuildOptions.BUILD_TAG == BuildTag.TESTNETDEV) return LoginNHM_TESTNETDEV;
                //BuildTag.PRODUCTION
                return LoginNHM_PRODUCTION;
            }
        }
        public static string NhmPayingFaq
        {
            get
            {
                if (BuildOptions.BUILD_TAG == BuildTag.TESTNET) return NhmPayingFaq_TESTNET;
                if (BuildOptions.BUILD_TAG == BuildTag.TESTNETDEV) return NhmPayingFaq_TESTNETDEV;
                //BuildTag.PRODUCTION
                return NhmPayingFaq_PRODUCTION;
            }
        }

        public static string PluginsJsonApiUrl
        {
            get
            {
                if (BuildOptions.IS_PLUGINS_TEST_SOURCE) return "https://miner-plugins-test-dev.nicehash.com/api/plugins";
                return "https://miner-plugins.nicehash.com/api/plugins";
            }
        }
        
        
        // add version
        public const string VisitReleasesUrl = "https://github.com/NiceHash/NiceHashMiner/releases/";
        public const string VisitNewVersionReleaseUrl = "https://github.com/NiceHash/NiceHashMiner/releases/tag/";


        // add btc adress as parameter

        // help and faq
        public const string NhmHelp = "https://github.com/nicehash/NiceHashMiner/";
        public const string NhmNoDevHelp = "https://github.com/nicehash/NiceHashMiner/blob/master/doc/Troubleshooting.md#-no-supported-devices";

        //about
        public const string About = "https://www.nicehash.com/support/general-help/nicehash-service/what-is-nicehash-and-how-it-works";

        //nvidia help
        public const string NvidiaDriversHelp = "https://www.nvidia.com/download/find.aspx";
        public const string AVHelp = "https://www.nicehash.com/blog/post/how-to-add-nicehash-miner-folder-to-windows-defender-exclusion%253F";
        public const string LargePagesHelp = "https://www.nicehash.com/blog/post/how-to-optimize-cpu-mining-performance-for-monero-random-x?utm_source=NHM&utm_medium=referral&utm_campaign=optimize%20cpu";      
        public const string VirtualMemoryHelp = "https://www.nicehash.com/blog/post/how-to-increase-virtual-memory-on-windows?utm_source=NHM&utm_medium=referral&utm_campaign=nicehash%20miner";      
    }
}

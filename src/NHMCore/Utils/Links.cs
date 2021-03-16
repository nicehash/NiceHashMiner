using NHM.Common;
using NHM.Common.Enums;

namespace NHMCore.Utils
{
    public static class Links
    {
        public static string AddWDExclusionHelp_PRODUCTION => "https://www.nicehash.com/blog/post/how-to-add-nicehash-miner-folder-to-windows-defender-exclusion%3F?utm_source=NHM&utm_medium=Guide";


        public static string VisitUrl
        {
            get
            {
                switch (BuildOptions.BUILD_TAG)
                {
                    case BuildTag.TESTNET: return "https://test.nicehash.com";
                    case BuildTag.TESTNETDEV: return "https://test-dev.nicehash.com";
                    // BuildTag.PRODUCTION
                    default: return "https://nicehash.com";
                }
            }
        }

        public static string CheckStats
        {
            get
            {
                switch (BuildOptions.BUILD_TAG)
                {
                    case BuildTag.TESTNET: return "https://test.nicehash.com/mining/stats";
                    case BuildTag.TESTNETDEV: return "https://test-dev.nicehash.com/mining/stats";
                    // BuildTag.PRODUCTION
                    default: return "https://nicehash.com/my/mining/stats";
                }
            }
        }
        public static string CheckStatsRig
        {
            get
            {
                switch (BuildOptions.BUILD_TAG)
                {
                    case BuildTag.TESTNET: return "https://test.nicehash.com/my/mining/rigs/{RIG_ID}";
                    case BuildTag.TESTNETDEV: return "https://test-dev.nicehash.com/my/mining/rigs/{RIG_ID}";
                    // BuildTag.PRODUCTION
                    default: return "https://www.nicehash.com/my/mining/rigs/{RIG_ID}?utm_source=NHM&utm_medium=ViewStatsOnline";
                }
            }
        }

        public static string Register
        {
            get
            {
                // TODO missing
                switch (BuildOptions.BUILD_TAG)
                {
                    case BuildTag.TESTNET: return "NO_URL";
                    case BuildTag.TESTNETDEV: return "NO_URL";
                    // BuildTag.PRODUCTION
                    default: return "https://nicehash.com/my/register";
                }
            }
        }

        // ?nhm=1 - LoginNHM
        public static string Login
        {
            get
            {
                switch (BuildOptions.BUILD_TAG)
                {
                    case BuildTag.TESTNET: return "https://test.nicehash.com/my/login";
                    case BuildTag.TESTNETDEV: return "https://test-dev.nicehash.com/my/login";
                    // BuildTag.PRODUCTION
                    default: return "https://www.nicehash.com/my/login";
                }
            }
        }

        public static string NhmPayingFaq
        {
            get
            {
                // TODO same for all builds
                switch (BuildOptions.BUILD_TAG)
                {
                    case BuildTag.TESTNET: return "https://www.nicehash.com/support/mining-help/earnings-and-payments/when-and-how-do-you-get-paid";
                    case BuildTag.TESTNETDEV: return "https://www.nicehash.com/support/mining-help/earnings-and-payments/when-and-how-do-you-get-paid";
                    // BuildTag.PRODUCTION
                    default: return "https://www.nicehash.com/support/mining-help/earnings-and-payments/when-and-how-do-you-get-paid?utm_source=NHM&utm_medium=Guide";
                }
            }
        }

        public static string AMDComputeModeHelp
        {
            get
            {
                // TODO same for all builds
                switch (BuildOptions.BUILD_TAG)
                {
                    case BuildTag.TESTNET: return "https://www.nicehash.com/blog/post/how-to-enable-compute-mode-on-amd-cards-and-double-your-hash-rate%3F";
                    case BuildTag.TESTNETDEV: return "https://www.nicehash.com/blog/post/how-to-enable-compute-mode-on-amd-cards-and-double-your-hash-rate%3F";
                    // BuildTag.PRODUCTION
                    default: return "https://www.nicehash.com/blog/post/how-to-enable-compute-mode-on-amd-cards-and-double-your-hash-rate?utm_source=NHM&utm_medium=Guide";
                }
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
        public static string VisitReleasesUrl => "https://github.com/NiceHash/NiceHashMiner/releases/";
        public static string VisitNewVersionReleaseUrl => "https://github.com/NiceHash/NiceHashMiner/releases/tag/";


        // add btc adress as parameter

        // help and faq
        public static string NhmHelp => "https://github.com/nicehash/NiceHashMiner/";
        public static string NhmNoDevHelp => "https://github.com/nicehash/NiceHashMiner/blob/master/doc/Troubleshooting.md#-no-supported-devices";
        public static string FailedBenchmarkHelp => "https://www.nicehash.com/blog/post/benchmark-error-in-nicehash-miner";

        //about
        public static string About => "https://www.nicehash.com/support/general-help/nicehash-service/what-is-nicehash-and-how-it-works";

        //nvidia help
        public static string NvidiaDriversHelp => "https://www.nvidia.com/download/find.aspx";
        public static string AVHelp => "https://www.nicehash.com/blog/post/how-to-add-nicehash-miner-folder-to-windows-defender-exclusion%253F";
        public static string LargePagesHelp => "https://www.nicehash.com/blog/post/how-to-optimize-cpu-mining-performance-for-monero-random-x?utm_source=NHM&utm_medium=referral&utm_campaign=optimize%20cpu";
        public static string VirtualMemoryHelp => "https://www.nicehash.com/blog/post/how-to-increase-virtual-memory-on-windows?utm_source=NHM&utm_medium=referral&utm_campaign=nicehash%20miner";
    }
}

using NHM.Common;
using NHM.Common.Enums;

namespace NHMCore.Utils
{
    public static class Links
    {
        public static string VisitUrl => BuildOptions.BUILD_TAG switch
        {
            BuildTag.TESTNET => "https://test.nicehash.com",
            BuildTag.TESTNETDEV => "https://test-dev.nicehash.com",
            _ => "https://nicehash.com", // BuildTag.PRODUCTION
        };

        public static string CheckStats => BuildOptions.BUILD_TAG switch
        {
            BuildTag.TESTNET => "https://test.nicehash.com/mining/stats",
            BuildTag.TESTNETDEV => "https://test-dev.nicehash.com/mining/stats",
            _ => "https://nicehash.com/my/mining/stats", // BuildTag.PRODUCTION
        };

        public static string CheckStatsRig => BuildOptions.BUILD_TAG switch
        {
            BuildTag.TESTNET => "https://test.nicehash.com/my/mining/rigs/{RIG_ID}",
            BuildTag.TESTNETDEV => "https://test-dev.nicehash.com/my/mining/rigs/{RIG_ID}",
            _ => "https://www.nicehash.com/my/mining/rigs/{RIG_ID}?utm_source=NHM&utm_medium=ViewStatsOnline", // BuildTag.PRODUCTION
        };


        public static string Register => BuildOptions.BUILD_TAG switch
        {
            BuildTag.TESTNET => "NO_URL",
            BuildTag.TESTNETDEV => "NO_URL",
            _ => "https://nicehash.com/my/register", // BuildTag.PRODUCTION
        };

        // ?nhm=1 - LoginNHM
        public static string Login => BuildOptions.BUILD_TAG switch
        {
            BuildTag.TESTNET => "https://test.nicehash.com/my/login",
            BuildTag.TESTNETDEV => "https://test-dev.nicehash.com/my/login",
            _ => "https://www.nicehash.com/my/login", // BuildTag.PRODUCTION
        };

        public static string NhmPayingFaq => BuildOptions.BUILD_TAG switch
        {
            BuildTag.TESTNET => "https://www.nicehash.com/support/mining-help/earnings-and-payments/when-and-how-do-you-get-paid",
            BuildTag.TESTNETDEV => "https://www.nicehash.com/support/mining-help/earnings-and-payments/when-and-how-do-you-get-paid",
            _ => "https://www.nicehash.com/support/mining-help/earnings-and-payments/when-and-how-do-you-get-paid?utm_source=NHM&utm_medium=Guide", // BuildTag.PRODUCTION
        };

        public static string PluginsJsonApiUrl => BuildOptions.IS_PLUGINS_TEST_SOURCE switch
        {
            true => "https://miner-plugins-test-dev.nicehash.com/api/plugins",
            _ => "https://miner-plugins.nicehash.com/api/plugins",
        };


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
        public static string NoOptimalDrivers => "https://www.nicehash.com/blog/post/psa-nvidia-driver-526-47-issues-when-mining-or-gaming?";
    }
}

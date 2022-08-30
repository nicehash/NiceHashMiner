using NHM.Common.Enums;

namespace NHM.Common
{
    public static class Nhmws
    {
        // SMA Socket
        internal static string BuildTagNhmSocketAddress() => 
            BuildOptions.BUILD_TAG switch {
                BuildTag.TESTNET => "wss://nhmws-test.nicehash.com/v3/nhml",
                BuildTag.TESTNETDEV => "wss://nhmws-dev.nicehash.com/v3/nhml",
                _ => "wss://nhmws.nicehash.com/v3/nhml", // BuildTag.PRODUCTION
            };

        public static string NhmSocketAddress =>
            BuildOptions.CUSTOM_ENDPOINTS_ENABLED switch
            {
                true => StratumServiceHelpers.NhmSocketAddress,
                _ => BuildTagNhmSocketAddress(),
            };
    }
}

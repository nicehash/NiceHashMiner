
using NHM.Common.Enums;

namespace NHM.Common
{
    public static class Nhmws
    {
        // SMA Socket

        internal static string BuildTagNhmSocketAddress()
        {
            if (BuildOptions.BUILD_TAG == BuildTag.TESTNET) return "wss://nhmws-test.nicehash.com/v3/nhml";
            if (BuildOptions.BUILD_TAG == BuildTag.TESTNETDEV) return "wss://nhmws-test-dev.nicehash.com/v3/nhml";
            //BuildTag.PRODUCTION
            return "wss://nhmws.nicehash.com/v3/nhml";
        }

        public static string NhmSocketAddress
        {
            get
            {
                if (BuildOptions.CUSTOM_ENDPOINTS_ENABLED) return StratumServiceHelpers.NhmSocketAddress;
                return BuildTagNhmSocketAddress();
            }
        }
    }
}

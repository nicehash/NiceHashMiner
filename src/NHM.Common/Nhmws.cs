
namespace NHM.Common
{
    public static class Nhmws
    {
        // SMA Socket
#if CUSTOM_ENDPOINTS
        public static string NhmSocketAddress => StratumServiceHelpers.NhmSocketAddress;
#elif TESTNET
        public const string NhmSocketAddress = "https://nhmws-test.nicehash.com/v3/nhml"; 
#elif TESTNETDEV
        public const string NhmSocketAddress = "https://nhmws-test-dev.nicehash.com/v3/nhml"; 
#else
        public const string NhmSocketAddress = "https://nhmws.nicehash.com/v3/nhml";
#endif
    }
}

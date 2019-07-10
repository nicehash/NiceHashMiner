using System;
using System.Collections.Generic;
using System.Text;

namespace NHM.Common
{
    public static class Nhmws
    {
        // SMA Socket
#if CUSTOM_ENDPOINTS
        public static string NhmSocketAddress => StratumServiceHelpers.NhmSocketAddress;
#elif TESTNET
        public const string NhmSocketAddress = "https://nhmws-test.nicehash.com/v3/nhml"; // new endpoint with balances and exchanges
#elif TESTNETDEV
        public const string NhmSocketAddress = "https://nhmws-test-dev.nicehash.com/v3/nhml"; // new endpoint with balances and exchanges
#elif PRODUCTION_NEW
        public const string NhmSocketAddress = "https://nhmws-new.nicehash.com/v3/nhml"; // new platform
#else
        public const string NhmSocketAddress = "https://nhmws.nicehash.com/v2/nhm";
#endif
    }
}

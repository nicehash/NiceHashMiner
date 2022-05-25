//#define USE_NHMWS4
using NHM.Common;
using NHM.Common.Enums;
using System.Threading;
using System.Threading.Tasks;

namespace NHMCore.Nhmws
{
#if USE_NHMWS4
    using NHWebSocketImpl = NHMCore.Nhmws.V4.NHWebSocketV4;
#else
    using NHWebSocketImpl = NHMCore.Nhmws.V3.NHWebSocketV3;
#endif

    static class NHWebSocket
    {
        private static class NhmwsWrapper
        {
            internal static string BuildTagNhmSocketAddressV4() =>
                BuildOptions.BUILD_TAG switch
                {
                    BuildTag.TESTNET => "wss://nhmws-test.nicehash.com/v4/nhm",
                    BuildTag.TESTNETDEV => "wss://nhmws-test-dev.nicehash.com/v4/nhm",
                    _ => "wss://nhmws.nicehash.com/v4/nhm", // BuildTag.PRODUCTION
                };


            public static string NhmSocketAddress
            {
                get
                {
                    if (BuildOptions.CUSTOM_ENDPOINTS_ENABLED) return StratumServiceHelpers.NhmSocketAddress;
#if USE_NHMWS4
                    return BuildTagNhmSocketAddressV4();
#else
                    return NHM.Common.Nhmws.NhmSocketAddress;
#endif
                }
            }
        }

        public static void NotifyStateChanged() => NHWebSocketImpl.NotifyStateChanged();

        public static Task MainLoop => NHWebSocketImpl.MainLoop;

        public static void StartLoop(CancellationToken token) => NHWebSocketImpl.StartLoop(NhmwsWrapper.NhmSocketAddress, token);
        //public static void StartLoop(string address, CancellationToken token) => NHWebSocketImpl.StartLoop(address, token);

        static public void ResetCredentials(string btc = null, string worker = null, string group = null) => NHWebSocketImpl.ResetCredentials(btc, worker, group);

        static public void SetCredentials(string btc = null, string worker = null, string group = null) => NHWebSocketImpl.SetCredentials(btc, worker, group);
    }
}

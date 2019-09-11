
namespace NHM.Common
{
    public static class BUILD_TAG
    {
#if TESTNET
        public static readonly string BuildTag = "TESTNET";
#elif TESTNETDEV
        public static readonly string BuildTag = "TESTNETDEV";
#elif PRODUCTION_NEW
        public static readonly string BuildTag = "PRODUCTION_NEW";
#else
        public static readonly string BuildTag = "PRODUCTION";
#endif
    }
}

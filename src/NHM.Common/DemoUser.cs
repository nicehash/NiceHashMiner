using NHM.Common.Enums;

namespace NHM.Common
{
    public static class DemoUser
    {
        public static string BTC =>
            BuildOptions.BUILD_TAG switch
            {
                BuildTag.TESTNET => "2N6ibfrTwUSSvzAz1esPe1gYULG82asTHiS",
                BuildTag.TESTNETDEV => "2N2e2ET1jMY9r5is9KaTKnU3bkCFaYHEEEx",
                _ => "NHbQTA7iMnCUuPjhkVCa99zbNUDFAry1nLH4", // BuildTag.PRODUCTION
            };
    }
}

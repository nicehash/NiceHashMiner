
namespace NHM.Common
{
    public static class DemoUser
    {
#if TESTNET
        public static readonly string BTC = "2N6ibfrTwUSSvzAz1esPe1gYULG82asTHiS";
#elif TESTNETDEV
        public static readonly string BTC = "2N2e2ET1jMY9r5is9KaTKnU3bkCFaYHEEEx"; // TODO
#else
        public static readonly string BTC = "33hGFJZQAfbdzyHGqhJPvZwncDjUBdZqjW";
#endif
    }
}

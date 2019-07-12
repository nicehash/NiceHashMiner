
namespace NiceHashMiner
{
    // during the transition 
    internal static class NHMProductInfo
    {
#if TESTNET || TESTNETDEV || PRODUCTION_NEW // NEW PRODUCTION
        public static string Name => "NiceHash Miner";
#else  // OLD PRODUCTION
        public static string Name => "NiceHash Miner Legacy";
#endif

        // shared
        public static string ChooseLanguage => $"Choose a default language for {Name}:";
        public static string TermsOfUse => $"{Name} Terms Of Use";
    }
}

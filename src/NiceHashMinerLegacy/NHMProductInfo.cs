using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
    }
}

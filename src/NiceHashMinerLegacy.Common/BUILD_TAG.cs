using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NiceHashMinerLegacy.Common
{
    public static class BUILD_TAG
    {
#if TESTNET
        public static readonly string BuildTag = "TESTNET";
#elif TESTNETDEV
        public static readonly string BuildTag = "TESTNETDEV";
#else
        public static readonly string BuildTag = "PRODUCTION";
#endif
    }
}

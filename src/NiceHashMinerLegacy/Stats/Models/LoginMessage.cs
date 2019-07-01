using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NiceHashMiner.Stats.Models
{
#pragma warning disable 649, IDE1006
    class LoginMessage
    {
        public string method => "login";
        public string version { get; set; } = "";
        public int protocol { get; set; }

        // Or maybe use omit emtpy 
        // TESTNET
#if TESTNET || TESTNETDEV || PRODUCTION_NEW
        public string btc { get; set; } = "";
        public string worker { get; set; } = "";
        public string group { get; set; } = "";
        public string rig { get; set; } = "";
#endif
    }
#pragma warning restore 649, IDE1006
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NiceHashMiner.Stats.Models
{
    class MarketsMessage
    {
        public string method => "markets";
        public IEnumerable<string> data { get; set; }
    }
}

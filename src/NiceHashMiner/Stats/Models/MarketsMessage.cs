using System.Collections.Generic;

namespace NiceHashMiner.Stats.Models
{
    class MarketsMessage
    {
        public string method => "markets";
        public IEnumerable<string> data { get; set; }
    }
}

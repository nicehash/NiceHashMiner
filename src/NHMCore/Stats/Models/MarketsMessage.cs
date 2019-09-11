using System.Collections.Generic;

namespace NHMCore.Stats.Models
{
    class MarketsMessage
    {
        public string method => "markets";
        public IEnumerable<string> data { get; set; }
    }
}

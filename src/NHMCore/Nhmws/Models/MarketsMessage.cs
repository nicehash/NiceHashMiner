using System.Collections.Generic;

namespace NHMCore.Nhmws.Models
{
    class MarketsMessage
    {
        public string method => "markets";
        public IEnumerable<string> data { get; set; }
    }
}

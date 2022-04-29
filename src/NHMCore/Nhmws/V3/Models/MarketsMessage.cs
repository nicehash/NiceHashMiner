using System.Collections.Generic;

namespace NHMCore.Nhmws.V3.Models
{
    class MarketsMessage
    {
        public string method => "markets";
        public IEnumerable<string> data { get; set; }
    }
}

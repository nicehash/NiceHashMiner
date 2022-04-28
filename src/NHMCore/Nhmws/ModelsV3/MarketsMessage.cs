using System.Collections.Generic;

namespace NHMCore.Nhmws.ModelsV3
{
    class MarketsMessage
    {
        public string method => "markets";
        public IEnumerable<string> data { get; set; }
    }
}

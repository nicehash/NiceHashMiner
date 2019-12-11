using System.Collections.Generic;

namespace NHMCore.Nhmws.Models
{
#pragma warning disable 649, IDE1006
    class ExchangeRateJson
    {
        public List<Dictionary<string, string>> exchanges { get; set; }
        public Dictionary<string, double> exchanges_fiat { get; set; }
    }
#pragma warning restore 649, IDE1006
}

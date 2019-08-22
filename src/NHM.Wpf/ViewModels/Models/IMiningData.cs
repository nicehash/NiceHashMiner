using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NHM.Common;

namespace NHM.Wpf.ViewModels.Models
{
    public interface IMiningData
    {
        string Name { get; }
        string StateName { get; }

        double Hashrate { get; }
        IEnumerable<Hashrate> Speeds { get; }
        double Payrate { get; }
        double FiatPayrate { get; }
        double PowerUsage { get; }
        double PowerCost { get; }
        double Profit { get; }
    }
}

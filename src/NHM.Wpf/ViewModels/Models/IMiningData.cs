using NHM.Common;
using System.Collections.Generic;

namespace NHM.Wpf.ViewModels.Models
{
    /// <summary>
    /// Contract for object that can show info on mining status ListView.
    /// </summary>
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

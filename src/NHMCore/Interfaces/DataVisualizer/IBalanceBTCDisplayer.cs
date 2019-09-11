using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NHMCore.Interfaces.DataVisualizer
{
    public interface IBalanceBTCDisplayer : IDataVisualizer
    {
        void DisplayBTCBalance(object sender, double btcBalance);
    }
}

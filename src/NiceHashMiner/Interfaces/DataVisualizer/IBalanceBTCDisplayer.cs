using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NiceHashMiner.Interfaces.DataVisualizer
{
    interface IBalanceBTCDisplayer : IDataVisualizer
    {
        void DisplayBTCBalance(object sender, double btcBalance);
    }
}

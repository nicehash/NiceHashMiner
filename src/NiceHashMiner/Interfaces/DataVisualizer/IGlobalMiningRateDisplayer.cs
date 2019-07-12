using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NiceHashMiner.Interfaces.DataVisualizer
{
    // TODO rename to total?
    interface IGlobalMiningRateDisplayer : IDataVisualizer
    {
        void DisplayGlobalMiningRate(object sender, double totalMiningRate);
    }
}

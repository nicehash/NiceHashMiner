using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NHMCore.Interfaces.DataVisualizer
{
    // TODO rename to total?
    public interface IGlobalMiningRateDisplayer : IDataVisualizer
    {
        void DisplayGlobalMiningRate(object sender, double totalMiningRate);
    }
}

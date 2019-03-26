using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NiceHashMiner.Interfaces.DataVisualizer
{
    interface IBTCDisplayer : IDataVisualizer
    {
        void DisplayBTC(object sender, string btc);
    }
}

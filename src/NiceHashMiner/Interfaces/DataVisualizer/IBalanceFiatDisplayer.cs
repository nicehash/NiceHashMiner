using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NiceHashMiner.Interfaces.DataVisualizer
{
    interface IBalanceFiatDisplayer : IDataVisualizer
    {
        void DisplayFiatBalance(object sender, (double fiatBalance, string fiatCurrencySymbol) args);
    }
}

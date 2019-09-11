using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NHMCore.Interfaces.DataVisualizer
{
    public interface IBalanceFiatDisplayer : IDataVisualizer
    {
        void DisplayFiatBalance(object sender, (double fiatBalance, string fiatCurrencySymbol) args);
    }
}

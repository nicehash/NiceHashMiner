using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NiceHashMiner.Interfaces.DataVisualizer
{
    interface INoInternetConnectionDisplayer : IDataVisualizer
    {
        void DisplayNoInternetConnection(object sender, bool noInternet);
    }
}

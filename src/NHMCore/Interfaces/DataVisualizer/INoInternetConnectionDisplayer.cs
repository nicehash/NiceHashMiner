using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NHMCore.Interfaces.DataVisualizer
{
    public interface INoInternetConnectionDisplayer : IDataVisualizer
    {
        void DisplayNoInternetConnection(object sender, bool noInternet);
    }
}

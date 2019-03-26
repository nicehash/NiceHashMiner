using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NiceHashMiner.Interfaces.DataVisualizer
{
    interface IVersionDisplayer : IDataVisualizer
    {
        void DisplayVersion(object sender, string version);
    }
}

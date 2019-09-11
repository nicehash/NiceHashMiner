using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NHMCore.Interfaces.DataVisualizer
{
    public interface IDevicesStateDisplayer : IDataVisualizer
    {
        void RefreshDeviceListView(object sender, EventArgs _);
    }
}

using System;

namespace NHMCore.Interfaces.DataVisualizer
{
    public interface IDevicesStateDisplayer : IDataVisualizer
    {
        void RefreshDeviceListView(object sender, EventArgs _);
    }
}

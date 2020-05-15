using NHM.Common.Device;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NHM.MinerPluginToolkitV1.Interfaces
{
    /// <summary>
    /// IDevicesCrossReference interface is used by plugins to map detected devices for miners with custom id ordering. 
    /// Implement this interface to safely map detected devices with miner ids (this state should be stored in the IPlugin object).
    /// For miners that don't order devices by CUDA or OpenCL IDs you will need to map them by PciBus IDs.
    /// </summary>
    public interface IDevicesCrossReference
    {
        Task DevicesCrossReference(IEnumerable<BaseDevice> devices);
    }
}

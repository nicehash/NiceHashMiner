using System.Collections.Generic;
using System.Threading.Tasks;
using NiceHashMinerLegacy.Common.Device;

namespace MinerPluginToolkitV1.Interfaces
{
    /// <summary>
    /// IDevicesCrossReference interface is used by plugins with miners that don't order devices by pciBus Ids
    /// For 
    /// </summary>
    public interface IDevicesCrossReference
    {
        Task DevicesCrossReference(IEnumerable<BaseDevice> devices);
    }
}

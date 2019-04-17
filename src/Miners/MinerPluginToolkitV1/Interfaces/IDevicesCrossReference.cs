using System.Collections.Generic;
using System.Threading.Tasks;
using NiceHashMinerLegacy.Common.Device;

namespace MinerPluginToolkitV1.Interfaces
{
    interface IDevicesCrossReference
    {
        Task DevicesCrossReference(IEnumerable<BaseDevice> devices);
    }
}

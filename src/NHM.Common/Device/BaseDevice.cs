using Newtonsoft.Json;
using NHM.Common.Enums;

namespace NHM.Common.Device
{
    public class BaseDevice
    {
        public string Name { get; init; }
        public DeviceType DeviceType { get; init; }
        public string UUID { get; init; }

        // TODO the ID will correspond to CPU Index, CUDA ID and AMD/OpenCL ID
        public int ID { get; init; }
    }
}

using Newtonsoft.Json;
using NHM.Common.Enums;

namespace NHM.Common.Device
{
    public class BaseDevice
    {
        public BaseDevice(BaseDevice bd)
        {
            DeviceType = bd.DeviceType;
            UUID = bd.UUID;
            Name = bd.Name;
            ID = bd.ID;
        }


        [JsonConstructor]
        public BaseDevice(DeviceType deviceType, string uuid, string name, int id)
        {
            DeviceType = deviceType;
            UUID = uuid;
            Name = name;
            ID = id;
        }
        public string Name { get; }
        public DeviceType DeviceType { get; }
        public string UUID { get; }

        // TODO the ID will correspond to CPU Index, CUDA ID and AMD/OpenCL ID
        public int ID { get; }
    }
}

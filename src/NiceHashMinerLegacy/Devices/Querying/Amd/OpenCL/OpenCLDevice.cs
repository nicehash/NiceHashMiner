using System;

namespace NiceHashMiner.Devices.Querying.Amd.OpenCL
{
    [Serializable]
    public class OpenCLDevice
    {
        public int AMD_BUS_ID { get; set; } = -1; // -1 indicates that it is not set
        public uint DeviceID { get; set; }
        public ulong _CL_DEVICE_GLOBAL_MEM_SIZE { get; set; } = 0;
        public string _CL_DEVICE_NAME { get; set; }
        public string _CL_DEVICE_TYPE { get; set; }
        public string _CL_DEVICE_VENDOR { get; set; }
        public string _CL_DEVICE_VERSION { get; set; }
        public string _CL_DRIVER_VERSION { get; set; }
        public string _CL_DEVICE_BOARD_NAME_AMD { get; set; }
    }
}

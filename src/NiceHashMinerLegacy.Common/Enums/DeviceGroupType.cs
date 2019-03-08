using System;

namespace NiceHashMinerLegacy.Common.Enums
{
    // TODO remove this in the near future
    //[Obsolete("DeviceGroupType should be removed")]
    public enum DeviceGroupType
    {
        NONE = -1,
        CPU = 0,
        AMD_OpenCL,
        [Obsolete("SM 2.1 support is dropped")]
        NVIDIA_2_1,
        NVIDIA_3_x,
        NVIDIA_5_x,
        NVIDIA_6_x,
        LAST
    }
}

using NiceHashMinerLegacy.Common.Enums;

namespace NiceHashMiner.Devices
{
    public static class GroupNames
    {
        private static readonly string[] Names =
        {
            "CPU", // we can have more then one CPU
            "AMD_OpenCL",
            "NVIDIA2.1",
            "NVIDIA3.x",
            "NVIDIA5.x",
            "NVIDIA6.x",
        };

        public static string GetGroupName(DeviceGroupType type, int id)
        {
            if (DeviceGroupType.CPU == type)
            {
                return "CPU" + id;
            }
            if ((int) type < Names.Length && (int) type >= 0)
            {
                return Names[(int) type];
            }
            return "UnknownGroup";
        }

        public static string GetNameGeneral(DeviceType type)
        {
            switch (type)
            {
                case DeviceType.CPU:
                    return "CPU";
                case DeviceType.NVIDIA:
                    return "NVIDIA";
                case DeviceType.AMD:
                    return "AMD";
            }
            return "UnknownDeviceType";
        }
    }
}

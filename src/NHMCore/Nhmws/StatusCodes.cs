using NHM.Common.Enums;
using System;

namespace NHMCore.Nhmws
{
    internal static class StatusCodes
    {
        private static string deviceTypePrefixPart(DeviceType type)
        {
            switch (type)
            {
                case DeviceType.CPU:
                    return "01";
                case DeviceType.NVIDIA:
                    return "10";
                case DeviceType.AMD:
                    return "11";
            }
            return "00"; // error
        }

        private static string deviceStateSuffixPart(DeviceState state)
        {
            switch (state)
            {
                case DeviceState.Disabled:
                    return "000";
                case DeviceState.Stopped: // inactive
                    return "001";
                case DeviceState.Mining: // active and mining
                    return "010";
                case DeviceState.Benchmarking: // active and benchmarking
                    return "011";
                case DeviceState.Error:
                    return "100";
                case DeviceState.Pending: // recovering or initializing
                    return "101";
            }
            return "111"; // error
        }

        public static int DeviceReportStatus(DeviceType type, DeviceState state)
        {
            var prefix = deviceTypePrefixPart(type);
            var suffix = deviceStateSuffixPart(state);
            var binCode = $"{prefix}{suffix}";
            return Convert.ToInt32(binCode, 2);
        }
    }
}

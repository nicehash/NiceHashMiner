using NHM.Common.Enums;
using System;

namespace NHMCore.Nhmws.V3
{
    internal static class StatusCodes
    {
        private static string deviceTypePrefixPart(DeviceType type) =>
            type switch
            {
                DeviceType.CPU => "01",
                DeviceType.NVIDIA => "10",
                DeviceType.AMD => "11",
                _ => "00", // error
            };

        private static string deviceStateSuffixPart(DeviceState state) =>
            state switch
            {
                DeviceState.Disabled => "000",
                DeviceState.Stopped => "001", // inactive
                DeviceState.Mining => "010", // active and mining
                DeviceState.Benchmarking => "011", // active and benchmarking
                DeviceState.Error => "100",
                DeviceState.Pending => "101", // recovering or initializing
                _ => "111", // error
            };

        public static int DeviceReportStatus(DeviceType type, DeviceState state)
        {
            var prefix = deviceTypePrefixPart(type);
            var suffix = deviceStateSuffixPart(state);
            var binCode = $"{prefix}{suffix}";
            return Convert.ToInt32(binCode, 2);
        }
    }
}

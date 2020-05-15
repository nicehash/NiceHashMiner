using NHM.Common.Enums;
using System.Collections.Generic;
using System.Linq;

namespace NHM.MinerPluginToolkitV1.ClaymoreCommon
{
    public static class ClaymoreHelpers
    {
        public static int GetPlatformIDForType(IEnumerable<DeviceType> types)
        {
            if (types.Contains(DeviceType.AMD) && types.Contains(DeviceType.NVIDIA))
                return 3;

            if (types.Contains(DeviceType.AMD))
                return 1;

            if (types.Contains(DeviceType.NVIDIA))
                return 2;

            return -1;
        }

        private static string deviceIDs => "0123456789abcdefghijkmnopqrstuvwxyz";
        public static char GetClaymoreDeviceID(int index)
        {
            if (index < deviceIDs.Length) return deviceIDs[index];
            return '-'; // invalid 
        }
    }
}

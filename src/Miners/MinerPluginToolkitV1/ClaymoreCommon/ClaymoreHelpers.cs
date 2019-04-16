using NiceHashMinerLegacy.Common.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MinerPluginToolkitV1.ClaymoreCommon
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
    }
}

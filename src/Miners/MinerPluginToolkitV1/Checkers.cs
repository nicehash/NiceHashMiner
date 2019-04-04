using NiceHashMinerLegacy.Common.Device;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MinerPluginToolkitV1
{
    public static class Checkers
    {
        public static bool IsGcn4(AMDDevice dev)
        {
            if (dev.Name.Contains("Vega"))
                return true;
            if (dev.InfSection.ToLower().Contains("polaris"))
                return true;

            return false;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NiceHashMinerLegacy.Common.Algorithm;
using NiceHashMinerLegacy.Common.Device;

namespace NiceHashMiner.Miners.IntegratedPlugins.CCMinerIntegrated
{
    class CCMinerX11GostIntegratedPlugin : CCMinersPluginBase
    {
        public override string PluginUUID => "CCMinerX11Gost";

        public override Version Version => new Version(1,0);

        public override string Name => "CCMinerX11Gost";

        protected override string DirPath => "ccminer_x11gost";

        public override Dictionary<BaseDevice, IReadOnlyList<Algorithm>> GetSupportedAlgorithms(IEnumerable<BaseDevice> devices)
        {
            throw new NotImplementedException();
        }
    }
}

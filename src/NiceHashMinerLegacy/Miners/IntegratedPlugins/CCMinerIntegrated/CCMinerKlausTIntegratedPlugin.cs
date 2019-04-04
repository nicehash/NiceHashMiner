using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NiceHashMinerLegacy.Common.Algorithm;
using NiceHashMinerLegacy.Common.Device;

namespace NiceHashMiner.Miners.IntegratedPlugins.CCMinerIntegrated
{
    class CCMinerKlausTIntegratedPlugin : CCMinersPluginBase
    {
        public override string PluginUUID => "CCMinerKlausT";

        public override Version Version => new Version(1,0);

        public override string Name => "CCMinerKlausT";

        protected override string DirPath => "ccminer_klaust";

        public override Dictionary<BaseDevice, IReadOnlyList<Algorithm>> GetSupportedAlgorithms(IEnumerable<BaseDevice> devices)
        {
            throw new NotImplementedException();
        }
    }
}

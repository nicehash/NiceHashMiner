using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NiceHashMinerLegacy.Common.Algorithm;
using NiceHashMinerLegacy.Common.Device;

namespace NiceHashMiner.Miners.IntegratedPlugins.CCMinerIntegrated
{
    class CCMinerTPruvotIntegratedPlugin : CCMinersPluginBase
    {
        public override string PluginUUID => "CCMinerTPruvot";

        public override Version Version => new Version(1,0);

        public override string Name => "CCMinerTPruvot";

        protected override string DirPath => "ccminer_tpruvot";

        public override Dictionary<BaseDevice, IReadOnlyList<Algorithm>> GetSupportedAlgorithms(IEnumerable<BaseDevice> devices)
        {
            throw new NotImplementedException();
        }
    }
}

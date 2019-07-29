using MinerPluginToolkitV1;
using NHM.Common.Algorithm;
using NHM.Common.Device;
using NHM.Common.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SRBMiner
{
    public class SRBMinerPlugin : PluginBase
    {
        public override Version Version => new Version(1,0);

        public override string Name => "SRBMiner";

        public override string Author => "domen.kirnkrefl@nicehash.com";

        public override string PluginUUID => throw new NotImplementedException();

        public override IEnumerable<string> CheckBinaryPackageMissingFiles()
        {
            throw new NotImplementedException();
        }

        public override Dictionary<BaseDevice, IReadOnlyList<Algorithm>> GetSupportedAlgorithms(IEnumerable<BaseDevice> devices)
        {
            throw new NotImplementedException();
        }

        public override bool ShouldReBenchmarkAlgorithmOnDevice(BaseDevice device, Version benchmarkedPluginVersion, params AlgorithmType[] ids)
        {
            throw new NotImplementedException();
        }

        protected override MinerBase CreateMinerBase()
        {
            return new SRBMiner(PluginUUID);
        }
    }
}

using NHM.Common;
using NHM.Common.Enums;
using NHM.MinerPlugin;
using NHM.MinerPluginToolkitV1;
using NHM.MinerPluginToolkitV1.CCMinerCommon;
using System.Linq;
using System.Threading.Tasks;

namespace ZEnemy
{
    public class ZEnemy : MinerBase
    {
        private string _devices;

        private int _apiPort;

        public ZEnemy(string uuid) : base(uuid)
        { }

        protected virtual string AlgorithmName(AlgorithmType algorithmType) => PluginSupportedAlgorithms.AlgorithmName(algorithmType);

        private double DevFee => PluginSupportedAlgorithms.DevFee(_algorithmType);

        public override Task<ApiData> GetMinerStatsDataAsync()
        {
            return CCMinerAPIHelpers.GetMinerStatsDataAsync(_apiPort, _algorithmType, _miningPairs, _logGroup, DevFee);
        }

        protected override void Init()
        {
            _devices = string.Join(",", _miningPairs.Select(p => p.Device.ID));
        }

        protected override string MiningCreateCommandLine()
        {
            // API port function might be blocking
            _apiPort = GetAvaliablePort();
            // instant non blocking
            var urlWithPort = StratumServiceHelpers.GetLocationUrl(_algorithmType, _miningLocation, NhmConectionType.STRATUM_TCP);

            var algo = AlgorithmName(_algorithmType);

            var commandLine = $"--algo {algo} --url={urlWithPort} --user {_username} --api-bind={_apiPort} --devices {_devices} {_extraLaunchParameters}";
            return commandLine;
        }
    }
}

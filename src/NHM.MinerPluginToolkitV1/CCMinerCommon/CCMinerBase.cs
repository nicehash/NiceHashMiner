using NHM.Common;
using NHM.Common.Enums;
using NHM.MinerPlugin;
using System.Linq;
using System.Threading.Tasks;

namespace NHM.MinerPluginToolkitV1.CCMinerCommon
{
    public abstract class CCMinerBase : MinerBase
    {
        // command line parts
        protected string _devices;
        protected int _apiPort;

        protected bool _noTimeLimitOption = false;

        public CCMinerBase(string uuid) : base(uuid)
        { }

        protected abstract string AlgorithmName(AlgorithmType algorithmType);

        public async override Task<ApiData> GetMinerStatsDataAsync()
        {
            var ret = await CCMinerAPIHelpers.GetMinerStatsDataAsync(_apiPort, _algorithmType, _miningPairs, _logGroup, 0.0);
            return ret;
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
            var url = StratumServiceHelpers.GetLocationUrl(_algorithmType, _miningLocation, NhmConectionType.STRATUM_TCP);
            var algo = AlgorithmName(_algorithmType);

            var commandLine = $"--algo={algo} --url={url} --user={_username} --api-bind={_apiPort} --devices {_devices} {_extraLaunchParameters}";
            return commandLine;
        }
    }
}

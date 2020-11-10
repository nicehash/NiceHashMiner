using NHM.Common;
using NHM.Common.Enums;
using NHM.MinerPlugin;
using NHM.MinerPluginToolkitV1;
using NHM.MinerPluginToolkitV1.ClaymoreCommon;
using NHM.MinerPluginToolkitV1.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TTMiner
{
    public class TTMiner : MinerBase, IAfterStartMining
    {
        private int _apiPort;
        private string _devices;
        private double DevFee => PluginSupportedAlgorithms.DevFee(_algorithmType);
        // figure out how to fix API workaround without this started time
        private DateTime _started;

        protected readonly Dictionary<string, int> _mappedDeviceIds = new Dictionary<string, int>();

        private string AlgoName => PluginSupportedAlgorithms.AlgorithmName(_algorithmType);

        public TTMiner(string uuid, Dictionary<string, int> mappedDeviceIds) : base(uuid)
        {
            _mappedDeviceIds = mappedDeviceIds;
        }

        protected override string MiningCreateCommandLine()
        {
            return CreateCommandLine(_username);
        }

        private string CreateCommandLine(string username)
        {
            _apiPort = GetAvaliablePort();
            var url = StratumServiceHelpers.GetLocationUrl(_algorithmType, _miningLocation, NhmConectionType.STRATUM_TCP);
            var cmd = $"-a {AlgoName} -url {url} -u {username} -d {_devices} --api-bind 127.0.0.1:{_apiPort} {_extraLaunchParameters}";
            return cmd;
        }

        public async override Task<ApiData> GetMinerStatsDataAsync()
        {
            var api = new ApiData();
            var elapsedSeconds = DateTime.Now.Subtract(_started).Seconds;
            if (elapsedSeconds < 15)
            {
                return api;
            }

            var miningDevices = _miningPairs.Select(pair => pair.Device).ToList();
            var algorithmTypes = new AlgorithmType[] { _algorithmType };
            return await ClaymoreAPIHelpers.GetMinerStatsDataAsync(_apiPort, miningDevices, _logGroup, DevFee, 0.0, algorithmTypes);
        }

        protected override IEnumerable<MiningPair> GetSortedMiningPairs(IEnumerable<MiningPair> miningPairs)
        {
            var pairsList = miningPairs.ToList();
            // sort by _mappedDeviceIds
            pairsList.Sort((a, b) => _mappedDeviceIds[a.Device.UUID].CompareTo(_mappedDeviceIds[b.Device.UUID]));
            return pairsList;
        }

        protected override void Init()
        {
            var mappedDevIDs = _miningPairs.Select(p => _mappedDeviceIds[p.Device.UUID]);
            _devices = string.Join(" ", mappedDevIDs);
        }

        public void AfterStartMining()
        {
            _started = DateTime.Now;
        }
    }
}

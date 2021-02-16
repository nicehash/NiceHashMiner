using Newtonsoft.Json;
using NHM.Common;
using NHM.Common.Enums;
using NHM.MinerPlugin;
using NHM.MinerPluginToolkitV1;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace WildRig
{
    public class WildRig : MinerBase
    {
        private int _apiPort;
        private readonly HttpClient _http = new HttpClient();
        private string _devices = "";
        protected readonly Dictionary<string, int> _mappedIDs = new Dictionary<string, int>();

        private string AlgoName => PluginSupportedAlgorithms.AlgorithmName(_algorithmType);

        private double DevFee => PluginSupportedAlgorithms.DevFee(_algorithmType);

        public WildRig(string uuid, Dictionary<string, int> mappedIDs) : base(uuid)
        {
            _mappedIDs = mappedIDs;
        }


        public async override Task<ApiData> GetMinerStatsDataAsync()
        {
            var ad = new ApiData();
            try
            {
                var result = await _http.GetStringAsync($"http://127.0.0.1:{_apiPort}");
                ad.ApiResponse = result;
                var summary = JsonConvert.DeserializeObject<JsonApiResponse>(result);

                var gpus = _miningPairs.Select(pair => pair.Device);
                var perDeviceSpeedInfo = new Dictionary<string, IReadOnlyList<(AlgorithmType type, double speed)>>();
                var perDevicePowerInfo = new Dictionary<string, int>();
                var totalSpeed = 0d;
                var totalPowerUsage = 0;

                var hashrate = summary.hashrate;
                if (hashrate != null)
                {
                    for (int i = 0; i < gpus.Count(); i++)
                    {
                        var deviceSpeed = hashrate.threads.ElementAtOrDefault(i).FirstOrDefault();
                        totalSpeed += deviceSpeed;
                        perDeviceSpeedInfo.Add(gpus.ElementAt(i)?.UUID, new List<(AlgorithmType type, double speed)>() { (_algorithmType, deviceSpeed * (1 - DevFee * 0.01)) });
                    }
                }
                ad.PowerUsageTotal = totalPowerUsage;
                ad.AlgorithmSpeedsPerDevice = perDeviceSpeedInfo;
                ad.PowerUsagePerDevice = perDevicePowerInfo;
            }
            catch (Exception e)
            {
                Logger.Error(_logGroup, $"Error occured while getting API stats: {e.Message}");
            }

            return ad;
        }

        protected override IEnumerable<MiningPair> GetSortedMiningPairs(IEnumerable<MiningPair> miningPairs)
        {
            var pairsList = miningPairs.ToList();
            // sort by mapped ids
            pairsList.Sort((a, b) => _mappedIDs[a.Device.UUID].CompareTo(_mappedIDs[b.Device.UUID]));
            return pairsList;
        }

        protected override void Init()
        {
            _devices = string.Join(",", _miningPairs.Select(p => _mappedIDs[p.Device.UUID]));
        }

        protected override string MiningCreateCommandLine()
        {
            return CreateCommandLine(_username);
        }

        private string CreateCommandLine(string username)
        {
            _apiPort = GetAvaliablePort();
            var url = StratumServiceHelpers.GetLocationUrl(_algorithmType, _miningLocation, NhmConectionType.STRATUM_TCP);
            return $"-a {AlgoName} -o {url} -u {username} --api-port={_apiPort} -d {_devices} --multiple-instance {_extraLaunchParameters}";
        }
    }
}

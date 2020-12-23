using Newtonsoft.Json;
using NHM.Common;
using NHM.Common.Device;
using NHM.Common.Enums;
using NHM.MinerPlugin;
using NHM.MinerPluginToolkitV1;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace SRBMiner
{
    public class SRBMiner : MinerBase
    {
        private int _apiPort;
        private double DevFee = 0.85;
        private string _devices;
        private HttpClient _httpClient;
        protected readonly Dictionary<string, int> _mappedDeviceIds = new Dictionary<string, int>();

        public SRBMiner(string uuid, Dictionary<string, int> mappedDeviceIds) : base(uuid)
        {
            _mappedDeviceIds = mappedDeviceIds;
        }

        private string AlgoName => PluginSupportedAlgorithms.AlgorithmName(_algorithmType);

        public async override Task<ApiData> GetMinerStatsDataAsync()
        {
            // lazy init
            if (_httpClient == null) _httpClient = new HttpClient();
            var ad = new ApiData();
            try
            {
                var result = await _httpClient.GetStringAsync($"http://127.0.0.1:{_apiPort}");
                ad.ApiResponse = result;
                var summary = JsonConvert.DeserializeObject<ApiJsonResponse>(result);

                var gpus = _miningPairs.Select(pair => pair.Device);
                var perDeviceSpeedInfo = new Dictionary<string, IReadOnlyList<(AlgorithmType type, double speed)>>();
                var perDevicePowerInfo = new Dictionary<string, int>();
                var totalSpeed = 0d;
                var totalPowerUsage = 0;

                var amdDevices = gpus.Cast<AMDDevice>();
                foreach (var gpu in amdDevices)
                {
                    var algorithmDevices = summary.algorithms.FirstOrDefault().hashrate.gpu;
                    var deviceName = summary.gpu_devices.Where(dev => dev.bus_id == gpu.PCIeBusID).FirstOrDefault().device;
                    var currentDevStats = algorithmDevices.Where(dev => dev.Key == deviceName).FirstOrDefault().Value;
                    if (currentDevStats == 0) continue;

                    totalSpeed += currentDevStats;
                    perDeviceSpeedInfo.Add(gpu.UUID, new List<(AlgorithmType type, double speed)>() { (_algorithmType, currentDevStats * (1 - DevFee * 0.01)) });
                }
                ad.PowerUsageTotal = totalPowerUsage;
                ad.AlgorithmSpeedsPerDevice = perDeviceSpeedInfo;
                ad.PowerUsagePerDevice = perDevicePowerInfo;
            }
            catch (Exception e)
            {
                Logger.Error(_logGroup, $"Error occured while getting API stats: {e.Message}");
                //CurrentMinerReadStatus = MinerApiReadStatus.NETWORK_EXCEPTION;
            }

            return ad;
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
            _devices = string.Join("!", mappedDevIDs);
        }

        protected override string MiningCreateCommandLine()
        {
            // API port function might be blocking
            _apiPort = GetAvaliablePort();
            var urlWithPort = StratumServiceHelpers.GetLocationUrl(_algorithmType, _miningLocation, NhmConectionType.STRATUM_TCP);
            var cmd = $"--algorithm {AlgoName} --pool {urlWithPort} --wallet {_username} --gpu-id {_devices} --disable-cpu --api-enable --api-port {_apiPort} {_extraLaunchParameters}";
            return cmd;
        }
    }
}

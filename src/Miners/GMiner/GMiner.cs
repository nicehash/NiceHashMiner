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

namespace GMinerPlugin
{
    // NOTE: GMiner will NOT run if the VS debugger is attached to NHML. 
    // Detach the debugger to use GMiner.

    // benchmark is 
    public class GMiner : MinerBase
    {
        protected AlgorithmType _algorithmSecondType = AlgorithmType.NONE;
        private const double DevFee = 2.0;
        private HttpClient _httpClient;
        private int _apiPort;
        // command line parts
        private string _devices;

        protected readonly Dictionary<string, int> _mappedDeviceIds = new Dictionary<string, int>();


        public GMiner(string uuid, Dictionary<string, int> mappedDeviceIds) : base(uuid)
        {
            _mappedDeviceIds = mappedDeviceIds;
        }

        protected virtual string AlgorithmName()
        {
            if (_algorithmSecondType != AlgorithmType.NONE)
            {
                var ret = $"{PluginSupportedAlgorithms.AlgorithmName(_algorithmType)}+{PluginSupportedAlgorithms.AlgorithmName(_algorithmSecondType)}";
                return ret;
            }
            // default single algo
            return PluginSupportedAlgorithms.AlgorithmName(_algorithmType);
        }

        private string CreateCommandLine(string username)
        {
            // API port function might be blocking
            _apiPort = GetAvaliablePort();

            var algo = AlgorithmName();

            var urlWithPort = StratumServiceHelpers.GetLocationUrl(_algorithmType, _miningLocation, NhmConectionType.NONE);
            var cmd = $"-a {algo} --proto stratum --server {urlWithPort} -u {username} -d {_devices} -w 0 --api {_apiPort} {_extraLaunchParameters}";
            if (_algorithmSecondType != AlgorithmType.NONE)
            {
                var urlWithPort2 = StratumServiceHelpers.GetLocationUrl(_algorithmSecondType, _miningLocation, NhmConectionType.NONE);
                // --algo eth+ckb --server eth.2miners.com:2020 --user 0x5218597d48333d4a70cce91e810007b37e2937b5 --dserver ckb.2miners.com:6464 --duser ckb1qyq9v9yc2pmauycldz4e4ejuxdmvph0xpazq4nh3ph
                cmd = $"-a {algo} --proto stratum --server {urlWithPort} -u {username} --proto stratum --dserver {urlWithPort2} --duser {username} -d {_devices} -w 0 --api {_apiPort} {_extraLaunchParameters}";
            }

            if (_algorithmType == AlgorithmType.ZHash)
            {
                cmd += " --pers auto";
            }

            return cmd;
        }

        public async override Task<ApiData> GetMinerStatsDataAsync()
        {
            // lazy init
            if (_httpClient == null) _httpClient = new HttpClient();
            var ad = new ApiData();
            try
            {
                var result = await _httpClient.GetStringAsync($"http://127.0.0.1:{_apiPort}/stat");
                ad.ApiResponse = result;
                var summary = JsonConvert.DeserializeObject<JsonApiResponse>(result);

                var gpus = _miningPairs.Select(pair => pair.Device);
                var perDeviceSpeedInfo = new Dictionary<string, IReadOnlyList<(AlgorithmType type, double speed)>>();
                var perDevicePowerInfo = new Dictionary<string, int>();
                var totalSpeed = 0d;
                var totalSpeed2 = 0d;
                var totalPowerUsage = 0;
                foreach (var gpu in gpus)
                {
                    var currentDevStats = summary.devices.Where(devStats => devStats.gpu_id == _mappedDeviceIds[gpu.UUID]).FirstOrDefault();
                    if (currentDevStats == null) continue;
                    totalSpeed += currentDevStats.speed;
                    totalSpeed2 += currentDevStats.speed2;
                    if (_algorithmSecondType == AlgorithmType.NONE)
                    {
                        perDeviceSpeedInfo.Add(gpu.UUID, new List<(AlgorithmType type, double speed)>() { (_algorithmType, currentDevStats.speed * (1 - DevFee * 0.01)) });
                    }
                    else
                    {
                        // only one dual algo here
                        perDeviceSpeedInfo.Add(gpu.UUID, new List<(AlgorithmType type, double speed)>() {
                            (_algorithmType, currentDevStats.speed * (1 - 3.0 * 0.01)),
                            (_algorithmSecondType, currentDevStats.speed2 * (1 - DevFee * 0.01))
                        });
                    }
                    var kPower = currentDevStats.power_usage * 1000;
                    totalPowerUsage += kPower;
                    perDevicePowerInfo.Add(gpu.UUID, kPower);
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
            _devices = string.Join(" ", mappedDevIDs);

            var dualType = MinerToolkit.GetAlgorithmDualType(_miningPairs);
            _algorithmSecondType = dualType.Item1;
            var ok = dualType.Item2;
            if (!ok) _algorithmSecondType = AlgorithmType.NONE;
        }

        protected override string MiningCreateCommandLine()
        {
            return CreateCommandLine(_username);
        }
    }
}

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


namespace TRex
{
    public class TRex : MinerBase
    {
        private string _devices;

        private int _apiPort;

        private readonly HttpClient _httpClient = new HttpClient();

        public TRex(string uuid) : base(uuid)
        { }

        protected virtual string AlgorithmName(AlgorithmType algorithmType) => PluginSupportedAlgorithms.AlgorithmName(algorithmType);

        private double DevFee => PluginSupportedAlgorithms.DevFee(_algorithmType);

        public async override Task<ApiData> GetMinerStatsDataAsync()
        {
            var ad = new ApiData();
            try
            {
                var summaryApiResult = await _httpClient.GetStringAsync($"http://127.0.0.1:{_apiPort}/summary");
                ad.ApiResponse = summaryApiResult;
                var summary = JsonConvert.DeserializeObject<JsonApiResponse>(summaryApiResult);

                var gpuDevices = _miningPairs.Select(pair => pair.Device);
                var perDeviceSpeedInfo = new Dictionary<string, IReadOnlyList<(AlgorithmType type, double speed)>>();
                var perDevicePowerInfo = new Dictionary<string, int>();
                var totalSpeed = summary.hashrate;
                var totalPowerUsage = 0.0;

                foreach (var gpuDevice in gpuDevices)
                {
                    var currentStats = summary.gpus.Where(devStats => devStats.gpu_id == gpuDevice.ID).FirstOrDefault();
                    if (currentStats == null) continue;
                    perDeviceSpeedInfo.Add(gpuDevice.UUID, new List<(AlgorithmType type, double speed)>() { (_algorithmType, currentStats.hashrate * (1 - DevFee * 0.01)) });
                    var kPower = currentStats.power * 1000;
                    totalPowerUsage += kPower;
                    perDevicePowerInfo.Add(gpuDevice.UUID, kPower);
                }
                ad.PowerUsageTotal = Convert.ToInt32(totalPowerUsage);
                ad.AlgorithmSpeedsPerDevice = perDeviceSpeedInfo;
                ad.PowerUsagePerDevice = perDevicePowerInfo;

            }
            catch (Exception e)
            {
                Logger.Error(_logGroup, $"Error occured while getting API stats: {e.Message}");
            }

            return ad;
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
            if (_algorithmType == AlgorithmType.DaggerHashimoto)
            {
                url = $"stratum2+tcp://{StratumServiceHelpers.GetLocationUrl(_algorithmType, _miningLocation, NhmConectionType.NONE)}";
            }
            var algo = AlgorithmName(_algorithmType);

            var commandLine = $"--algo {algo} --url {url} --user {_username} --api-bind-http 127.0.0.1:{_apiPort} --api-bind-telnet 0 --devices {_devices} {_extraLaunchParameters} --no-watchdog";
            return commandLine;
        }
    }
}


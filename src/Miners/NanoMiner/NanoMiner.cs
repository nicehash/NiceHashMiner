using Newtonsoft.Json;
using NHM.Common;
using NHM.Common.Enums;
using NHM.MinerPlugin;
using NHM.MinerPluginToolkitV1;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace NanoMiner
{
    public class NanoMiner : MinerBase
    {

        private readonly HttpClient _http = new HttpClient();
        private int _apiPort;

        protected readonly Dictionary<string, int> _mappedIDs = new Dictionary<string, int>();

        public NanoMiner(string uuid, Dictionary<string, int> mappedIDs) : base(uuid)
        {
            _mappedIDs = mappedIDs;
        }

        protected virtual string AlgorithmName(AlgorithmType algorithmType) => PluginSupportedAlgorithms.AlgorithmName(algorithmType);
        private double DevFee => PluginSupportedAlgorithms.DevFee(_algorithmType);

        protected override IEnumerable<MiningPair> GetSortedMiningPairs(IEnumerable<MiningPair> miningPairs)
        {
            var pairsList = miningPairs.ToList();
            // sort by mapped ids
            pairsList.Sort((a, b) => _mappedIDs[a.Device.UUID].CompareTo(_mappedIDs[b.Device.UUID]));
            return pairsList;
        }

        protected override void Init()
        {
            // ?????
        }

        public async override Task<ApiData> GetMinerStatsDataAsync()
        {
            var api = new ApiData();
            try
            {
                var result = await _http.GetStringAsync($"http://127.0.0.1:{_apiPort}/stats");
                api.ApiResponse = result;
                var apiResponse = JsonConvert.DeserializeObject<JsonApiResponse>(result);
                var parsedApiResponse = JsonApiHelpers.ParseJsonApiResponse(apiResponse, _mappedIDs);

                var perDeviceSpeedInfo = new Dictionary<string, IReadOnlyList<(AlgorithmType type, double speed)>>();
                var perDevicePowerInfo = new Dictionary<string, int>();
                var totalSpeed = 0d;
                var totalPowerUsage = 0;

                foreach (var miningPair in _miningPairs)
                {
                    var deviceUUID = miningPair.Device.UUID;
                    if (parsedApiResponse.ContainsKey(deviceUUID))
                    {
                        var stat = parsedApiResponse[deviceUUID];
                        var currentPower = (int)stat.Power;
                        totalPowerUsage += currentPower;
                        var hashrate = stat.Hashrate * (1 - DevFee * 0.01);
                        totalSpeed += hashrate;
                        perDeviceSpeedInfo.Add(deviceUUID, new List<(AlgorithmType type, double speed)>() { (_algorithmType, hashrate) });
                        perDevicePowerInfo.Add(deviceUUID, currentPower);
                    }
                    else
                    {
                        perDeviceSpeedInfo.Add(deviceUUID, new List<(AlgorithmType type, double speed)>() { (_algorithmType, 0) });
                        perDevicePowerInfo.Add(deviceUUID, 0);
                    }
                }

                api.AlgorithmSpeedsPerDevice = perDeviceSpeedInfo;
                api.PowerUsagePerDevice = perDevicePowerInfo;
                api.PowerUsageTotal = totalPowerUsage;
            }
            catch (Exception e)
            {
                Logger.Error(_logGroup, $"Error occured while getting API stats: {e.Message}");
            }

            return api;
        }

        protected override string MiningCreateCommandLine()
        {
            return CreateCommandLine(_username);
        }

        private string CreateCommandLine(string username)
        {
            _apiPort = GetAvaliablePort();

            var algo = AlgorithmName(_algorithmType);

            var url = StratumServiceHelpers.GetLocationUrl(_algorithmType, _miningLocation, NhmConectionType.NONE);
            var paths = GetBinAndCwdPaths();

            var configString = "";
            if (_extraLaunchParameters != "")
            {
                var arrayOfELP = _extraLaunchParameters.Split(' ');
                foreach (var elp in arrayOfELP)
                {
                    configString += $"{elp}\r\n";
                }
            }

            var devs = string.Join(",", _miningPairs.Select(p => _mappedIDs[p.Device.UUID]));

            configString += $"webPort={_apiPort}\r\nwatchdog=false\n\r\n\r[{algo}]\r\nwallet={username}\r\nrigName=\r\ndevices={devs}\r\npool1={url}";
            try
            {
                File.WriteAllText(Path.Combine(paths.Item2, $"config_nh_{devs}.ini"), configString);
            }
            catch (Exception e)
            {
                Logger.Error(_logGroup, $"Unable to create config file: {e.Message}");
            }
            return $"config_nh_{devs}.ini";
        }
    }
}

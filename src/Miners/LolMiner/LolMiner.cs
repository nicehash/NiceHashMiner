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

namespace LolMiner
{
    public class LolMiner : MinerBase
    {
        private string _devices;
        private int _apiPort;
        private double DevFee = 1d;

        private string _disableWatchdogParam = "--disablewatchdog 1";

        // the order of intializing devices is the order how the API responds
        private Dictionary<int, string> _initOrderMirrorApiOrderUUIDs = new Dictionary<int, string>();
        protected Dictionary<string, int> _mappedIDs;

        private readonly HttpClient _http = new HttpClient();

        public LolMiner(string uuid, Dictionary<string, int> mappedIDs) : base(uuid)
        {
            _mappedIDs = mappedIDs;
        }

        protected virtual string AlgorithmName(AlgorithmType algorithmType) => PluginSupportedAlgorithms.AlgorithmName(algorithmType);

        public async override Task<ApiData> GetMinerStatsDataAsync()
        {
            var ad = new ApiData();
            try
            {
                var summaryApiResult = await _http.GetStringAsync($"http://127.0.0.1:{_apiPort}/summary");
                ad.ApiResponse = summaryApiResult;
                var summary = JsonConvert.DeserializeObject<ApiJsonResponse>(summaryApiResult);
                var perDeviceSpeedInfo = new Dictionary<string, IReadOnlyList<(AlgorithmType type, double speed)>>();
                var speedUnit = summary.Session.Performance_Unit;
                var multiplier = 1;
                switch (speedUnit)
                {
                    case "mh/s":
                        multiplier = 1000000; //1M
                        break;
                    case "kh/s":
                        multiplier = 1000; //1k
                        break;
                    default:
                        break;
                }
                var totalSpeed = summary.Session.Performance_Summary * multiplier;

                var totalPowerUsage = 0;
                var perDevicePowerInfo = new Dictionary<string, int>();

                var apiDevices = summary.GPUs;

                foreach (var pair in _miningPairs)
                {
                    var gpuUUID = pair.Device.UUID;
                    var gpuID = _mappedIDs[gpuUUID];
                    var currentStats = summary.GPUs.Where(devStats => devStats.Index == gpuID).FirstOrDefault();
                    if (currentStats == null) continue;
                    perDeviceSpeedInfo.Add(gpuUUID, new List<(AlgorithmType type, double speed)>() { (_algorithmType, currentStats.Performance * multiplier * (1 - DevFee * 0.01)) });
                }

                ad.AlgorithmSpeedsPerDevice = perDeviceSpeedInfo;
                ad.PowerUsageTotal = totalPowerUsage;
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

            // ???????? GetSortedMiningPairs is now sorted so this thing probably makes no sense anymore
            var miningPairs = _miningPairs.ToList();
            for (int i = 0; i < miningPairs.Count; i++)
            {
                _initOrderMirrorApiOrderUUIDs[i] = miningPairs[i].Device.UUID;
            }
            // if ELP contains watchdog remove default wd-param
            if (_extraLaunchParameters.Contains("--disablewatchdog"))
            {
                _disableWatchdogParam = "";
            }
        }

        protected override string MiningCreateCommandLine()
        {
            // API port function might be blocking
            _apiPort = GetAvaliablePort();
            // instant non blocking
            var urlWithPort = StratumServiceHelpers.GetLocationUrl(_algorithmType, _miningLocation, NhmConectionType.NONE);

            var algo = AlgorithmName(_algorithmType);
            var commandLine = $"--pool {urlWithPort} --user {_username} --tls 0 --apiport {_apiPort} {_disableWatchdogParam} --devices {_devices} {_extraLaunchParameters}";

            if (_algorithmType == AlgorithmType.ZHash) commandLine += " --coin AUTO144_5";
            else commandLine += $" --algo {algo}";
            //--disablewatchdog 1
            return commandLine;
        }
    }
}

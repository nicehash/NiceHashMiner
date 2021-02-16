using NHM.Common;
using NHM.Common.Enums;
using NHM.MinerPlugin;
using NHM.MinerPluginToolkitV1;
using NHM.MinerPluginToolkitV1.ClaymoreCommon;
using NHM.MinerPluginToolkitV1.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Phoenix
{
    public class Phoenix : MinerBase, IBeforeStartMining
    {
        private const double DevFee = 0.65;

        private int _apiPort;
        private string _devices;
        protected readonly Dictionary<string, int> _mappedDeviceIds = new Dictionary<string, int>();

        public Phoenix(string uuid, Dictionary<string, int> mappedIDs) : base(uuid)
        {
            _mappedDeviceIds = mappedIDs;
        }

        public string CreateCommandLine(string username)
        {
            var urlWithPort = StratumServiceHelpers.GetLocationUrl(_algorithmType, _miningLocation, NhmConectionType.STRATUM_TCP);
            var deviceType = _miningPairs.FirstOrDefault().Device.DeviceType == DeviceType.AMD ? " -amd" : " -nvidia";
            var cmd = $"-pool {urlWithPort} -wal {username} -proto 4 {deviceType} -gpus {_devices} -wdog 0 -gbase 0 {_extraLaunchParameters}";

            if (!_extraLaunchParameters.Contains("-stales"))
            {
                cmd += " -stales 0";
            }

            return cmd;
        }

        public async override Task<ApiData> GetMinerStatsDataAsync()
        {
            var miningDevices = _miningPairs.Select(pair => pair.Device).ToList();
            var algorithmTypes = new AlgorithmType[] { _algorithmType };
            // multiply dagger API data 
            var ad = await ClaymoreAPIHelpers.GetMinerStatsDataAsync(_apiPort, miningDevices, _logGroup, DevFee, 0.0, algorithmTypes);
            if (ad.AlgorithmSpeedsPerDevice != null)
            {
                // speed is in khs
                ad.AlgorithmSpeedsPerDevice = ad.AlgorithmSpeedsPerDevice.Select(pair => new KeyValuePair<string, IReadOnlyList<(AlgorithmType type, double speed)>>(pair.Key, pair.Value.Select((ts) => (ts.type, ts.speed * 1000)).ToList())).ToDictionary(x => x.Key, x => x.Value);
            }
            return ad;
        }

        private static HashSet<string> _deleteConfigs = new HashSet<string> { "config.txt", "dpools.txt", "epools.txt" };
        private static bool IsDeleteConfigFile(string file)
        {
            foreach (var conf in _deleteConfigs)
            {
                if (file.Contains(conf)) return true;
            }
            return false;
        }

        public void BeforeStartMining()
        {
            var binCwd = GetBinAndCwdPaths().Item2;
            var txtFiles = Directory.GetFiles(binCwd, "*.txt", SearchOption.AllDirectories)
                .Where(file => IsDeleteConfigFile(file))
                .ToArray();
            foreach (var deleteFile in txtFiles)
            {
                try
                {
                    File.Delete(deleteFile);
                }
                catch (Exception e)
                {
                    Logger.Error(_logGroup, $"BeforeStartMining error while deleting file '{deleteFile}': {e.Message}");
                }
            }
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
            _devices = string.Join(",", mappedDevIDs);
        }

        protected override string MiningCreateCommandLine()
        {
            _apiPort = GetAvaliablePort();
            return CreateCommandLine(_username) + $" -mport 127.0.0.1:-{_apiPort}";
        }
    }
}

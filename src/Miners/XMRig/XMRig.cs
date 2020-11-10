using Newtonsoft.Json;
using NHM.Common;
using NHM.Common.Enums;
using NHM.MinerPlugin;
using NHM.MinerPluginToolkitV1;
using NHM.MinerPluginToolkitV1.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace XMRig
{
    public class XMRig : MinerBase, IBeforeStartMining
    {
        // TODO DevFee 
        private double DevFee = 1.0;
        private int _apiPort;
        protected readonly HttpClient _httpClient = new HttpClient();

        private string AlgoName
        {
            get
            {
                return PluginSupportedAlgorithms.AlgorithmName(_algorithmType);
            }
        }

        public XMRig(string uuid) : base(uuid) { }

        public async override Task<ApiData> GetMinerStatsDataAsync()
        {
            var api = new ApiData();
            try
            {
                var result = await _httpClient.GetStringAsync($"http://127.0.0.1:{_apiPort}/1/summary");
                api.ApiResponse = result;
                var summary = JsonConvert.DeserializeObject<JsonApiResponse>(result);

                var totalSpeed = 0d;
                var perDeviceSpeedInfo = new Dictionary<string, IReadOnlyList<(AlgorithmType type, double speed)>>();
                var perDevicePowerInfo = new Dictionary<string, int>();
                // init per device sums
                foreach (var pair in _miningPairs)
                {
                    var uuid = pair.Device.UUID;
                    var currentSpeed = summary.hashrate.total.FirstOrDefault() ?? 0d;
                    totalSpeed += currentSpeed;
                    perDeviceSpeedInfo.Add(uuid, new List<(AlgorithmType type, double speed)>() { (_algorithmType, currentSpeed * (1 - DevFee * 0.01)) });
                    // no power usage info
                    perDevicePowerInfo.Add(uuid, -1);
                }

                api.AlgorithmSpeedsPerDevice = perDeviceSpeedInfo;
                api.PowerUsagePerDevice = perDevicePowerInfo;
                api.PowerUsageTotal = -1;
            }
            catch (Exception e)
            {
                Logger.Error(_logGroup, $"Error occured while getting API stats: {e.Message}");
            }

            return api;
        }

        protected override void Init()
        {
            if (_extraLaunchParameters.Contains("--donate-level="))
            {
                var splittedELP = _extraLaunchParameters.Split(' ');
                try
                {
                    foreach (var elp in splittedELP)
                    {
                        if (elp.Contains("--donate-level="))
                        {
                            var parsedDevFee = elp.Split('=')[1];
                            double.TryParse(parsedDevFee, out var devFee);
                            DevFee = devFee;
                        }
                    }
                }
                catch (Exception e)
                {
                    Logger.Error(_logGroup, $"Init failed: {e.Message}");
                }
            }
            else
            {
                _extraLaunchParameters += " --donate-level=1";
            }
        }

        private string CreateCommandLine(string username)
        {
            _apiPort = GetAvaliablePort();
            var urlWithPort = StratumServiceHelpers.GetLocationUrl(_algorithmType, _miningLocation, NhmConectionType.NONE);
            var cmd = "";
            //if user wants to manually tweek with config file we let him do that - WARNING some functionalities might not work (benchmarking, api data)
            if (_extraLaunchParameters.Contains("--config="))
            {
                cmd = _extraLaunchParameters;
            }
            else
            {
                cmd = $"-a {AlgoName} -o {urlWithPort} -u {username} --http-enabled --http-port={_apiPort} --nicehash {_extraLaunchParameters}";
            }
            Logger.Info("STARTED", $"command: {cmd}");
            return cmd;
        }

        protected override string MiningCreateCommandLine()
        {
            return CreateCommandLine(_username);
        }


        private static HashSet<string> _deleteConfigs = new HashSet<string> { "config.json" };
        private static bool IsDeleteConfigFile(string file)
        {
            foreach (var conf in _deleteConfigs)
            {
                if (file.Contains(conf)) return true;
            }
            return false;
        }
        void IBeforeStartMining.BeforeStartMining()
        {
            //if user wants to manually tweek with config file we let him do that - WARNING some functionalities might not work (benchmarking, api data)
            if (_extraLaunchParameters.Contains("--config="))
            {
                return;
            }
            var binCwd = GetBinAndCwdPaths().Item2;
            var txtFiles = Directory.GetFiles(binCwd, "*.json", SearchOption.AllDirectories)
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
    }
}

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MinerPlugin;
using MinerPlugin.Toolkit;
using NiceHashMinerLegacy.Common.Enums;
using static NiceHashMinerLegacy.Common.StratumServiceHelpers;
using static MinerPlugin.Toolkit.MinersApiPortsManager;

namespace ZEnemy
{
    public class ZEnemyBase : MinerBase
    {
        private AlgorithmType _algorithmType;

        private string _devices;
        private string _extraLaunchParameters = "";
        private int _apiPort;
        private ApiDataHelper apiReader = new ApiDataHelper(); // consider replacing with HttpClient

        protected virtual string AlgorithmName(AlgorithmType algorithmType)
        {
            switch (algorithmType)
            {
                case AlgorithmType.X16R: return "x16r";
                case AlgorithmType.Skunk: return "skunk";
            }
            return "";
        }

        public async override Task<ApiData> GetMinerStatsDataAsync()
        {
            var summaryApiResult = await apiReader.GetApiDataAsync(_apiPort, ApiDataHelper.GetHttpRequestNhmAgentStrin("summary"));
            double totalSpeed = 0;
            int totalPower = 0;
            if (!string.IsNullOrEmpty(summaryApiResult))
            {
                try
                {
                    var summaryOptvals = summaryApiResult.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
                    foreach (var optvalPairs in summaryOptvals)
                    {
                        var pair = optvalPairs.Split(new char[] { '=' }, StringSplitOptions.RemoveEmptyEntries);
                        if (pair.Length != 2) continue;
                        if (pair[0] == "KHS")
                        {
                            totalSpeed = double.Parse(pair[1], CultureInfo.InvariantCulture) * 1000; // HPS
                        }
                    }
                }
                catch
                { }
            }
            // TODO if have multiple GPUs call the threads as well, but maybe not as often since it might crash the miner
            //var threadsApiResult = await _httpClient.GetStringAsync($"{localhost}/threads");
            var threadsApiResult = await apiReader.GetApiDataAsync(_apiPort, ApiDataHelper.GetHttpRequestNhmAgentStrin("threads"));
            var perDeviceSpeedInfo = new List<(string uuid, IReadOnlyList<(AlgorithmType, double)>)>();
            var perDevicePowerInfo = new List<(string, int)>();
            if (!string.IsNullOrEmpty(threadsApiResult))
            {
                // TODO return empty
                try
                {
                    var gpus = threadsApiResult.Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
                    foreach (var gpu in gpus)
                    {
                        var gpuOptvalPairs = gpu.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
                        var gpuData = (id: -1, power: -1, speed: -1d);
                        foreach (var optvalPairs in gpuOptvalPairs)
                        {
                            var optval = optvalPairs.Split(new char[] { '=' }, StringSplitOptions.RemoveEmptyEntries);
                            if (optval.Length != 2) continue;
                            if (optval[0] == "GPU")
                            {
                                gpuData.id = int.Parse(optval[1], CultureInfo.InvariantCulture);
                            }
                            if (optval[0] == "POWER")
                            {
                                gpuData.power = int.Parse(optval[1], CultureInfo.InvariantCulture);
                            }
                            if (optval[0] == "KHS")
                            {
                                gpuData.speed = double.Parse(optval[1], CultureInfo.InvariantCulture) * 1000; // HPS
                            }
                        }
                        // TODO do stuff with it gpuData
                        var device = _miningPairs.Where(kvp => kvp.device.ID == gpuData.id).Select(kvp => kvp.device).FirstOrDefault();
                        if (device != null)
                        {
                            perDeviceSpeedInfo.Add((device.UUID, new List<(AlgorithmType, double)>() { (_algorithmType, gpuData.speed) }));
                            perDevicePowerInfo.Add((device.UUID, gpuData.power));
                            totalPower += gpuData.power;
                        }

                    }
                }
                catch
                { }
            }
            var ad = new ApiData();
            var total = new List<(AlgorithmType, double)>();
            total.Add((_algorithmType, totalSpeed));
            ad.AlgorithmSpeedsTotal = total;
            ad.PowerUsageTotal = totalPower;
            ad.AlgorithmSpeedsPerDevice = perDeviceSpeedInfo;
            ad.PowerUsagePerDevice = perDevicePowerInfo;

            return ad;
        }

        public async override Task<(double speed, bool ok, string msg)> StartBenchmark(CancellationToken stop, BenchmarkPerformanceType benchmarkType = BenchmarkPerformanceType.Standard)
        {
            var avgRet = 0.0;
            var counter = 0;
            var maxCheck = 0;
            var after = "Diff:";

            // determine benchmark time 
            // settup times
            var benchmarkTime = 20; // in seconds
            switch (benchmarkType)
            {
                case BenchmarkPerformanceType.Quick:
                    benchmarkTime = 20;
                    maxCheck = 1;
                    break;
                case BenchmarkPerformanceType.Standard:
                    benchmarkTime = 60;
                    maxCheck = 2;
                    break;
                case BenchmarkPerformanceType.Precise:
                    benchmarkTime = 120;
                    maxCheck = 3;
                    break;
            }

            var urlWithPort = GetLocationUrl(_algorithmType, _miningLocation, NhmConectionType.STRATUM_TCP);
            var split = urlWithPort.Split(':');
            var url = split[1].Substring(2, split[1].Length - 2);
            var port = split[2];
            var algo = AlgorithmName(_algorithmType);

            var commandLine = $"--algo {algo} --url={url}:{port} --user 38QriJ13vEBWUPhJrEKKZwKdVftV9dtW25";
            var (binPath, binCwd) = GetBinAndCwdPaths();
            var bp = new BenchmarkProcess(binPath, binCwd, commandLine);

            bp.CheckData = (string data) =>
            {
                var s = data;
                Console.WriteLine(s);
                var ret = (hashrate: default(double), found: false);

                if (s.Contains("Uptime:"))
                {
                    maxCheck--;
                    if(maxCheck == 0)
                    {
                        ret.hashrate = avgRet / counter;
                        ret.found = true;
                    }
                } else
                {
                    if (!s.Contains(after))
                    {
                        return ret;
                    }
                    var afterString = s.GetStringAfter(after).ToLower();
                    var afterStringArray = afterString.Split(',');
                    var hashRate = afterStringArray[1];
                    var numString = new string(hashRate
                        .ToCharArray()
                        .SkipWhile(c => !char.IsDigit(c))
                        .TakeWhile(c => char.IsDigit(c) || c == '.')
                        .ToArray());

                    if (!double.TryParse(numString, NumberStyles.Float, CultureInfo.InvariantCulture, out var hash))
                    {
                        return ret;
                    }

                    counter++;
                    if (hashRate.Contains("kh"))
                        avgRet += hash * 1000;
                    else if (hashRate.Contains("mh"))
                        avgRet += hash * 1000000;
                    else if (hashRate.Contains("gh"))
                        avgRet += hash * 1000000000;
                    else
                        avgRet += hash;

                }

                return ret;
            };

            var benchmarkTimeout = TimeSpan.FromSeconds(300);
            var benchmarkWait = TimeSpan.FromMilliseconds(500);
            var t = MinerToolkit.WaitBenchmarkResult(bp, benchmarkTimeout, benchmarkWait, stop);
            return await t;
        }

        protected override (string binPath, string binCwd) GetBinAndCwdPaths()
        {
            var binPath = @"C:\Users\Domen\Desktop\nhml\bin_3rdparty\Z-ENEMY\z-enemy.exe";
            var binCwd = @"C:\Users\Domen\Desktop\nhml\bin_3rdparty\Z-ENEMY";
            return (binPath, binCwd);
        }

        protected override void Init()
        {
            bool ok;
            (_algorithmType, ok) = MinerToolkit.GetAlgorithmSingleType(_miningPairs);
            if (!ok) throw new InvalidOperationException("Invalid mining initialization");
            // all good continue on

            // init command line params parts
            var deviceIds = MinerToolkit.GetDevicesIDsInOrder(_miningPairs);
            _devices = $"--devices {string.Join(",", deviceIds)}";
            // TODO implement this later
            //_extraLaunchParameters;
        }

        protected override string MiningCreateCommandLine()
        {
            // API port function might be blocking
            _apiPort = GetAvaliablePortInRange(); // use the default range
            // instant non blocking
            var urlWithPort = GetLocationUrl(_algorithmType, _miningLocation, NhmConectionType.STRATUM_TCP);
            var split = urlWithPort.Split(':');
            var url = split[1].Substring(2, split[1].Length - 2);
            var port = split[2];

            var algo = AlgorithmName(_algorithmType);

            var commandLine = $"--algo {algo} --url={url}:{port} --user {_username} --api-bind={_apiPort} {_extraLaunchParameters}";
            return commandLine;
        }
    }
}

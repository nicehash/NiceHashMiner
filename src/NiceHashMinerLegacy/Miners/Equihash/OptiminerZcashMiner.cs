using Newtonsoft.Json;
using NiceHashMiner.Devices;
using NiceHashMiner.Miners.Grouping;
using NiceHashMiner.Miners.Parsing;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using NiceHashMiner.Algorithms;
using NiceHashMinerLegacy.Common.Enums;

namespace NiceHashMiner.Miners.Equihash
{
    public class OptiminerZcashMiner : Miner
    {
        public OptiminerZcashMiner()
            : base("OptiminerZcashMiner")
        {
            ConectionType = NhmConectionType.NONE;
        }

#pragma warning disable
        private class Stratum
        {
            public string target { get; set; }
            public bool connected { get; set; }
            public int connection_failures { get; set; }
            public string host { get; set; }
            public int port { get; set; }
        }

        private class JsonApiResponse
        {
            public double uptime;
            public Dictionary<string, Dictionary<string, double>> solution_rate;
            public Dictionary<string, double> share;
            public Dictionary<string, Dictionary<string, double>> iteration_rate;
            public Stratum stratum;
        }
#pragma warning restore

        // give some time or else it will crash
        private Stopwatch _startApi = null;

        private bool _skipApiCheck = true;
        private readonly int _waitSeconds = 30;

        public override void Start(string url, string btcAdress, string worker)
        {
            var username = GetUsername(btcAdress, worker);
            LastCommandLine = " " + GetDevicesCommandString() + " -m " + ApiPort + " -s " + url + " -u " + username +
                              " -p x";
            ProcessHandle = _Start();

            _startApi = new Stopwatch();
            _startApi.Start();
        }

        protected override void _Stop(MinerStopType willswitch)
        {
            Stop_cpu_ccminer_sgminer_nheqminer(willswitch);
        }

        protected override int GetMaxCooldownTimeInMilliseconds()
        {
            return 60 * 1000 * 5; // 5 minute max, whole waiting time 75seconds
        }

        protected override string GetDevicesCommandString()
        {
            var extraParams = ExtraLaunchParametersParser.ParseForMiningSetup(MiningSetup, DeviceType.AMD);
            var deviceStringCommand = " -c " + ComputeDeviceManager.Available.AmdOpenCLPlatformNum;
            deviceStringCommand += " ";
            var ids = MiningSetup.MiningPairs.Select(mPair => "-d " + mPair.Device.ID.ToString()).ToList();
            deviceStringCommand += string.Join(" ", ids);

            return deviceStringCommand + extraParams;
        }

        public override async Task<ApiData> GetSummaryAsync()
        {
            CurrentMinerReadStatus = MinerApiReadStatus.NONE;
            var ad = new ApiData(MiningSetup.CurrentAlgorithmType);

            if (_skipApiCheck == false)
            {
                JsonApiResponse resp = null;
                try
                {
                    var dataToSend = GetHttpRequestNhmAgentStrin("");
                    var respStr = await GetApiDataAsync(ApiPort, dataToSend, true);
                    if (respStr != null && respStr.Contains("{"))
                    {
                        var start = respStr.IndexOf("{");
                        if (start > -1)
                        {
                            var respStrJson = respStr.Substring(start);
                            resp = JsonConvert.DeserializeObject<JsonApiResponse>(respStrJson.Trim(),
                                Globals.JsonSettings);
                        }
                    }
                    //Helpers.ConsolePrint("OptiminerZcashMiner API back:", respStr);
                }
                catch (Exception ex)
                {
                    Helpers.ConsolePrint("OptiminerZcashMiner", "GetSummary exception: " + ex.Message);
                }

                if (resp?.solution_rate != null)
                {
                    //Helpers.ConsolePrint("OptiminerZcashMiner API back:", "resp != null && resp.error == null");
                    const string totalKey = "Total";
                    const string _5SKey = "5s";
                    if (resp.solution_rate.ContainsKey(totalKey))
                    {
                        var totalSolutionRateDict = resp.solution_rate[totalKey];
                        if (totalSolutionRateDict != null && totalSolutionRateDict.ContainsKey(_5SKey))
                        {
                            ad.Speed = totalSolutionRateDict[_5SKey];
                            CurrentMinerReadStatus = MinerApiReadStatus.GOT_READ;
                        }
                    }
                    if (ad.Speed == 0)
                    {
                        CurrentMinerReadStatus = MinerApiReadStatus.READ_SPEED_ZERO;
                    }
                }
            }
            else if (_skipApiCheck && _startApi.Elapsed.TotalSeconds > _waitSeconds)
            {
                _startApi.Stop();
                _skipApiCheck = false;
            }

            return ad;
        }

        // benchmark stuff

        protected override string BenchmarkCreateCommandLine(Algorithm algorithm, int time)
        {
            var t = time / 9; // sgminer needs 9 times more than this miner so reduce benchmark speed
            var ret = " " + GetDevicesCommandString() + " --benchmark " + t;
            return ret;
        }

        protected override void BenchmarkOutputErrorDataReceivedImpl(string outdata)
        {
            CheckOutdata(outdata);
        }

        protected override bool BenchmarkParseLine(string outdata)
        {
            const string find = "Benchmark:";
            if (outdata.Contains(find))
            {
                var start = outdata.IndexOf("Benchmark:") + find.Length;
                var itersAndVars = outdata.Substring(start).Trim();
                var ar = itersAndVars.Split(' ');
                if (ar.Length >= 4)
                {
                    // gets sols/s
                    BenchmarkAlgorithm.BenchmarkSpeed = Helpers.ParseDouble(ar[2]);
                    return true;
                }
            }
            return false;
        }
    }
}

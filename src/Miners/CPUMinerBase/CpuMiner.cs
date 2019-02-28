using MinerPlugin;
using MinerPlugin.Toolkit;
using MinerPlugin.Interfaces;
using NiceHashMinerLegacy.Common.Enums;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using static NiceHashMinerLegacy.Common.StratumServiceHelpers;
using static MinerPlugin.Toolkit.MinersApiPortsManager;
using NiceHashMinerLegacy.Common.Device;
using System.Collections.Generic;
using System.Globalization;

namespace CPUMinerBase
{
    public class CpuMiner : MinerBase, IAfterStartMining
    {
        // cpuminer can mine only one algorithm at a given time
        private AlgorithmType _algorithmType;

        // command line parts
        private ulong _affinityMask = 0;
        private string _extraLaunchParameters = "";
        private int _apiPort;

        private ApiDataHelper apiReader = new ApiDataHelper(); // consider replacing with HttpClient

        protected virtual string AlgorithmName(AlgorithmType algorithmType)
        {
            switch (algorithmType)
            {
                case AlgorithmType.Lyra2z: return "lyra2z";
            }
            // TODO throw exception
            return "";
        }

        public async override Task<ApiData> GetMinerStatsDataAsync()
        {
            var summaryApiResult = await apiReader.GetApiDataAsync(_apiPort, ApiDataHelper.GetHttpRequestNhmAgentStrin("summary"));
            double totalSpeed = 0;
            int totalPower = 0;
            if (!string.IsNullOrEmpty(summaryApiResult))
            {
                // TODO return empty
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
            var ad = new ApiData();
            var total = new List<(AlgorithmType, double)>();
            total.Add((_algorithmType, totalSpeed));
            ad.AlgorithmSpeedsTotal = total;
            ad.PowerUsageTotal = totalPower;
            // cpuMiner is single device so no need for API

            return ad;
        }

        public async override Task<(double speed, bool ok, string msg)> StartBenchmark(CancellationToken stop, BenchmarkPerformanceType benchmarkType = BenchmarkPerformanceType.Standard)
        {
            // determine benchmark time 
            // settup times
            var benchmarkTime = 20; // in seconds
            switch (benchmarkType)
            {
                case BenchmarkPerformanceType.Quick:
                    benchmarkTime = 20;
                    break;
                case BenchmarkPerformanceType.Standard:
                    benchmarkTime = 60;
                    break;
                case BenchmarkPerformanceType.Precise:
                    benchmarkTime = 120;
                    break;
            }

            var algo = AlgorithmName(_algorithmType);

            var commandLine = $"--algo={algo} --benchmark --time-limit {benchmarkTime} {_extraLaunchParameters}";

            var (binPath, binCwd) = GetBinAndCwdPaths();
            var bp = new BenchmarkProcess(binPath, binCwd, commandLine);
            // TODO benchmark process add after benchmark

            // make sure this is culture invariant
            // TODO implement fallback average, final benchmark 
            bp.CheckData = (string data) => {
                if (double.TryParse(data, out var parsedSpeed)) return (parsedSpeed, true);
                return (0d, false);
            };

            var benchmarkTimeout = TimeSpan.FromSeconds(benchmarkTime + 5);
            var benchmarkWait = TimeSpan.FromMilliseconds(500);
            var t = MinerToolkit.WaitBenchmarkResult(bp, benchmarkTimeout, benchmarkWait, stop);
            return await t;
        }

        protected override (string, string) GetBinAndCwdPaths()
        {
            var binPath = @"D:\Programming\NiceHashMinerLegacy\Release\bin\cpuminer_opt\cpuminer.exe";
            var binCwd = @"D:\Programming\NiceHashMinerLegacy\Release\bin\cpuminer_opt\";
            return (binPath, binCwd);
        }

        protected override void Init()
        {
            bool ok;
            (_algorithmType, ok) = _miningPairs.GetAlgorithmSingleType();
            if (!ok) throw new InvalidOperationException("Invalid mining initialization");

            var cpuDevice = _miningPairs.Select(kvp => kvp.device).FirstOrDefault();
            if (cpuDevice is CPUDevice cpu) {
                // TODO affinity mask stuff
                //_affinityMask
            }

            // TODO implement this later
            //_extraLaunchParameters;
        }

        protected override string MiningCreateCommandLine()
        {
            // API port function might be blocking
            _apiPort = GetAvaliablePortInRange(); // use the default range
            // instant non blocking
            var url = GetLocationUrl(_algorithmType, _miningLocation, NhmConectionType.STRATUM_TCP);
            var algo = AlgorithmName(_algorithmType);

            var commandLine = $"--algo={algo} --url={url} --user={_username} --api-bind={_apiPort} {_extraLaunchParameters}";
            return commandLine;
        }

        public void AfterStartMining()
        {
            int pid = -1;
            if (_miningProcess?.Handle != null) {
                pid = _miningProcess.Handle.Id;
            }
            // TODO C# can have this shorter
            if (_affinityMask != 0 && pid != -1)
            {
                var (ok, msg) = ProcessHelpers.AdjustAffinity(pid, _affinityMask);
                // TODO log what is going on is it ok or not 
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using MyDownloader.Core.Common;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NiceHashMiner.Configs;
using NiceHashMiner.Enums;
using NiceHashMiner.Miners.Parsing;

namespace NiceHashMiner.Miners
{
    public class Xmrig : Miner
    {
        private int _benchmarkTimeWait = 120;
        private const string _lookForStart = "speed 2.5s/60s/15m";
        private const string _lookForEnd = "h/s max";

        public Xmrig() : base("Xmrig") { }

        public override void Start(string url, string btcAdress, string worker) {
            LastCommandLine = GetStartCommand(url, btcAdress, worker);
            ProcessHandle = _Start();
        }

        private string GetStartCommand(string url, string btcAdress, string worker) {
            var extras = ExtraLaunchParametersParser.ParseForMiningSetup(MiningSetup, DeviceType.CPU);
            return $" -o {url} -u {btcAdress}:{worker} --nicehash {extras} --api-port {APIPort}";
        }

        protected override void _Stop(MinerStopType willswitch) {
            Stop_cpu_ccminer_sgminer_nheqminer(willswitch);
        }

        protected override int GET_MAX_CooldownTimeInMilliseconds() {
            return 60 * 1000 * 5;  // 5 min
        }

        public override async Task<APIData> GetSummaryAsync() {
            return await GetSummaryCPUAsync();
        }

        protected override bool IsApiEof(byte third, byte second, byte last) {
            return third == 0x7d && second == 0xa && last == 0x7d;
        }

        #region Benchmark

        protected override string BenchmarkCreateCommandLine(Algorithm algorithm, int time) {
            var server = Globals.GetLocationURL(algorithm.NiceHashID,
                Globals.MiningLocation[ConfigManager.GeneralConfig.ServiceLocation], 
                ConectionType);
            _benchmarkTimeWait = time;
            return GetStartCommand(server, Globals.GetBitcoinUser(), ConfigManager.GeneralConfig.WorkerName.Trim())
                + " -l benchmark_log.txt --print-time=2";
        }

        protected override void BenchmarkThreadRoutine(object CommandLine) {
            BenchmarkThreadRoutineAlternate(CommandLine, _benchmarkTimeWait);
        }

        protected override void ProcessBenchLinesAlternate(string[] lines) {
            // Xmrig reports 2.5s and 60s averages, so prefer to use 60s values for benchmark
            // but fall back on 2.5s values if 60s time isn't hit
            var twoSecTotal = 0d;
            var sixtySecTotal = 0d;
            var twoSecCount = 0;
            var sixtySecCount = 0;

            foreach (var line in lines) {
                bench_lines.Add(line);
                var lineLowered = line.ToLower();
                if (lineLowered.Contains(_lookForStart)) {
                    var speeds = Regex.Match(lineLowered, $"{_lookForStart} (.+?) {_lookForEnd}").Groups[1].Value.Split();
                    if (double.TryParse(speeds[1], out var sixtySecSpeed)) {
                        sixtySecTotal += sixtySecSpeed;
                        ++sixtySecCount;
                    } else if (double.TryParse(speeds[0], out var twoSecSpeed)) {
                        // Store 2.5s data in case 60s is never reached
                        twoSecTotal += twoSecSpeed;
                        ++twoSecCount;
                    }
                }
            }

            if (sixtySecCount > 0 && sixtySecTotal > 0) {
                // Run iff 60s averages are reported
                BenchmarkAlgorithm.BenchmarkSpeed = sixtySecTotal / sixtySecCount;
            } else if (twoSecCount > 0) {
                // Run iff no 60s averages are reported but 2.5s are
                BenchmarkAlgorithm.BenchmarkSpeed = twoSecTotal / twoSecCount;
            }
        }

        protected override void BenchmarkOutputErrorDataReceivedImpl(string outdata) {
            CheckOutdata(outdata);
        }

        protected override bool BenchmarkParseLine(string outdata) {
            Helpers.ConsolePrint(MinerTAG(), outdata);
            return false;
        }

        #endregion
    }
}
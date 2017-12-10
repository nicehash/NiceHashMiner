using Newtonsoft.Json;
using NiceHashMiner.Configs;
using NiceHashMiner.Enums;
using NiceHashMiner.Miners.Grouping;
using NiceHashMiner.Miners.Parsing;
using NiceHashMiner.Devices;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;

namespace NiceHashMiner.Miners {
    public abstract class ClaymoreBaseMiner : Miner {

        protected int benchmarkTimeWait = 2 * 45; // Ok... this was all wrong 
        int benchmark_read_count = 0;
        double benchmark_sum = 0.0d;
        int secondary_benchmark_read_count = 0;
        double secondary_benchmark_sum = 0.0d;
        protected readonly string LOOK_FOR_START;
        const string LOOK_FOR_END = "h/s";

        // only dagger change
        protected bool ignoreZero = false;
        protected double api_read_mult = 1;
        protected AlgorithmType SecondaryAlgorithmType = AlgorithmType.NONE;

        public ClaymoreBaseMiner(string minerDeviceName, string look_FOR_START)
            : base(minerDeviceName) {
            ConectionType = NHMConectionType.STRATUM_TCP;
            LOOK_FOR_START = look_FOR_START.ToLower();
            IsKillAllUsedMinerProcs = true;
        }

        protected abstract double DevFee();

        protected virtual string SecondaryLookForStart() {
            return "";
        }

        // return true if a secondary algo is being used
        public bool IsDual() {
            return (SecondaryAlgorithmType != AlgorithmType.NONE);
        }

        protected override int GET_MAX_CooldownTimeInMilliseconds() {
            return 60 * 1000 * 5; // 5 minute max, whole waiting time 75seconds
        }

        private class JsonApiResponse {
            public List<string> result { get; set; }
            public int id { get; set; }
            public object error { get; set; }
        }

        public override async Task<APIData> GetSummaryAsync() {
            _currentMinerReadStatus = MinerAPIReadStatus.NONE;
            APIData ad = new APIData(MiningSetup.CurrentAlgorithmType, MiningSetup.CurrentSecondaryAlgorithmType);

            TcpClient client = null;
            JsonApiResponse resp = null;
            try {
                byte[] bytesToSend = ASCIIEncoding.ASCII.GetBytes("{\"id\":0,\"jsonrpc\":\"2.0\",\"method\":\"miner_getstat1\"}n");
                client = new TcpClient("127.0.0.1", APIPort);
                NetworkStream nwStream = client.GetStream();
                await nwStream.WriteAsync(bytesToSend, 0, bytesToSend.Length);
                byte[] bytesToRead = new byte[client.ReceiveBufferSize];
                int bytesRead = await nwStream.ReadAsync(bytesToRead, 0, client.ReceiveBufferSize);
                string respStr = Encoding.ASCII.GetString(bytesToRead, 0, bytesRead);
                resp = JsonConvert.DeserializeObject<JsonApiResponse>(respStr, Globals.JsonSettings);
                client.Close();
                //Helpers.ConsolePrint("ClaymoreZcashMiner API back:", respStr);
            } catch (Exception ex) {
                Helpers.ConsolePrint(this.MinerTAG(), "GetSummary exception: " + ex.Message);
            }

            if (resp != null && resp.error == null) {
                //Helpers.ConsolePrint("ClaymoreZcashMiner API back:", "resp != null && resp.error == null");
                if (resp.result != null && resp.result.Count > 4) {
                    //Helpers.ConsolePrint("ClaymoreZcashMiner API back:", "resp.result != null && resp.result.Count > 4");
                    var speeds = resp.result[3].Split(';');
                    var secondarySpeeds = (IsDual()) ? resp.result[5].Split(';') : new string[0];
                    ad.Speed = 0;
                    ad.SecondarySpeed = 0;
                    foreach (var speed in speeds) {
                        //Helpers.ConsolePrint("ClaymoreZcashMiner API back:", "foreach (var speed in speeds) {");
                        double tmpSpeed = 0;
                        try {
                            tmpSpeed = Double.Parse(speed, CultureInfo.InvariantCulture);
                        } catch {
                            tmpSpeed = 0;
                        }
                        ad.Speed += tmpSpeed;
                    }
                    foreach (var speed in secondarySpeeds) {
                        double tmpSpeed = 0;
                        try {
                            tmpSpeed = Double.Parse(speed, CultureInfo.InvariantCulture);
                        } catch {
                            tmpSpeed = 0;
                        }
                        ad.SecondarySpeed += tmpSpeed;
                    }
                    ad.Speed *= api_read_mult;
                    ad.SecondarySpeed *= api_read_mult;
                    _currentMinerReadStatus = MinerAPIReadStatus.GOT_READ;
                }
                if (ad.Speed == 0) {
                    _currentMinerReadStatus = MinerAPIReadStatus.READ_SPEED_ZERO;
                }
                // some clayomre miners have this issue reporting negative speeds in that case restart miner
                if (ad.Speed < 0) {
                    Helpers.ConsolePrint(this.MinerTAG(), "Reporting negative speeds will restart...");
                    this.Restart();
                }
            }

            return ad;
        }

        protected override void _Stop(MinerStopType willswitch) {
            Stop_cpu_ccminer_sgminer_nheqminer(willswitch);
        }

        protected virtual string DeviceCommand(int amdCount = 1) {
            return " -di ";
        }

        // This method now overridden in ClaymoreCryptoNightMiner 
        // Following logic for ClaymoreDual and ClaymoreZcash
        protected override string GetDevicesCommandString() {
            // First by device type (AMD then NV), then by bus ID index
            var sortedMinerPairs = MiningSetup.MiningPairs
                .OrderByDescending(pair => pair.Device.DeviceType)
                .ThenBy(pair => pair.Device.IDByBus)
                .ToList();
            string extraParams = ExtraLaunchParametersParser.ParseForMiningPairs(sortedMinerPairs, DeviceType.AMD);

            List<string> ids = new List<string>();

            int amdDeviceCount = ComputeDeviceManager.Query.AMD_Devices.Count;
            Helpers.ConsolePrint("ClaymoreIndexing", String.Format("Found {0} AMD devices", amdDeviceCount));

            foreach (var mPair in sortedMinerPairs) {
                var id = mPair.Device.IDByBus;
                if (id < 0) {
                    // should never happen
                    Helpers.ConsolePrint("ClaymoreIndexing", "ID by Bus too low: " + id.ToString() + " skipping device");
                    continue;
                }
                if (mPair.Device.DeviceType == DeviceType.NVIDIA) {
                    Helpers.ConsolePrint("ClaymoreIndexing", "NVIDIA device increasing index by " + amdDeviceCount.ToString());
                    id += amdDeviceCount;
                }
                if (id > 9) {  // New >10 GPU support in CD9.8
                    if (id < 36) {  // CD supports 0-9 and a-z indexes, so 36 GPUs
                        char idchar = (char)(id + 87);  // 10 = 97(a), 11 - 98(b), etc
                        ids.Add(idchar.ToString());
                    } else {
                        Helpers.ConsolePrint("ClaymoreIndexing", "ID " + id + " too high, ignoring");
                    }
                } else {
                    ids.Add(id.ToString());
                }
            }
            var deviceStringCommand = DeviceCommand(amdDeviceCount) + String.Join("", ids);

            return deviceStringCommand + extraParams;
        }

        // benchmark stuff

        protected override void BenchmarkThreadRoutine(object CommandLine) {
            BenchmarkThreadRoutineAlternate(CommandLine, benchmarkTimeWait);
        }

        protected override void ProcessBenchLinesAlternate(string[] lines) {
            foreach (var line in lines) {
                if (line != null) {
                    bench_lines.Add(line);
                    string lineLowered = line.ToLower();
                    if (lineLowered.Contains(LOOK_FOR_START)) {
                        if (ignoreZero) {
                            double got = getNumber(lineLowered);
                            if (got != 0) {
                                benchmark_sum += got;
                                ++benchmark_read_count;
                            }
                        } else {
                            benchmark_sum += getNumber(lineLowered);
                            ++benchmark_read_count;
                        }
                    } else if (!String.IsNullOrEmpty(SecondaryLookForStart()) && lineLowered.Contains(SecondaryLookForStart())) {
                        if (ignoreZero) {
                            double got = getNumber(lineLowered, SecondaryLookForStart(), LOOK_FOR_END);
                            if (got != 0) {
                                secondary_benchmark_sum += got;
                                ++secondary_benchmark_read_count;
                            }
                        } else {
                            secondary_benchmark_sum += getNumber(lineLowered);
                            ++secondary_benchmark_read_count;
                        }
                    }
                }
            }
            if (benchmark_read_count > 0) {
                BenchmarkAlgorithm.BenchmarkSpeed = benchmark_sum / benchmark_read_count;
                BenchmarkAlgorithm.SecondaryBenchmarkSpeed = secondary_benchmark_sum / secondary_benchmark_read_count;
            }
        }

        // stub benchmarks read from file
        protected override void BenchmarkOutputErrorDataReceivedImpl(string outdata) {
            CheckOutdata(outdata);
        }

        protected override bool BenchmarkParseLine(string outdata) {
            Helpers.ConsolePrint("BENCHMARK", outdata);
            return false;
        }

        protected double getNumber(string outdata) {
            return getNumber(outdata, LOOK_FOR_START, LOOK_FOR_END);
        }

        protected double getNumber(string outdata, string LOOK_FOR_START, string LOOK_FOR_END) {
            try {
                double mult = 1; 
                int speedStart = outdata.IndexOf(LOOK_FOR_START);
                string speed = outdata.Substring(speedStart, outdata.Length - speedStart);
                speed = speed.Replace(LOOK_FOR_START, "");
                speed = speed.Substring(0, speed.IndexOf(LOOK_FOR_END));

                if (speed.Contains("k")) {
                    mult = 1000;
                    speed = speed.Replace("k", "");
                } else if (speed.Contains("m")) {
                    mult = 1000000;
                    speed = speed.Replace("m", "");
                }
                //Helpers.ConsolePrint("speed", speed);
                speed = speed.Trim();
                return (Double.Parse(speed, CultureInfo.InvariantCulture) * mult) * (1.0 - DevFee() * 0.01);
            } catch (Exception ex) {
                Helpers.ConsolePrint("getNumber", ex.Message + " | args => " + outdata + " | " + LOOK_FOR_END + " | " + LOOK_FOR_START);
            }
            return 0;
        }
    }
}

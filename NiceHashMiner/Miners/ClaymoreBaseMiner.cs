using Newtonsoft.Json;
using NiceHashMiner.Devices;
using NiceHashMiner.Miners.Grouping;
using NiceHashMiner.Miners.Parsing;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using NiceHashMiner.Algorithms;
using NiceHashMinerLegacy.Common.Enums;

namespace NiceHashMiner.Miners
{
    public abstract class ClaymoreBaseMiner : Miner
    {
        protected int BenchmarkTimeWait = 2 * 45; // Ok... this was all wrong 
        private int _benchmarkReadCount;
        private double _benchmarkSum;
        private int _secondaryBenchmarkReadCount;
        private double _secondaryBenchmarkSum;
        protected string LookForStart;
        protected string LookForEnd = "h/s";
        protected string SecondaryLookForStart;

        protected double DevFee;

        // only dagger change
        protected bool IgnoreZero = false;

        protected double ApiReadMult = 1;
        protected AlgorithmType SecondaryAlgorithmType = AlgorithmType.NONE;

        // CD intensity tuning
        protected const int defaultIntensity = 30;

        protected ClaymoreBaseMiner(string minerDeviceName)
            : base(minerDeviceName)
        {
            ConectionType = NhmConectionType.STRATUM_SSL;
            IsKillAllUsedMinerProcs = true;
        }

        // return true if a secondary algo is being used
        public bool IsDual()
        {
            return (SecondaryAlgorithmType != AlgorithmType.NONE);
        }

        protected override int GetMaxCooldownTimeInMilliseconds()
        {
            return 60 * 1000 * 5; // 5 minute max, whole waiting time 75seconds
        }

        private class JsonApiResponse
        {
#pragma warning disable IDE1006 // Naming Styles
            public List<string> result { get; set; }
            public int id { get; set; }
            public object error { get; set; }
#pragma warning restore IDE1006 // Naming Styles
        }

        public override async Task<ApiData> GetSummaryAsync()
        {
            CurrentMinerReadStatus = MinerApiReadStatus.NONE;
            var ad = new ApiData(MiningSetup.CurrentAlgorithmType, MiningSetup.CurrentSecondaryAlgorithmType);

            JsonApiResponse resp = null;
            try
            {
                var bytesToSend = Encoding.ASCII.GetBytes("{\"id\":0,\"jsonrpc\":\"2.0\",\"method\":\"miner_getstat1\"}n");
                var client = new TcpClient("127.0.0.1", ApiPort);
                var nwStream = client.GetStream();
                await nwStream.WriteAsync(bytesToSend, 0, bytesToSend.Length);
                var bytesToRead = new byte[client.ReceiveBufferSize];
                var bytesRead = await nwStream.ReadAsync(bytesToRead, 0, client.ReceiveBufferSize);
                var respStr = Encoding.ASCII.GetString(bytesToRead, 0, bytesRead);
                resp = JsonConvert.DeserializeObject<JsonApiResponse>(respStr, Globals.JsonSettings);
                client.Close();
                //Helpers.ConsolePrint("ClaymoreZcashMiner API back:", respStr);
            }
            catch (Exception ex)
            {
                Helpers.ConsolePrint(MinerTag(), "GetSummary exception: " + ex.Message);
            }

            if (resp != null && resp.error == null)
            {
                //Helpers.ConsolePrint("ClaymoreZcashMiner API back:", "resp != null && resp.error == null");
                if (resp.result != null && resp.result.Count > 4)
                {
                    //Helpers.ConsolePrint("ClaymoreZcashMiner API back:", "resp.result != null && resp.result.Count > 4");
                    var speeds = resp.result[3].Split(';');
                    var secondarySpeeds = (IsDual()) ? resp.result[5].Split(';') : new string[0];
                    ad.Speed = 0;
                    ad.SecondarySpeed = 0;
                    foreach (var speed in speeds)
                    {
                        //Helpers.ConsolePrint("ClaymoreZcashMiner API back:", "foreach (var speed in speeds) {");
                        double tmpSpeed;
                        try
                        {
                            tmpSpeed = double.Parse(speed, CultureInfo.InvariantCulture);
                        }
                        catch
                        {
                            tmpSpeed = 0;
                        }

                        ad.Speed += tmpSpeed;
                    }

                    foreach (var speed in secondarySpeeds)
                    {
                        double tmpSpeed;
                        try
                        {
                            tmpSpeed = double.Parse(speed, CultureInfo.InvariantCulture);
                        }
                        catch
                        {
                            tmpSpeed = 0;
                        }

                        ad.SecondarySpeed += tmpSpeed;
                    }

                    ad.Speed *= ApiReadMult;
                    ad.SecondarySpeed *= ApiReadMult;
                    CurrentMinerReadStatus = MinerApiReadStatus.GOT_READ;
                }

                if (ad.Speed == 0)
                {
                    CurrentMinerReadStatus = MinerApiReadStatus.READ_SPEED_ZERO;
                }

                // some clayomre miners have this issue reporting negative speeds in that case restart miner
                if (ad.Speed < 0)
                {
                    Helpers.ConsolePrint(MinerTag(), "Reporting negative speeds will restart...");
                    Restart();
                }
            }

            return ad;
        }

        protected override void _Stop(MinerStopType willswitch)
        {
            Stop_cpu_ccminer_sgminer_nheqminer(willswitch);
        }

        protected virtual string DeviceCommand(int amdCount = 1)
        {
            return " -di ";
        }

        // This method now overridden in ClaymoreCryptoNightMiner 
        // Following logic for ClaymoreDual and ClaymoreZcash
        protected override string GetDevicesCommandString()
        {
            // First by device type (AMD then NV), then by bus ID index
            var sortedMinerPairs = MiningSetup.MiningPairs
                .OrderByDescending(pair => pair.Device.DeviceType)
                .ThenBy(pair => pair.Device.IDByBus)
                .ToList();
            var extraParams = ExtraLaunchParametersParser.ParseForMiningPairs(sortedMinerPairs, DeviceType.AMD);

            var ids = new List<string>();
            var intensities = new List<string>();

            var amdDeviceCount = ComputeDeviceManager.Query.AmdDevices.Count;
            Helpers.ConsolePrint("ClaymoreIndexing", $"Found {amdDeviceCount} AMD devices");

            foreach (var mPair in sortedMinerPairs)
            {
                var id = mPair.Device.IDByBus;
                if (id < 0)
                {
                    // should never happen
                    Helpers.ConsolePrint("ClaymoreIndexing", "ID by Bus too low: " + id + " skipping device");
                    continue;
                }

                if (mPair.Device.DeviceType == DeviceType.NVIDIA)
                {
                    Helpers.ConsolePrint("ClaymoreIndexing", "NVIDIA device increasing index by " + amdDeviceCount);
                    id += amdDeviceCount;
                }

                if (id > 9)
                {
                    // New >10 GPU support in CD9.8
                    if (id < 36)
                    {
                        // CD supports 0-9 and a-z indexes, so 36 GPUs
                        var idchar = (char) (id + 87); // 10 = 97(a), 11 - 98(b), etc
                        ids.Add(idchar.ToString());
                    }
                    else
                    {
                        Helpers.ConsolePrint("ClaymoreIndexing", "ID " + id + " too high, ignoring");
                    }
                }
                else
                {
                    ids.Add(id.ToString());
                }

                if (mPair.Algorithm is DualAlgorithm algo && algo.TuningEnabled)
                {
                    intensities.Add(algo.CurrentIntensity.ToString());
                }
            }

            var deviceStringCommand = DeviceCommand(amdDeviceCount) + string.Join("", ids);
            var intensityStringCommand = "";
            if (intensities.Count > 0)
            {
                intensityStringCommand = " -dcri " + string.Join(",", intensities);
            }

            return deviceStringCommand + intensityStringCommand + extraParams;
        }

        // benchmark stuff

        protected override void BenchmarkThreadRoutine(object commandLine)
        {
            if (BenchmarkAlgorithm is DualAlgorithm dualBenchAlgo && dualBenchAlgo.TuningEnabled)
            {
                var stepsLeft = (int) Math.Ceiling((double) (dualBenchAlgo.TuningEnd - dualBenchAlgo.CurrentIntensity) /
                                                   (dualBenchAlgo.TuningInterval)) + 1;
                Helpers.ConsolePrint("CDTUING", "{0} tuning steps remain, should complete in {1} seconds", stepsLeft,
                    stepsLeft * BenchmarkTimeWait);
                Helpers.ConsolePrint("CDTUNING",
                    $"Starting benchmark for intensity {dualBenchAlgo.CurrentIntensity} out of {dualBenchAlgo.TuningEnd}");
            }

            _benchmarkReadCount = 0;
            _benchmarkSum = 0;
            _secondaryBenchmarkReadCount = 0;
            _secondaryBenchmarkSum = 0;

            BenchmarkThreadRoutineAlternate(commandLine, BenchmarkTimeWait);
        }

        protected override void ProcessBenchLinesAlternate(string[] lines)
        {
            foreach (var line in lines)
            {
                if (line != null)
                {
                    BenchLines.Add(line);
                    var lineLowered = line.ToLower();
                    if (lineLowered.Contains(LookForStart))
                    {
                        var got = GetNumber(lineLowered);
                        if (!IgnoreZero || got > 0)
                        {
                            _benchmarkSum += got;
                            ++_benchmarkReadCount;
                        }
                    }
                    else if (!string.IsNullOrEmpty(SecondaryLookForStart) &&
                             lineLowered.Contains(SecondaryLookForStart))
                    {
                        var got = GetNumber(lineLowered, SecondaryLookForStart, LookForEnd);
                        if (IgnoreZero || got > 0)
                        {
                            _secondaryBenchmarkSum += got;
                            ++_secondaryBenchmarkReadCount;
                        }
                    }
                }
            }

            if (_benchmarkReadCount > 0)
            {
                var speed = _benchmarkSum / _benchmarkReadCount;
                BenchmarkAlgorithm.BenchmarkSpeed = speed;
                if (BenchmarkAlgorithm is DualAlgorithm dualBenchAlgo)
                {
                    var secondarySpeed = _secondaryBenchmarkSum / Math.Max(1, _secondaryBenchmarkReadCount);
                    if (dualBenchAlgo.TuningEnabled)
                    {
                        dualBenchAlgo.SetIntensitySpeedsForCurrent(speed, secondarySpeed);
                    }
                    else
                    {
                        dualBenchAlgo.SecondaryBenchmarkSpeed = secondarySpeed;
                    }
                }
            }
        }

        // stub benchmarks read from file
        protected override void BenchmarkOutputErrorDataReceivedImpl(string outdata)
        {
            CheckOutdata(outdata);
        }

        protected override bool BenchmarkParseLine(string outdata)
        {
            Helpers.ConsolePrint("BENCHMARK", outdata);
            return false;
        }

        protected double GetNumber(string outdata)
        {
            return GetNumber(outdata, LookForStart, LookForEnd);
        }

        protected double GetNumber(string outdata, string LOOK_FOR_START, string LOOK_FOR_END)
        {
            try
            {
                double mult = 1;
                var speedStart = outdata.IndexOf(LOOK_FOR_START, StringComparison.Ordinal);
                var speed = outdata.Substring(speedStart, outdata.Length - speedStart);
                speed = speed.Replace(LOOK_FOR_START, "");
                speed = speed.Substring(0, speed.IndexOf(LOOK_FOR_END, StringComparison.Ordinal));

                if (speed.Contains("k"))
                {
                    mult = 1000;
                    speed = speed.Replace("k", "");
                }
                else if (speed.Contains("m"))
                {
                    mult = 1000000;
                    speed = speed.Replace("m", "");
                }

                //Helpers.ConsolePrint("speed", speed);
                speed = speed.Trim();
                return (double.Parse(speed, CultureInfo.InvariantCulture) * mult) * (1.0 - DevFee * 0.01);
            }
            catch (Exception ex)
            {
                Helpers.ConsolePrint("GetNumber",
                    ex.Message + " | args => " + outdata + " | " + LOOK_FOR_END + " | " + LOOK_FOR_START);
            }

            return 0;
        }
    }
}

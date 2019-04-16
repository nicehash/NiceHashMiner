// PRODUCTION
#if !(TESTNET || TESTNETDEV)
using NiceHashMiner.Configs;
using System;
using System.Collections.Generic;
using System.Linq;
using NiceHashMiner.Algorithms;
using NiceHashMiner.Devices;
using NiceHashMiner.Miners.Grouping;
using NiceHashMiner.Miners.Parsing;
using NiceHashMinerLegacy.Common.Enums;
using NiceHashMinerLegacy.Common;

namespace NiceHashMiner.Miners
{
    public class ClaymoreDualOld : ClaymoreBaseMiner
    {
        public ClaymoreDualOld(AlgorithmType secondaryAlgorithmType)
            : base("ClaymoreDual")
        {
            IgnoreZero = true;
            ApiReadMult = 1000;
            ConectionType = NhmConectionType.STRATUM_TCP;
            SecondaryAlgorithmType = secondaryAlgorithmType;

            LookForStart = "eth - total speed:";
            SecondaryLookForStart = SecondaryShortName() + " - total speed:";
            DevFee = 1.0;

            _enviormentVariables = new Dictionary<string, string>()
            {
                {"GPU_MAX_ALLOC_PERCENT", "100"},
                {"GPU_USE_SYNC_OBJECTS", "1"},
                {"GPU_SINGLE_ALLOC_PERCENT", "100"},
                {"GPU_MAX_HEAP_SIZE", "100"},
                {"GPU_FORCE_64BIT_PTR", "0"}
            };

            IsMultiType = true;
        }

#pragma warning disable 0618
        // the short form the miner uses for secondary algo in cmd line and log
        public string SecondaryShortName()
        {
            switch (SecondaryAlgorithmType)
            {
                case AlgorithmType.Decred:
                    return "dcr";
                case AlgorithmType.Lbry:
                    return "lbc";
                case AlgorithmType.Pascal:
                    return "pasc";
                case AlgorithmType.Sia:
                    return "sc";
                case AlgorithmType.Blake2s:
                    return "b2s";
                case AlgorithmType.Keccak:
                    return "kc";
            }

            return "";
        }
#pragma warning restore 0618

        protected override int GetMaxCooldownTimeInMilliseconds()
        {
            return 90 * 1000; // 1.5 minute max, whole waiting time 75seconds
        }

#pragma warning disable 0618
        private string GetStartCommand(string url, string username)
        {
            var dualModeParams = "";
            if (!IsDual())
            {
                // leave convenience param for non-dual entry
                foreach (var pair in MiningSetup.MiningPairs)
                {
                    if (!pair.CurrentExtraLaunchParameters.Contains("-dual=")) continue;
                    var dual = AlgorithmType.NONE;
                    var coinP = "";
                    if (pair.CurrentExtraLaunchParameters.Contains("Decred"))
                    {
                        dual = AlgorithmType.Decred;
                        coinP = " -dcoin dcr ";
                    }

                    if (pair.CurrentExtraLaunchParameters.Contains("Siacoin"))
                    {
                        dual = AlgorithmType.Sia;
                        coinP = " -dcoin sc";
                    }

                    if (pair.CurrentExtraLaunchParameters.Contains("Lbry"))
                    {
                        dual = AlgorithmType.Lbry;
                        coinP = " -dcoin lbc ";
                    }

                    if (pair.CurrentExtraLaunchParameters.Contains("Pascal"))
                    {
                        dual = AlgorithmType.Pascal;
                        coinP = " -dcoin pasc ";
                    }

                    if (dual != AlgorithmType.NONE)
                    {
                        var urlSecond = StratumServiceHelpers.GetLocationUrl(dual,
                            StratumService.SelectedServiceLocation,
                            ConectionType);
                        dualModeParams = $" {coinP} -dpool {urlSecond} -dwal {username}";
                        break;
                    }
                }
            }
            else
            {
                var urlSecond = StratumServiceHelpers.GetLocationUrl(SecondaryAlgorithmType,
                    StratumService.SelectedServiceLocation, ConectionType);
                dualModeParams = $" -dcoin {SecondaryShortName()} -dpool {urlSecond} -dwal {username} -dpsw x";
            }

            return " "
                   + GetDevicesCommandString()
                   + $"  -epool {url} -ewal {username} -mport 127.0.0.1:-{ApiPort} -esm 3 -epsw x -allpools 1"
                   + dualModeParams;
        }
#pragma warning restore 0618

        protected virtual IEnumerable<MiningPair> SortDeviceList(IEnumerable<MiningPair> startingList)
        {
            // CD case, sort device type (AMD first) then by bus ID
            return startingList
                .OrderByDescending(pair => pair.Device.DeviceType)
                .ThenBy(pair => pair.Device.IDByBus);
        }

        protected virtual int GetIDOffsetForType(DeviceType type)
        {
            return type == DeviceType.NVIDIA ? AvailableDevices.NumDetectedAmdDevs : 0;
        }

        private static int GetPlatformIDForType(DeviceType type)
        {
            switch (type)
            {
                case DeviceType.AMD:
                    return 1;
                case DeviceType.NVIDIA:
                    return 2;
                default:
                    return 3;
            }
        }

        // This method now overridden in ClaymoreCryptoNightMiner 
        // Following logic for ClaymoreDual and ClaymoreZcash
        protected override string GetDevicesCommandString()
        {
            // First by device type (AMD then NV), then by bus ID index
            var sortedMinerPairs = SortDeviceList(MiningSetup.MiningPairs)
                .ToList();
            var extraParams = ExtraLaunchParametersParser.ParseForMiningPairs(sortedMinerPairs, DeviceType.AMD);

            var ids = new List<string>();
            var intensities = new List<string>();

            var firstDevType = sortedMinerPairs.First().Device.DeviceType;
            var hasMixedDevs = sortedMinerPairs.Skip(1).Any(p => p.Device.DeviceType != firstDevType);

            foreach (var mPair in sortedMinerPairs)
            {
                var id = mPair.Device.IDByBus;
                if (id < 0)
                {
                    // should never happen
                    Helpers.ConsolePrint("ClaymoreIndexing", "ID by Bus too low: " + id + " skipping device");
                    continue;
                }

                if (hasMixedDevs)
                {
                    var offset = GetIDOffsetForType(mPair.Device.DeviceType);
                    Helpers.ConsolePrint("ClaymoreIndexing", $"Increasing index by {offset}");
                    id += offset;
                }

                ids.Add(GetAlphanumericID(id));

                if (mPair.Algorithm is DualAlgorithm algo && algo.TuningEnabled)
                {
                    intensities.Add(algo.CurrentIntensity.ToString());
                }
            }

            var deviceStringCommand = "-di " + string.Join("", ids);
            if (!hasMixedDevs)
            {
                deviceStringCommand += $" -platform {GetPlatformIDForType(firstDevType)} ";
            }

            var intensityStringCommand = "";
            if (intensities.Count > 0)
            {
                intensityStringCommand = " -dcri " + string.Join(",", intensities);
            }

            return deviceStringCommand + intensityStringCommand + extraParams;
        }

        public override void Start(string url, string username)
        {
            // Update to most profitable intensity
            foreach (var mPair in MiningSetup.MiningPairs)
            {
                if (mPair.Algorithm is DualAlgorithm algo && algo.TuningEnabled)
                {
                    var intensity = algo.MostProfitableIntensity;
                    if (intensity < 0) intensity = defaultIntensity;
                    algo.CurrentIntensity = intensity;
                }
            }

            LastCommandLine = GetStartCommand(url, username) + " -dbg -1";
            ProcessHandle = _Start();
        }

        // benchmark stuff

        protected override string BenchmarkCreateCommandLine(Algorithm algorithm, int time)
        {
            // network stub
            var url = GetServiceUrl(algorithm.NiceHashID);
            // demo for benchmark
            var ret = GetStartCommand(url, DemoUser.BTC)
                         + " -logfile " + GetLogFileName();
            // local benhcmark
            if (!IsDual())
            {
                BenchmarkTimeWait = time;
                return $"{ret} {GetBenchmarkOption()}"; // benchmark 1 does not output secondary speeds
            }

            // dual seems to stop mining after this time if redirect output is true
            BenchmarkTimeWait = Math.Max(60, Math.Min(120, time * 3));
            return ret;
        }

        protected virtual string GetBenchmarkOption()
        {
            return "-benchmark 1";
        }
    }
}
#endif

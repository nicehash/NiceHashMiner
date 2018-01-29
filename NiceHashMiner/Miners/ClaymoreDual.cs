﻿using NiceHashMiner.Configs;
using NiceHashMiner.Enums;
using System;

namespace NiceHashMiner.Miners
{
    public class ClaymoreDual : ClaymoreBaseMiner
    {
        private const string _LookForStart = "ETH - Total Speed:";

        public ClaymoreDual(AlgorithmType secondaryAlgorithmType)
            : base("ClaymoreDual", _LookForStart)
        {
            IgnoreZero = true;
            ApiReadMult = 1000;
            ConectionType = NhmConectionType.STRATUM_TCP;
            SecondaryAlgorithmType = secondaryAlgorithmType;
        }

        // eth-only: 1%
        // eth-dual-mine: 2%
        protected override double DevFee()
        {
            return IsDual() ? 2.0 : 1.0;
        }

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
            }
            return "";
        }

        protected override string SecondaryLookForStart()
        {
            return (SecondaryShortName() + " - Total Speed:").ToLower();
        }

        protected override int GetMaxCooldownTimeInMilliseconds()
        {
            return 90 * 1000; // 1.5 minute max, whole waiting time 75seconds
        }

        private string GetStartCommand(string url, string btcAdress, string worker)
        {
            var username = GetUsername(btcAdress, worker);

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
                        var urlSecond = Globals.GetLocationUrl(dual, Globals.MiningLocation[ConfigManager.GeneralConfig.ServiceLocation],
                            ConectionType);
                        dualModeParams = $" {coinP} -dpool {urlSecond} -dwal {username}";
                        break;
                    }
                }
            }
            else
            {
                var urlSecond = Globals.GetLocationUrl(SecondaryAlgorithmType,
                    Globals.MiningLocation[ConfigManager.GeneralConfig.ServiceLocation], ConectionType);
                dualModeParams = $" -dcoin {SecondaryShortName()} -dpool {urlSecond} -dwal {username} -dpsw x";
            }

            return " "
                   + GetDevicesCommandString()
                   + $"  -epool {url} -ewal {username} -mport 127.0.0.1:-{ApiPort} -esm 3 -epsw x -allpools 1"
                   + dualModeParams;
        }

        public override void Start(string url, string btcAdress, string worker)
        {
            var username = GetUsername(btcAdress, worker);
            LastCommandLine = GetStartCommand(url, btcAdress, worker) + " -dbg -1";
            ProcessHandle = _Start();
        }

        protected override string DeviceCommand(int amdCount = 1)
        {
            // If no AMD cards loaded, instruct CD to only regard NV cards for indexing
            // This will allow proper indexing if AMD GPUs or APUs are present in the system but detection disabled
            var ret = (amdCount == 0) ? " -platform 2" : "";
            return ret + base.DeviceCommand(amdCount);
        }

        // benchmark stuff

        protected override string BenchmarkCreateCommandLine(Algorithm algorithm, int time)
        {
            // network stub
            var url = GetServiceUrl(algorithm.NiceHashID);
            // demo for benchmark
            var ret = GetStartCommand(url, Globals.GetBitcoinUser(), ConfigManager.GeneralConfig.WorkerName.Trim());
            // local benhcmark
            if (!IsDual())
            {
                BenchmarkTimeWait = time;
                return ret + "  -benchmark 1";
            }
            BenchmarkTimeWait =
                Math.Max(60, Math.Min(120, time * 3)); // dual seems to stop mining after this time if redirect output is true
            return ret; // benchmark 1 does not output secondary speeds
        }
    }
}

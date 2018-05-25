using NiceHashMiner.Configs;
using System;
using NiceHashMiner.Algorithms;
using NiceHashMinerLegacy.Common.Enums;

namespace NiceHashMiner.Miners
{
    public class ClaymoreDual : ClaymoreBaseMiner
    {
        public ClaymoreDual(AlgorithmType secondaryAlgorithmType)
            : base("ClaymoreDual")
        {
            IgnoreZero = true;
            ApiReadMult = 1000;
            ConectionType = NhmConectionType.STRATUM_TCP;
            SecondaryAlgorithmType = secondaryAlgorithmType;

            LookForStart = "eth - total speed:";
            SecondaryLookForStart = SecondaryShortName() + " - total speed:";
            DevFee = IsDual() ? 1.5 : 1.0;

            IsMultiType = true;
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
                case AlgorithmType.Blake2s:
                    return "b2s";
                case AlgorithmType.Keccak:
                    return "kc";
            }

            return "";
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
                        var urlSecond = Globals.GetLocationUrl(dual,
                            Globals.MiningLocation[ConfigManager.GeneralConfig.ServiceLocation],
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
            var ret = GetStartCommand(url, Globals.GetBitcoinUser(), ConfigManager.GeneralConfig.WorkerName.Trim())
                         + " -logfile " + GetLogFileName();
            // local benhcmark
            if (!IsDual())
            {
                BenchmarkTimeWait = time;
                return ret + "  -benchmark 1"; // benchmark 1 does not output secondary speeds
            }

            // dual seems to stop mining after this time if redirect output is true
            BenchmarkTimeWait = Math.Max(60, Math.Min(120, time * 3));
            return ret;
        }
    }
}

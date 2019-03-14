using NiceHashMiner.Devices;
using NiceHashMinerLegacy.Common.Enums;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NiceHashMinerLegacy.Extensions;
using NiceHashMiner.Algorithms;
using NiceHashMiner.Configs;

namespace NiceHashMiner.Miners
{
    public class TeamRedMiner : Miner
    {

        public TeamRedMiner() : base("TeamRedMiner")
        { }

        private string AlgoName
        {
            get
            {
                switch (MiningSetup.CurrentAlgorithmType)
                {
                    case AlgorithmType.CryptoNightV8:
                        return "cnv8";
                    case AlgorithmType.CryptoNightR:
                        return "cnr";
                    case AlgorithmType.Lyra2REv3:
                        return "lyra2rev3";
                    default:
                        return "";
                }
            }
        }

        private double DevFee
        {
            get
            {
                switch (MiningSetup.CurrentAlgorithmType)
                {
                    case AlgorithmType.CryptoNightV8:
                    case AlgorithmType.CryptoNightR:
                    case AlgorithmType.Lyra2REv3:
                        return 0.025d; // 2.5%
                    default:
                        return 0.03d; // 3.0%
                }
            }
        }

        protected override int GetMaxCooldownTimeInMilliseconds()
        {
            return 60 * 1000;
        }

        private string GetStartCommand(string url, string btcAddress, string worker)
        {
            var user = GetUsername(btcAddress, worker);
            var devs = string.Join(",", MiningSetup.MiningPairs.Select(p => p.Device.ID));
            var cmd = $"-a {AlgoName} -o {url} -u {user} --platform={AvailableDevices.AmdOpenCLPlatformNum} -d {devs}";
            //cmd += ExtraLaunchParametersParser.ParseForMiningSetup(MiningSetup, DeviceType.AMD);

            return cmd;
        }

        int _benchCount = 0;
        protected override bool BenchmarkParseLine(string outdata)
        {
            if (outdata.Contains("GPU") && outdata.ToLower().TryGetHashrateAfter($"{AlgoName}:", out var hashrate) && hashrate > 0)
            {
                ++_benchCount;
                if (_benchCount == 3)
                {
                    BenchmarkAlgorithm.BenchmarkSpeed = hashrate * (1.0d - DevFee);
                    return true;
                }
            }
            Helpers.ConsolePrint(MinerTag(), $"BenchmarkParseLine {outdata}");

            return false;
        }

        public override void Start(string url, string btcAdress, string worker)
        {
            LastCommandLine = GetStartCommand(url, btcAdress, worker) + $" --api_listen=127.0.0.1:{ApiPort}";
            ProcessHandle = _Start();
        }

        protected override string BenchmarkCreateCommandLine(Algorithm algorithm, int time)
        {
            var id = MiningSetup.MiningPairs.Select(pair => pair.Device.ID).FirstOrDefault();
            var url = Globals.GetLocationUrl(algorithm.NiceHashID, Globals.MiningLocation[ConfigManager.GeneralConfig.ServiceLocation],
                ConectionType);
            return GetStartCommand(url, Globals.DemoUser, "benchmark") + " --disable_colors";
        }

        protected override void BenchmarkOutputErrorDataReceivedImpl(string outdata)
        {
            CheckOutdata(outdata);
        }

        protected override void _Stop(MinerStopType willswitch)
        {
            ShutdownMiner();
        }

        // sgminer copy paste
        public override async Task<ApiData> GetSummaryAsync()
        {
            var ad = new ApiData(MiningSetup.CurrentAlgorithmType);

            var resp = await GetApiDataAsync(ApiPort, "summary");
            if (resp == null)
            {
                CurrentMinerReadStatus = MinerApiReadStatus.NONE;
                return null;
            }
            //// sgminer debug log
            //Helpers.ConsolePrint("sgminer-DEBUG_resp", resp);

            try
            {
                // Checks if all the GPUs are Alive first
                var resp2 = await GetApiDataAsync(ApiPort, "devs");
                if (resp2 == null)
                {
                    CurrentMinerReadStatus = MinerApiReadStatus.NONE;
                    return null;
                }
                //// sgminer debug log
                //Helpers.ConsolePrint("sgminer-DEBUG_resp2", resp2);

                var checkGpuStatus = resp2.Split(new[] { '|' }, StringSplitOptions.RemoveEmptyEntries);

                for (var i = 1; i < checkGpuStatus.Length - 1; i++)
                {
                    if (checkGpuStatus[i].Contains("Enabled=Y") && !checkGpuStatus[i].Contains("Status=Alive"))
                    {
                        Helpers.ConsolePrint(MinerTag(),
                            ProcessTag() + " GPU " + i + ": Sick/Dead/NoStart/Initialising/Disabled/Rejecting/Unknown");
                        CurrentMinerReadStatus = MinerApiReadStatus.WAIT;
                        return null;
                    }
                }

                var resps = resp.Split(new[] { '|' }, StringSplitOptions.RemoveEmptyEntries);

                if (resps[1].Contains("SUMMARY"))
                {
                    var data = resps[1].Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

                    // Get miner's current total speed
                    var speed = data[4].Split(new[] { '=' }, StringSplitOptions.RemoveEmptyEntries);
                    // Get miner's current total MH
                    var totalMH = double.Parse(data[18].Split(new[] { '=' }, StringSplitOptions.RemoveEmptyEntries)[1],
                        new CultureInfo("en-US"));

                    ad.Speed = double.Parse(speed[1]) * 1000;

                    if (totalMH <= PreviousTotalMH)
                    {
                        Helpers.ConsolePrint(MinerTag(), ProcessTag() + " SGMiner might be stuck as no new hashes are being produced");
                        Helpers.ConsolePrint(MinerTag(),
                            ProcessTag() + " Prev Total MH: " + PreviousTotalMH + " .. Current Total MH: " + totalMH);
                        CurrentMinerReadStatus = MinerApiReadStatus.NONE;
                        return null;
                    }

                    PreviousTotalMH = totalMH;
                }
                else
                {
                    ad.Speed = 0;
                }
            }
            catch
            {
                CurrentMinerReadStatus = MinerApiReadStatus.NONE;
                return null;
            }

            CurrentMinerReadStatus = MinerApiReadStatus.GOT_READ;
            // check if speed zero
            if (ad.Speed == 0) CurrentMinerReadStatus = MinerApiReadStatus.READ_SPEED_ZERO;

            return ad;
        }

    }
}

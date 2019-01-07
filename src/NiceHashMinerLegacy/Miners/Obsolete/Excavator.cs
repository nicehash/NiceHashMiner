using Newtonsoft.Json;
using NiceHashMiner.Miners.Grouping;
using NiceHashMiner.Miners.Parsing;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using NiceHashMiner.Algorithms;
using NiceHashMinerLegacy.Common.Enums;

namespace NiceHashMiner.Miners
{
    [Obsolete("Excavator is deprecated")]
    public class Excavator : Miner
    {
#pragma warning disable IDE1006
        private class DeviceStat
        {
            public int id { get; set; }
            public string name { get; set; }
            public double speed_hps { get; set; }
        }

        private class Result
        {
            public bool connected { get; set; }
            public double interval_seconds { get; set; }
            public double speed_hps { get; set; }
            public List<DeviceStat> devices { get; set; }
            public double accepted_per_minute { get; set; }
            public double rejected_per_minute { get; set; }
        }

        private class JsonApiResponse
        {
            public string method { get; set; }
            public Result result { get; set; }
            public object error { get; set; }
        }
#pragma warning restore IDE1006

        public Excavator()
            : base("excavator")
        {
            ConectionType = NhmConectionType.NONE;
            IsNeverHideMiningWindow = true;
        }

        public override void Start(string url, string btcAdress, string worker)
        {
            var username = GetUsername(btcAdress, worker);
            LastCommandLine = GetDevicesCommandString() + " -a " + MiningSetup.MinerName + " -p " + ApiPort + " -s " + url + " -u " +
                              username + ":x";
            ProcessHandle = _Start();
        }

        protected override string GetDevicesCommandString()
        {
            var ctMiningPairs = new List<MiningPair>();
            var deviceStringCommand = " -cd ";
            var defaultCT = 1;
            if (MiningSetup.CurrentAlgorithmType == AlgorithmType.Equihash)
            {
                defaultCT = 2;
            }
            foreach (var nvidiaPair in MiningSetup.MiningPairs)
            {
                if (nvidiaPair.CurrentExtraLaunchParameters.Contains("-ct"))
                {
                    for (var i = 0; i < ExtraLaunchParametersParser.GetEqmCudaThreadCount(nvidiaPair); ++i)
                    {
                        deviceStringCommand += nvidiaPair.Device.ID + " ";
                        ctMiningPairs.Add(nvidiaPair);
                    }
                }
                else
                {
                    // use default default_CT for best performance
                    for (var i = 0; i < defaultCT; ++i)
                    {
                        deviceStringCommand += nvidiaPair.Device.ID + " ";
                        ctMiningPairs.Add(nvidiaPair);
                    }
                }
            }

            var ctMiningSetup = new MiningSetup(ctMiningPairs);
            //deviceStringCommand += " " + ExtraLaunchParametersParser.ParseForMiningSetup(this.MiningSetup, DeviceType.NVIDIA);
            deviceStringCommand += " " + ExtraLaunchParametersParser.ParseForMiningSetup(ctMiningSetup, DeviceType.NVIDIA);

            return deviceStringCommand;
        }

        // benchmark stuff

        protected override string BenchmarkCreateCommandLine(Algorithm algorithm, int time)
        {
            var ret = " -a " + MiningSetup.MinerName + " -b " + time + " " + GetDevicesCommandString();
            return ret;
        }

        private const string TotalMes = "Total measured:";

        protected override bool BenchmarkParseLine(string outdata)
        {
            if (outdata.Contains(TotalMes))
            {
                try
                {
                    var speedStart = outdata.IndexOf(TotalMes);
                    var speed = outdata.Substring(speedStart, outdata.Length - speedStart).Replace(TotalMes, "");
                    var splitSrs = speed.Trim().Split(' ');
                    if (splitSrs.Length >= 2)
                    {
                        var speedStr = splitSrs[0];
                        var postfixStr = splitSrs[1];
                        var spd = double.Parse(speedStr, CultureInfo.InvariantCulture);
                        if (postfixStr.Contains("kH/s"))
                            spd *= 1000;
                        else if (postfixStr.Contains("MH/s"))
                            spd *= 1000000;
                        else if (postfixStr.Contains("GH/s"))
                            spd *= 1000000000;

                        // wrong benchmark workaround over 3gh/s is considered false
                        if (MiningSetup.CurrentAlgorithmType == AlgorithmType.Pascal
                            && spd > 3.0d * 1000000000.0d
                        )
                        {
                            return false;
                        }

                        BenchmarkAlgorithm.BenchmarkSpeed = spd;
                        return true;
                    }
                }
                catch { }
            }
            return false;
        }

        public override async Task<ApiData> GetSummaryAsync()
        {
            CurrentMinerReadStatus = MinerApiReadStatus.NONE;
            var ad = new ApiData(MiningSetup.CurrentAlgorithmType);

            JsonApiResponse resp = null;
            try
            {
                var bytesToSend = Encoding.ASCII.GetBytes("status\n");
                var client = new TcpClient("127.0.0.1", ApiPort);
                var nwStream = client.GetStream();
                await nwStream.WriteAsync(bytesToSend, 0, bytesToSend.Length);
                var bytesToRead = new byte[client.ReceiveBufferSize];
                var bytesRead = await nwStream.ReadAsync(bytesToRead, 0, client.ReceiveBufferSize);
                var respStr = Encoding.ASCII.GetString(bytesToRead, 0, bytesRead);
                resp = JsonConvert.DeserializeObject<JsonApiResponse>(respStr, Globals.JsonSettings);
                client.Close();
            }
            catch (Exception ex)
            {
                Helpers.ConsolePrint("ERROR", ex.Message);
            }

            if (resp != null && resp.error == null)
            {
                ad.Speed = resp.result.speed_hps;
                CurrentMinerReadStatus = MinerApiReadStatus.GOT_READ;
                if (ad.Speed == 0)
                {
                    CurrentMinerReadStatus = MinerApiReadStatus.READ_SPEED_ZERO;
                }
            }

            return ad;
        }

        protected override void _Stop(MinerStopType willswitch)
        {
            Stop_cpu_ccminer_sgminer_nheqminer(willswitch);
        }

        protected override int GetMaxCooldownTimeInMilliseconds()
        {
            return 60 * 1000 * 5; // 5 minute max, whole waiting time 75seconds
        }

        // benchmark stuff

        protected override void BenchmarkOutputErrorDataReceivedImpl(string outdata)
        {
            CheckOutdata(outdata);
        }
    }
}

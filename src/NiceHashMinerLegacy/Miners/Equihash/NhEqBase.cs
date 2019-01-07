using Newtonsoft.Json;
using NiceHashMiner.Devices;
using NiceHashMiner.Miners.Grouping;
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
    public abstract class NhEqBase : Miner
    {
        protected MiningSetup CpuSetup = new MiningSetup(null);
        protected MiningSetup NvidiaSetup = new MiningSetup(null);
        protected readonly int AmdOclPlatform;
        protected MiningSetup AmdSetup = new MiningSetup(null);

        // extra benchmark stuff
        protected double CurSpeed = 0;

        protected static readonly string IterPerSec = "I/s";
        protected static readonly string SolsPerSec = "Sols/s";
        protected const double SolMultFactor = 1.9;

#pragma warning disable IDE1006
        private class Result
        {
            public double interval_seconds { get; set; }
            public double speed_ips { get; set; }
            public double speed_sps { get; set; }
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

        protected NhEqBase(string minerDeviceName)
            : base(minerDeviceName)
        {
            AmdOclPlatform = ComputeDeviceManager.Available.AmdOpenCLPlatformNum;
        }

        public override void InitMiningSetup(MiningSetup miningSetup)
        {
            base.InitMiningSetup(miningSetup);
            var cpus = new List<MiningPair>();
            var nvidias = new List<MiningPair>();
            var amds = new List<MiningPair>();
            foreach (var pairs in MiningSetup.MiningPairs)
            {
                switch (pairs.Device.DeviceType)
                {
                    case DeviceType.CPU:
                        cpus.Add(pairs);
                        break;
                    case DeviceType.NVIDIA:
                        nvidias.Add(pairs);
                        break;
                    case DeviceType.AMD:
                        amds.Add(pairs);
                        break;
                }
            }
            // reinit
            CpuSetup = new MiningSetup(cpus);
            NvidiaSetup = new MiningSetup(nvidias);
            AmdSetup = new MiningSetup(amds);
        }

        protected override string BenchmarkCreateCommandLine(Algorithm algorithm, int time)
        {
            // TODO nvidia extras
            var ret = "-b " + GetDevicesCommandString();
            return ret;
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
                ad.Speed = resp.result.speed_sps;
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

        protected double GetNumber(string outdata, string startF, string remF)
        {
            try
            {
                var speedStart = outdata.IndexOf(startF);
                var speed = outdata.Substring(speedStart, outdata.Length - speedStart);
                speed = speed.Replace(startF, "");
                speed = speed.Replace(remF, "");
                speed = speed.Trim();
                return double.Parse(speed, CultureInfo.InvariantCulture);
            }
            catch { }
            return 0;
        }

        // benchmark stuff

        protected override void BenchmarkOutputErrorDataReceivedImpl(string outdata)
        {
            CheckOutdata(outdata);
        }
    }
}

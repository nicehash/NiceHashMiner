using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using NiceHashMiner.Algorithms;
using NiceHashMiner.Configs;
using NiceHashMinerLegacy.Common.Enums;
using NiceHashMinerLegacy.Extensions;

namespace NiceHashMiner.Miners
{
    public class Ttminer : Miner
    {
        public Ttminer() : base("Ttminer")
        { }

        private DateTime _started;
        int _benchCount = 0;

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

            var elapsedSeconds = DateTime.Now.Subtract(_started).Seconds;
            if (elapsedSeconds < 15)
            {
                return ad;
            }

            JsonApiResponse resp = null;
            try
            {
                var bytesToSend = Encoding.ASCII.GetBytes("{\"id\":0,\"jsonrpc\":\"2.0\",\"method\":\"miner_getstat1\"}\n");
                using (var client = new TcpClient("127.0.0.1", ApiPort))
                using (var nwStream = client.GetStream())
                {
                    await nwStream.WriteAsync(bytesToSend, 0, bytesToSend.Length);
                    var bytesToRead = new byte[client.ReceiveBufferSize];
                    var bytesRead = await nwStream.ReadAsync(bytesToRead, 0, client.ReceiveBufferSize);
                    var respStr = Encoding.ASCII.GetString(bytesToRead, 0, bytesRead);
                    //Helpers.ConsolePrint(MinerTag(), "respStr: " + respStr);
                    resp = JsonConvert.DeserializeObject<JsonApiResponse>(respStr, Globals.JsonSettings);
                }
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
                    var speeds = resp.result[3].Split(';');
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

        public override void Start(string url, string btcAdress, string worker)
        {
            LastCommandLine = CreateCommandLine(url, btcAdress, worker);
            _started = DateTime.Now;
            ProcessHandle = _Start();
        }

        protected override string BenchmarkCreateCommandLine(Algorithm algorithm, int time)
        {
            var id = MiningSetup.MiningPairs.Select(pair => pair.Device.ID).FirstOrDefault();
            var url = Globals.GetLocationUrl(algorithm.NiceHashID, Globals.MiningLocation[ConfigManager.GeneralConfig.ServiceLocation],
                ConectionType);
            return CreateCommandLine(url, Globals.DemoUser, "benchmark");
        }

        protected override void BenchmarkOutputErrorDataReceivedImpl(string outdata)
        {
            CheckOutdata(outdata);
        }

        protected override bool BenchmarkParseLine(string outdata)
        {

            if (outdata.Contains("GPU[") && outdata.ToLower().TryGetHashrateAfter("]:", out var hashrate) && hashrate > 0)
            {
                ++_benchCount;
                if (_benchCount > 2) {
                    BenchmarkAlgorithm.BenchmarkSpeed = hashrate;
                    return true;
                }
            }
            Helpers.ConsolePrint(MinerTag(), $"BenchmarkParseLine {outdata}");

            return false;
        }

        protected override int GetMaxCooldownTimeInMilliseconds()
        {
            return 60 * 1000 * 3; // wait a little longer
        }

        protected override void _Stop(MinerStopType willswitch)
        {
            ShutdownMiner();
        }

        private string AlgoName
        {
            get
            {
                switch (MiningSetup.CurrentAlgorithmType)
                {
                    case AlgorithmType.MTP:
                        return "mtp";
                    case AlgorithmType.Lyra2REv3:
                        return "LYRA2V3";
                    default:
                        return "";
                }
            }
        }

        private string CreateCommandLine(string url, string btcAddress, string worker)
        {
            var devs = string.Join(" ", MiningSetup.MiningPairs.Select(pair => pair.Device.ID.ToString()));
            var cmd = $"-a {AlgoName} -url {url} " +
                              $"-u {btcAddress}.{worker} -d {devs} --api-bind 127.0.0.1:{ApiPort} ";

            // TODO
            //cmd += ExtraLaunchParametersParser.ParseForMiningSetup(MiningSetup, DeviceType.AMD);

            return cmd;
        }
    }
}

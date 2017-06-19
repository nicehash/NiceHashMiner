using Newtonsoft.Json;
using NiceHashMiner.Enums;
using NiceHashMiner.Miners.Grouping;
using NiceHashMiner.Miners.Parsing;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Sockets;
using System.Text;

namespace NiceHashMiner.Miners
{
    public class EWBF : Miner
    {

        private class Result
        {
            public uint gpuid { get; set; }
            public uint cudaid { get; set; }
            public string busid { get; set; }
            public uint gpu_status { get; set; }
            public uint solver { get; set; }
            public int temperature { get; set; }
            public uint gpu_power_usage { get; set; }
            public uint speed_sps { get; set; }
            public uint accepted_shares { get; set; }
            public uint rejected_shares { get; set; }
        }

        private class JsonApiResponse
        {
            public uint id { get; set; }
            public string method { get; set; }
            public object error { get; set; }
            public List<Result> result { get; set; }
        }

        public EWBF() : base("ewbf") {
            ConectionType = NHMConectionType.NONE;
            IsNeverHideMiningWindow = true;
        }

        public override void Start(string url, string btcAdress, string worker) {
            LastCommandLine = GetDevicesCommandString() + "--server " + url.Split(':')[0] + " --user " + btcAdress + "." + worker + " --pass x --port " + url.Split(':')[1] + " --api 0.0.0.0:" + APIPort;
            ProcessHandle = _Start();
        }

        protected override string GetDevicesCommandString() {
            string deviceStringCommand = " --cuda_devices ";
            foreach (var nvidia_pair in this.MiningSetup.MiningPairs) {
                deviceStringCommand += nvidia_pair.Device.ID + " ";

            }

            deviceStringCommand += " " + ExtraLaunchParametersParser.ParseForMiningSetup(MiningSetup, DeviceType.NVIDIA);

            return deviceStringCommand;
        }

        // benchmark stuff

        protected override string BenchmarkCreateCommandLine(Algorithm algorithm, int time) {
            string ret = " -b " + time + " --server equihash.eu.nicehash.com --port 3357 --user 39nV4iyAiW1JAjRqH1jRMwjaKy7TWqneiz --pass x" + " " + GetDevicesCommandString();
            return ret;
        }

        const string TOTAL_MES = "Total speed:";
        protected override bool BenchmarkParseLine(string outdata) {

            if (outdata.Contains(TOTAL_MES)) {
                try {
                    int speedStart = outdata.IndexOf(TOTAL_MES);
                    string speed = outdata.Substring(speedStart, outdata.Length - speedStart).Replace(TOTAL_MES, "");
                    var splitSrs = speed.Trim().Split(' ');
                    if (splitSrs.Length >= 2) {
                        string speedStr = splitSrs[0];
                        string postfixStr = splitSrs[1];
                        double spd = Double.Parse(speedStr, CultureInfo.InvariantCulture);
                        if (postfixStr.Contains("kSols/s"))
                            spd *= 1000;
                        else if (postfixStr.Contains("MSols/s"))
                            spd *= 1000000;
                        else if (postfixStr.Contains("GSols/s"))
                            spd *= 1000000000;

                        BenchmarkAlgorithm.BenchmarkSpeed = spd;
                        return true;
                    }
                } catch {
                }
            }
            return false;
        }

        public override APIData GetSummary() {
            _currentMinerReadStatus = MinerAPIReadStatus.NONE;
            APIData ad = new APIData(MiningSetup.CurrentAlgorithmType);

            TcpClient client = null;
            JsonApiResponse resp = null;
            try {
                byte[] bytesToSend = Encoding.ASCII.GetBytes("{\"method\":\"getstat\"}\n");
                client = new TcpClient("127.0.0.1", APIPort);
                NetworkStream nwStream = client.GetStream();
                nwStream.Write(bytesToSend, 0, bytesToSend.Length);
                byte[] bytesToRead = new byte[client.ReceiveBufferSize];
                int bytesRead = nwStream.Read(bytesToRead, 0, client.ReceiveBufferSize);
                string respStr = Encoding.ASCII.GetString(bytesToRead, 0, bytesRead);
                resp = JsonConvert.DeserializeObject<JsonApiResponse>(respStr, Globals.JsonSettings);
                client.Close();
            } catch (Exception ex) {
                Helpers.ConsolePrint("ERROR", ex.Message);
            }

            if (resp != null && resp.error == null) {
                ad.Speed = resp.result.Aggregate<Result, uint>(0, (current, t1) => current + t1.speed_sps);
                _currentMinerReadStatus = MinerAPIReadStatus.GOT_READ;
                if (ad.Speed == 0) {
                    _currentMinerReadStatus = MinerAPIReadStatus.READ_SPEED_ZERO;
                }
            }

            return ad;
        }

        protected override void _Stop(MinerStopType willswitch) {
            Stop_cpu_ccminer_sgminer_nheqminer(willswitch);
        }

        protected override int GET_MAX_CooldownTimeInMilliseconds() {
            return 60 * 1000 * 5; // 5 minute max, whole waiting time 75seconds
        }

        // benchmark stuff

        protected override void BenchmarkOutputErrorDataReceivedImpl(string outdata) {
            CheckOutdata(outdata);
        }
    }
}
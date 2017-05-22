using System;
using System.Collections.Generic;
using System.Text;
using System.Globalization;
using System.Net;
using System.Net.Sockets;
using System.Windows.Forms;
using System.Diagnostics;
using System.IO;
using NiceHashMiner.Configs;
using NiceHashMiner.Enums;
using NiceHashMiner.Devices;
using NiceHashMiner.Miners.Grouping;
using NiceHashMiner.Miners.Parsing;

namespace NiceHashMiner.Miners
{
    public class sia : Miner
    {
        public sia() : base("sia") { }

        protected override int GET_MAX_CooldownTimeInMilliseconds()
        {
            return 60 * 1000; // 1 minute max, whole waiting time 75seconds
        }

        public override void Start(string url, string btcAdress, string worker)
        {
            if (!IsInit)
            {
                Helpers.ConsolePrint(MinerTAG(), "MiningSetup is not initialized exiting Start()");
                return;
            }
            string username = GetUsername(btcAdress, worker);

            string algo = "-a " + MiningSetup.MinerName;
            string apiBind = " --api-bind=127.0.0.1:" + APIPort.ToString();

            LastCommandLine = algo + " --url=" + url + " -u " + username + apiBind;
            ProcessHandle = _Start();
        }

        protected override void _Stop(MinerStopType willswitch)
        {
            Stop_cpu_ccminer_sgminer_nheqminer(willswitch);
        }

        // new decoupled benchmarking routines
        #region Decoupled benchmarking routines

        protected override string BenchmarkCreateCommandLine(Algorithm algorithm, int time)
        {
            string timeLimit = " --time-limit " + time.ToString();
            string CommandLine = " --algo=" + algorithm.MinerName + " --benchmark" + timeLimit + " " + ExtraLaunchParametersParser.ParseForMiningSetup(MiningSetup, DeviceType.NVIDIA) + " --devices ";

            CommandLine += GetDevicesCommandString();
            return CommandLine;
        }

        protected override bool BenchmarkParseLine(string outdata)
        {
            double lastSpeed = BenchmarkParseLine_cpu_ccminer_extra(outdata);
            if (lastSpeed > 0.0d)
            {
                BenchmarkAlgorithm.BenchmarkSpeed = lastSpeed;
                return true;
            }

            if (double.TryParse(outdata, out lastSpeed))
            {
                BenchmarkAlgorithm.BenchmarkSpeed = lastSpeed;
                return true;
            }
            return false;
        }

        protected override void BenchmarkOutputErrorDataReceivedImpl(string outdata)
        {
            CheckOutdata(outdata);
        }

        #endregion // Decoupled benchmarking routines

        public override APIData GetSummary()
        {
            return GetSummaryCPU_CCMINER();
        }

    }
}

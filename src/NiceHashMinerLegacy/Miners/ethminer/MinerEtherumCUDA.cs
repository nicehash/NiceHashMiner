using NiceHashMiner.Miners.Parsing;
using System.Collections.Generic;
using NiceHashMiner.Algorithms;
using NiceHashMinerLegacy.Common.Enums;

namespace NiceHashMiner.Miners
{
    public class MinerEtherumCUDA : MinerEtherum
    {
        // reference to all MinerEtherumCUDA make sure to clear this after miner Stop
        // we make sure only ONE instance of MinerEtherumCUDA is running
        private static readonly List<MinerEtherum> MinerEtherumCudaList = new List<MinerEtherum>();

        public MinerEtherumCUDA()
            : base("MinerEtherumCUDA", "NVIDIA")
        {
            MinerEtherumCudaList.Add(this);
        }

        ~MinerEtherumCUDA()
        {
            // remove from list
            MinerEtherumCudaList.Remove(this);
        }

        public override void Start(string url, string btcAdress, string worker)
        {
            Helpers.ConsolePrint(MinerTag(), "Starting MinerEtherumCUDA, checking existing MinerEtherumCUDA to stop");
            base.Start(url, btcAdress, worker, MinerEtherumCudaList);
        }

        protected override string GetStartCommandStringPart(string url, string username)
        {
            return " --cuda"
                   + " "
                   + ExtraLaunchParametersParser.ParseForMiningSetup(
                       MiningSetup,
                       DeviceType.NVIDIA)
                   + " -S " + url.Substring(14)
                   + " -O " + username + ":x "
                   + " --api-port " + ApiPort
                   + " --cuda-devices ";
        }

        protected override string GetBenchmarkCommandStringPart(Algorithm algorithm)
        {
            return " --benchmark-warmup 40 --benchmark-trial 20"
                   + " "
                   + ExtraLaunchParametersParser.ParseForMiningSetup(
                       MiningSetup,
                       DeviceType.NVIDIA)
                   + " --cuda --cuda-devices ";
        }
    }
}

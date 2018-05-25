using NiceHashMiner.Devices;
using NiceHashMiner.Miners.Parsing;
using System.Collections.Generic;
using NiceHashMiner.Algorithms;
using NiceHashMinerLegacy.Common.Enums;

namespace NiceHashMiner.Miners
{
    // TODO for NOW ONLY AMD
    // AMD or TODO it could be something else
    public class MinerEtherumOCL : MinerEtherum
    {
        // reference to all MinerEtherumOCL make sure to clear this after miner Stop
        // we make sure only ONE instance of MinerEtherumOCL is running
        private static readonly List<MinerEtherum> MinerEtherumOclList = new List<MinerEtherum>();

        private readonly int _gpuPlatformNumber;

        public MinerEtherumOCL()
            : base("MinerEtherumOCL", "AMD OpenCL")
        {
            _gpuPlatformNumber = ComputeDeviceManager.Available.AmdOpenCLPlatformNum;
            MinerEtherumOclList.Add(this);
        }

        ~MinerEtherumOCL()
        {
            // remove from list
            MinerEtherumOclList.Remove(this);
        }

        public override void Start(string url, string btcAdress, string worker)
        {
            Helpers.ConsolePrint(MinerTag(), "Starting MinerEtherumOCL, checking existing MinerEtherumOCL to stop");
            base.Start(url, btcAdress, worker, MinerEtherumOclList);
        }

        protected override string GetStartCommandStringPart(string url, string username)
        {
            return " --opencl --opencl-platform " + _gpuPlatformNumber
                   + " "
                   + ExtraLaunchParametersParser.ParseForMiningSetup(
                       MiningSetup,
                       DeviceType.AMD)
                   + " -S " + url.Substring(14)
                   + " -O " + username + ":x "
                   + " --api-port " + ApiPort
                   + " --opencl-devices ";
        }

        protected override string GetBenchmarkCommandStringPart(Algorithm algorithm)
        {
            return " --opencl --opencl-platform " + _gpuPlatformNumber
                   + " "
                   + ExtraLaunchParametersParser.ParseForMiningSetup(
                       MiningSetup,
                       DeviceType.AMD)
                   + " --benchmark-warmup 40 --benchmark-trial 20"
                   + " --opencl-devices ";
        }
    }
}

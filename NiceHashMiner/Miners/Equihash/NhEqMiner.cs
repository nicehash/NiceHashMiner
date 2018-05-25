using NiceHashMiner.Miners.Parsing;
using System.Diagnostics;
using System.Linq;
using NiceHashMinerLegacy.Common.Enums;

namespace NiceHashMiner.Miners
{
    public class NhEqMiner : NhEqBase
    {
        public NhEqMiner()
            : base("NhEqMiner")
        {
            ConectionType = NhmConectionType.NONE;
        }

        // CPU aff set from NHM
        protected override NiceHashProcess _Start()
        {
            var P = base._Start();
            if (CpuSetup.IsInit && P != null)
            {
                var affinityMask = CpuSetup.MiningPairs[0].Device.AffinityMask;
                if (affinityMask != 0)
                {
                    CpuID.AdjustAffinity(P.Id, affinityMask);
                }
            }

            return P;
        }

        public override void Start(string url, string btcAdress, string worker)
        {
            var username = GetUsername(btcAdress, worker);
            LastCommandLine = GetDevicesCommandString() + " -a " + ApiPort + " -l " + url + " -u " + username;
            ProcessHandle = _Start();
        }


        protected override string GetDevicesCommandString()
        {
            var deviceStringCommand = " ";

            if (CpuSetup.IsInit)
            {
                deviceStringCommand += " " + ExtraLaunchParametersParser.ParseForMiningSetup(CpuSetup, DeviceType.CPU);
            }
            else
            {
                // disable CPU
                deviceStringCommand += " -t 0 ";
            }

            if (NvidiaSetup.IsInit)
            {
                deviceStringCommand += " -cd ";
                deviceStringCommand = NvidiaSetup.MiningPairs.Aggregate(deviceStringCommand,
                    (current, nvidiaPair) => current + (nvidiaPair.Device.ID + " "));
                deviceStringCommand +=
                    " " + ExtraLaunchParametersParser.ParseForMiningSetup(NvidiaSetup, DeviceType.NVIDIA);
            }

            if (AmdSetup.IsInit)
            {
                deviceStringCommand += " -op " + AmdOclPlatform.ToString();
                deviceStringCommand += " -od ";
                deviceStringCommand = AmdSetup.MiningPairs.Aggregate(deviceStringCommand,
                    (current, amdPair) => current + (amdPair.Device.ID + " "));
                deviceStringCommand += " " + ExtraLaunchParametersParser.ParseForMiningSetup(AmdSetup, DeviceType.AMD);
            }

            return deviceStringCommand;
        }

        // benchmark stuff
        protected override Process BenchmarkStartProcess(string commandLine)
        {
            var benchmarkHandle = base.BenchmarkStartProcess(commandLine);

            if (CpuSetup.IsInit && benchmarkHandle != null)
            {
                var affinityMask = CpuSetup.MiningPairs[0].Device.AffinityMask;
                if (affinityMask != 0)
                {
                    CpuID.AdjustAffinity(benchmarkHandle.Id, affinityMask);
                }
            }

            return benchmarkHandle;
        }

        protected override bool BenchmarkParseLine(string outdata)
        {
            if (outdata.Contains(IterPerSec))
            {
                CurSpeed = GetNumber(outdata, "Speed: ", IterPerSec) * SolMultFactor;
            }
            if (outdata.Contains(SolsPerSec))
            {
                var sols = GetNumber(outdata, "Speed: ", SolsPerSec);
                if (sols > 0)
                {
                    BenchmarkAlgorithm.BenchmarkSpeed = CurSpeed;
                    return true;
                }
            }
            return false;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Text;
using NiceHashMiner.Miners.Grouping;
using NiceHashMiner.Miners.Parsing;
using NiceHashMinerLegacy.Common.Enums;

// Resharper disable All
#pragma warning disable

namespace NiceHashMiner.Miners {
    public class eqm : NhEqBase {
        public eqm()
            : base("eqm") {
            ConectionType = NhmConectionType.LOCKED;
            IsNeverHideMiningWindow = true;
        }

        public override void Start(string url, string btcAdress, string worker) {
            LastCommandLine = GetDevicesCommandString() + " -a " + ApiPort + " -l " + url + " -u " + btcAdress + " -w " + worker;
            ProcessHandle = _Start();
        }


        protected override string GetDevicesCommandString() {
            string deviceStringCommand = " ";

            if (CpuSetup.IsInit) {
                deviceStringCommand += "-p " + CpuSetup.MiningPairs.Count;
                deviceStringCommand += " " + ExtraLaunchParametersParser.ParseForMiningSetup(CpuSetup, DeviceType.CPU);
            } else {
                // disable CPU
                deviceStringCommand += " -t 0 ";
            }

            if (NvidiaSetup.IsInit) {
                deviceStringCommand += " -cd ";
                foreach (var nvidia_pair in NvidiaSetup.MiningPairs) {
                    if (nvidia_pair.CurrentExtraLaunchParameters.Contains("-ct")) {
                        for (int i = 0; i < ExtraLaunchParametersParser.GetEqmCudaThreadCount(nvidia_pair); ++i) {
                            deviceStringCommand += nvidia_pair.Device.ID + " ";
                        }
                    } else { // use default 2 best performance
                        for (int i = 0; i < 2; ++i) {
                            deviceStringCommand += nvidia_pair.Device.ID + " ";
                        }
                    }
                }
                // no extra launch params
                deviceStringCommand += " " + ExtraLaunchParametersParser.ParseForMiningSetup(NvidiaSetup, DeviceType.NVIDIA);
            }

            return deviceStringCommand;
        }

        // benchmark stuff
        const string TOTAL_MES = "Total measured:";
        protected override bool BenchmarkParseLine(string outdata) {

            if (outdata.Contains(TOTAL_MES) && outdata.Contains(IterPerSec)) {
                CurSpeed = GetNumber(outdata, TOTAL_MES, IterPerSec) * SolMultFactor;
            }
            if (outdata.Contains(TOTAL_MES) && outdata.Contains(SolsPerSec)) {
                var sols = GetNumber(outdata, TOTAL_MES, SolsPerSec);
                if (sols > 0) {
                    BenchmarkAlgorithm.BenchmarkSpeed = CurSpeed;
                    return true;
                }
            }
            return false;
        }
    }
}

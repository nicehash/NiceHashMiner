using NiceHashMiner.Configs;
using NiceHashMiner.Enums;
using NiceHashMiner.Miners.Grouping;
using NiceHashMiner.Miners.Parsing;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace NiceHashMiner.Miners {
    public class ClaymoreCryptoNightMiner : ClaymoreBaseMiner
    {

        private bool isOld;

        const string _LOOK_FOR_START = "XMR - Total Speed:";
        const string _LOOK_FOR_START_OLD = "hashrate =";
        public ClaymoreCryptoNightMiner(bool isOld = false)
            : base("ClaymoreCryptoNightMiner", isOld ? _LOOK_FOR_START_OLD : _LOOK_FOR_START) {
            this.isOld = isOld;
        }

        protected override double DevFee() {
            return isOld ? 2.0 : 1.0;
        }
        
        protected override string GetDevicesCommandString() {
            if (!isOld) return base.GetDevicesCommandString();

            string extraParams = ExtraLaunchParametersParser.ParseForMiningSetup(MiningSetup, DeviceType.AMD);
            string deviceStringCommand = " -di ";
            List<string> ids = new List<string>();
            foreach (var mPair in MiningSetup.MiningPairs) {
                var id = mPair.Device.ID;
                ids.Add(id.ToString());
            }
            deviceStringCommand += String.Join("", ids);

            return deviceStringCommand + extraParams;
        }

        public override void Start(string url, string btcAdress, string worker) {
            string username = GetUsername(btcAdress, worker);
            if (isOld) {
                LastCommandLine = " " + GetDevicesCommandString() + " -mport -" + APIPort + " -o " + url + " -u " +
                                  username + " -p " + worker + " -dbg -1";
            } else {
                LastCommandLine = " " + GetDevicesCommandString() + " -mport -" + APIPort + " -xpool " + url +
                                  " -xwal " + username + " -xpsw " + worker + " -dbg -1";
            }
            ProcessHandle = _Start();
        }

        // benchmark stuff

        protected override string BenchmarkCreateCommandLine(Algorithm algorithm, int time) {
            benchmarkTimeWait = time; // Takes longer as of v10

            // network workaround
            string url = Globals.GetLocationURL(algorithm.NiceHashID, Globals.MiningLocation[ConfigManager.GeneralConfig.ServiceLocation], this.ConectionType);
            // demo for benchmark
            string username = Globals.DemoUser;
            if (ConfigManager.GeneralConfig.WorkerName.Length > 0)
                username += "." + ConfigManager.GeneralConfig.WorkerName.Trim();
            string ret;
            if (isOld) {
                ret = " " + GetDevicesCommandString() + " -mport -" + APIPort + " -o " + url + " -u " + username +
                      " -p x";
            } else {
                ret = " " + GetDevicesCommandString() + " -mport -" + APIPort + " -xpool " + url + " -xwal " +
                             username + " -xpsw x";
            }
            return ret;
        }

    }
}

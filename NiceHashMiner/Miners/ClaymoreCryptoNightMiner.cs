using NiceHashMiner.Configs;
using NiceHashMiner.Enums;
using NiceHashMiner.Miners.Grouping;
using NiceHashMiner.Miners.Parsing;
using System.Linq;

namespace NiceHashMiner.Miners
{
    public class ClaymoreCryptoNightMiner : ClaymoreBaseMiner
    {
        private readonly bool _isOld;

        private const string _LookForStart = "XMR - Total Speed:";
        private const string LookForStartOld = "hashrate =";

        public ClaymoreCryptoNightMiner(bool isOld = false)
            : base("ClaymoreCryptoNightMiner", isOld ? LookForStartOld : _LookForStart)
        {
            _isOld = isOld;
        }

        protected override double DevFee()
        {
            return _isOld ? 2.0 : 1.0;
        }

        protected override string GetDevicesCommandString()
        {
            if (!_isOld) return base.GetDevicesCommandString();

            var extraParams = ExtraLaunchParametersParser.ParseForMiningSetup(MiningSetup, DeviceType.AMD);
            var deviceStringCommand = " -di ";
            var ids = MiningSetup.MiningPairs.Select(mPair => mPair.Device.ID).Select(id => id.ToString()).ToList();
            deviceStringCommand += string.Join("", ids);

            return deviceStringCommand + extraParams;
        }

        public override void Start(string url, string btcAdress, string worker)
        {
            var username = GetUsername(btcAdress, worker);
            if (_isOld)
            {
                LastCommandLine = " " + GetDevicesCommandString() + " -mport -" + ApiPort + " -o " + url +
                                  " -u " + username + " -p x -dbg -1";
            }
            else
            {
                LastCommandLine = " " + GetDevicesCommandString() + " -mport 127.0.0.1:-" + ApiPort + " -xpool " +
                                  url + " -xwal " + username + " -xpsw x -dbg -1";
            }
            ProcessHandle = _Start();
        }

        // benchmark stuff

        protected override string BenchmarkCreateCommandLine(Algorithm algorithm, int time)
        {
            BenchmarkTimeWait = time; // Takes longer as of v10

            // network workaround
            var url = Globals.GetLocationUrl(algorithm.NiceHashID, Globals.MiningLocation[ConfigManager.GeneralConfig.ServiceLocation],
                ConectionType);
            // demo for benchmark
            var username = Globals.DemoUser;
            if (ConfigManager.GeneralConfig.WorkerName.Length > 0)
                username += "." + ConfigManager.GeneralConfig.WorkerName.Trim();
            string ret;
            if (_isOld)
            {
                ret = " " + GetDevicesCommandString() + " -mport -" + ApiPort + " -o " + url + " -u " + username +
                      " -p x";
            }
            else
            {
                ret = " " + GetDevicesCommandString() + " -mport -" + ApiPort + " -xpool " + url + " -xwal " +
                      username + " -xpsw x";
            }
            return ret;
        }
    }
}

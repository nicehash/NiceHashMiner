using NiceHashMiner.Algorithms;

namespace NiceHashMiner.Miners
{
    public class ClaymoreZcashMiner : ClaymoreBaseMiner
    {
        public ClaymoreZcashMiner()
            : base("ClaymoreZcashMiner")
        {
            IgnoreZero = true;
            LookForStart = "zec - total speed:";
            DevFee = 2.0;
        }


        public override void Start(string url, string btcAdress, string worker)
        {
            var username = GetUsername(btcAdress, worker);
            LastCommandLine = " " + GetDevicesCommandString() + " -mport 127.0.0.1:-" + ApiPort + " -zpool " + url +
                              " -zwal " + username + " -zpsw x -dbg -1";
            ProcessHandle = _Start();
        }

        // benchmark stuff
        protected override string BenchmarkCreateCommandLine(Algorithm algorithm, int time)
        {
            BenchmarkTimeWait = time / 3; // 3 times faster than sgminer

            return $" -mport 127.0.0.1:{ApiPort} -benchmark 1 -logfile {GetLogFileName()} {GetDevicesCommandString()}";
        }
    }
}

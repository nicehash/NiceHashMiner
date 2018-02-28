namespace NiceHashMiner.Miners
{
    public class ClaymoreZcashMiner : ClaymoreBaseMiner
    {
        private const string LookForStart = "ZEC - Total Speed:";

        public ClaymoreZcashMiner()
            : base("ClaymoreZcashMiner", LookForStart)
        {
            IgnoreZero = true;
        }

        protected override double DevFee()
        {
            return 2.0;
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

            string ret =  $" -mport 127.0.0.1:{APIPort} -benchmark 1 -logfile {GetLogFileName()} {GetDevicesCommandString()}";
            return ret;
        }
    }
}


namespace NiceHashMiner.Stats.Models
{
    internal class ExecutedInfo
    {
        public string NewWorker = null;
        public string NewBtc = null;
        public string NewRig = null;

        public bool LoginNeeded => NewWorker != null || NewBtc != null || NewRig != null;
    }
}

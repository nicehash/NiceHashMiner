using NiceHashMinerLegacy.Common.Enums;

namespace NiceHashMiner.Miners.Grouping
{
    public static class GroupingLogic
    {
        public static bool ShouldGroup(MiningPair a, MiningPair b)
        {
            // group if same bin path and same algo type
            if (IsSameBinPath(a, b) && IsSameAlgorithmType(a, b) &&
                ((IsNotCpuGroups(a, b) && IsSameDeviceType(a, b)) ||
                 (a.Algorithm.MinerBaseType == MinerBaseType.Prospector &&
                 b.Algorithm.MinerBaseType == MinerBaseType.Prospector) ||
                 a.Algorithm.MinerBaseType == MinerBaseType.XmrStak || a.Algorithm.MinerBaseType == MinerBaseType.GMiner)) 
                return true;

            return false;
        }

        private static bool IsNotCpuGroups(MiningPair a, MiningPair b)
        {
            return a.Device.DeviceType != DeviceType.CPU && b.Device.DeviceType != DeviceType.CPU;
        }

        private static bool IsSameBinPath(MiningPair a, MiningPair b)
        {
            return a.Algorithm.MinerBinaryPath == b.Algorithm.MinerBinaryPath;
        }
        private static bool IsSameAlgorithmType(MiningPair a, MiningPair b) 
        {
            return a.Algorithm.DualNiceHashID == b.Algorithm.DualNiceHashID;
        }

        private static bool IsSameDeviceType(MiningPair a, MiningPair b)
        {
            return a.Device.DeviceType == b.Device.DeviceType;
        }
    }
}

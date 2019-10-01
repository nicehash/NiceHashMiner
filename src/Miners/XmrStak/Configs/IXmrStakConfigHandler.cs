using NHM.Common.Enums;

namespace XmrStak.Configs
{
    public interface IXmrStakConfigHandler
    {
        bool HasConfig(DeviceType deviceType, AlgorithmType algorithmType);

        void SaveMoveConfig(DeviceType deviceType, AlgorithmType algorithmType, string sourcePath);

        CpuConfig GetCpuConfig(AlgorithmType algorithmType);
        AmdConfig GetAmdConfig(AlgorithmType algorithmType);
        NvidiaConfig GetNvidiaConfig(AlgorithmType algorithmType);
    }
}

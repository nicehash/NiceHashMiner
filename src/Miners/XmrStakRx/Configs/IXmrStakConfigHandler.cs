using NHM.Common.Enums;

namespace XmrStakRx.Configs
{
    public interface IXmrStakRxConfigHandler
    {
        bool HasConfig(DeviceType deviceType, AlgorithmType algorithmType);

        void SaveMoveConfig(DeviceType deviceType, AlgorithmType algorithmType, string sourcePath);

        CpuConfig GetCpuConfig(AlgorithmType algorithmType);
        AmdConfig GetAmdConfig(AlgorithmType algorithmType);
        NvidiaConfig GetNvidiaConfig(AlgorithmType algorithmType);
    }
}

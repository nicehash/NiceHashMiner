using NHM.Common.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

using NHM.Common.Enums;

namespace NHM.DeviceMonitoring
{
    public interface ISetPowerLevel
    {
        bool SetPowerTarget(PowerLevel level);
    }
}

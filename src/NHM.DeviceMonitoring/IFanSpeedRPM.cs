
namespace NHM.DeviceMonitoring
{
    public interface IFanSpeedRPM
    {
        int FanSpeedRPM { get; }

        bool SetFanSpeedPercentage(int percentage);
    }
}

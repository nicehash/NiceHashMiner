namespace NHM.DeviceMonitoring
{
    public interface IGetFanSpeedPercentage
    {
        (int status, int percentage) GetFanSpeedPercentage();
    }
}

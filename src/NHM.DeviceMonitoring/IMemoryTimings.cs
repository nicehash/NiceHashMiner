namespace NHM.DeviceMonitoring
{
    public interface IMemoryTimings
    {
        int SetMemoryTimings(string mt);
        int ResetMemoryTimings();
        void PrintMemoryTimings();
    }
}

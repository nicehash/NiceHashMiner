
namespace NHM.Common.Device
{
    public interface IGpuDevice
    {
        string UUID { get; }
        int PCIeBusID { get; }
        ulong GpuRam { get; }
        // on AMD must be always true while on NVIDIA it depends
        bool IsOpenCLBackendEnabled { get; }
    }
}

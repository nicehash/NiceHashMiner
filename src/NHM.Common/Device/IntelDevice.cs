using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NHM.Common.Device
{
    public class IntelDevice : BaseDevice, IGpuDevice
    {
        public static string RawDetectionOutput = string.Empty;

        // TODO does it make sense to set here the actual installed NVIDIA drivers??
        public Version DEVICE_INTEL_DRIVER = new Version(0, 0); // or use just null

        public int PCIeBusID { get; init; }
        public ulong GpuRam { get; init; }

        // we assume disabled and we check it after OpenCL detection.
        public bool IsOpenCLBackendEnabled { get; private set; } = false;
        public string RawDeviceData { get; init; }
    }
}

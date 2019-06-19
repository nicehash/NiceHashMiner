using NHM.DeviceDetection.WMI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NiceHashMinerLegacy.Common.Device;

namespace NHM.DeviceDetection
{
    public class DeviceDetectionResult
    {
        // WMI VideoControllers
        public IReadOnlyList<VideoControllerData> AvailableVideoControllers { get; internal set; }
        public Version NvidiaDriverWMI { get; internal set; }

        // CPU
        public CPUDevice CPU { get; internal set; }

        // NVIDIA
        public IReadOnlyList<CUDADevice> CUDADevices { get; internal set; }
        public bool IsDCHDriver { get; internal set; }
        public bool IsNvmlFallback { get; internal set; }
        public Version NvidiaDriver { get; internal set; }

        // AMD
        public IReadOnlyList<AMDDevice> AMDDevices { get; internal set; }
    }
}

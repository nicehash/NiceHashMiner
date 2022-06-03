using NHM.Common.Device;
using NHM.DeviceDetection.WMI;
using System;
using System.Collections.Generic;

namespace NHM.DeviceDetection
{
    public class DeviceDetectionResult : IEquatable<DeviceDetectionResult>
    {
        // WMI VideoControllers
        public IReadOnlyList<VideoControllerData> AvailableVideoControllers { get; internal set; }
        public Version NvidiaDriverWMI { get; internal set; }

        // CPU
        public CPUDevice CPU { get; internal set; }

        // NVIDIA
        public IReadOnlyList<CUDADevice> CUDADevices { get; internal set; }
        public bool HasCUDADevices => CUDADevices != null && CUDADevices.Count > 0;
        public bool IsDCHDriver { get; internal set; }
        public Version NvidiaDriver { get; internal set; }
        public bool NVIDIADriverObsolete { get; internal set; }


        public bool IsNvidiaNVMLLoadedError { get; internal set; }
        public bool IsNvidiaNVMLInitializedError { get; internal set; }
        public IReadOnlyList<CUDADevice> UnsupportedCUDADevices { get; internal set; }

        // AMD
        public IReadOnlyList<AMDDevice> AMDDevices { get; internal set; }
        public bool HasAMDDevices => AMDDevices != null && AMDDevices.Count > 0;
        public bool IsOpenClFallback { get; internal set; }
        public Version AmdDriver { get; internal set; }
        public bool AMDDriverObsolete { get; internal set; }



        // FAKE
        public IReadOnlyList<FakeDevice> FAKEDevices { get; internal set; }
        public bool Equals(DeviceDetectionResult other)
        {
            if(AvailableVideoControllers != null && other.AvailableVideoControllers != null)
            {
                if (AvailableVideoControllers.Count != other.AvailableVideoControllers.Count) return false;
                for (int i = 0; i < AvailableVideoControllers.Count; i++)
                {
                    if (AvailableVideoControllers[i] != other.AvailableVideoControllers[i]) return false;
                }
            }
            if (AvailableVideoControllers != null && other.AvailableVideoControllers == null ||
                AvailableVideoControllers == null && other.AvailableVideoControllers != null) return false;
            if (NvidiaDriverWMI != other.NvidiaDriverWMI) return false;
            if (!CPU.Equals(other.CPU)) return false;
            if (CUDADevices != null && other.CUDADevices != null)
            {
                if (CUDADevices.Count != other.CUDADevices.Count) return false;
                for (int i = 0; i < CUDADevices.Count; i++)
                {
                    if (!CUDADevices[i].Equals(other.CUDADevices[i])) return false;
                }
            }
            if (CUDADevices != null && other.CUDADevices == null ||
                CUDADevices == null && other.CUDADevices != null) return false;
            if (IsDCHDriver != other.IsDCHDriver) return false;
            if (NvidiaDriver != other.NvidiaDriver) return false;
            if (NVIDIADriverObsolete != other.NVIDIADriverObsolete) return false;
            if (IsNvidiaNVMLLoadedError != other.IsNvidiaNVMLLoadedError) return false;
            if (IsNvidiaNVMLInitializedError != other.IsNvidiaNVMLInitializedError) return false;
            if(UnsupportedCUDADevices != null && other.UnsupportedCUDADevices != null)
            {
                if (UnsupportedCUDADevices.Count != other.UnsupportedCUDADevices.Count) return false;
                for (int i = 0; i < UnsupportedCUDADevices.Count; i++)
                {
                    if (!UnsupportedCUDADevices[i].Equals(other.UnsupportedCUDADevices[i])) return false;
                }
            }
            if (UnsupportedCUDADevices == null && other.UnsupportedCUDADevices != null ||
                UnsupportedCUDADevices != null && other.UnsupportedCUDADevices == null) return false;
            if(AMDDevices != null && other.AMDDevices != null)
            {
                if (AMDDevices.Count != other.AMDDevices.Count) return false;
                for (int i = 0; i < AMDDevices.Count; i++)
                {
                    if (!AMDDevices[i].Equals(other.AMDDevices[i])) return false;
                }
            }
            if (AMDDevices == null && other.AMDDevices != null ||
                AMDDevices != null && other.AMDDevices == null) return false;
            if (IsOpenClFallback != other.IsOpenClFallback) return false;
            if (AmdDriver != other.AmdDriver) return false;
            if (AMDDriverObsolete != other.AMDDriverObsolete) return false;
            if(FAKEDevices != null && other.FAKEDevices != null)
            {
                if (FAKEDevices.Count != other.FAKEDevices.Count) return false;
                for (int i = 0; i < FAKEDevices.Count; i++)
                {
                    if (!FAKEDevices[i].Equals(other.FAKEDevices[i])) return false;
                }
            }
            if (FAKEDevices == null && other.FAKEDevices != null ||
                FAKEDevices != null && other.FAKEDevices == null) return false;
            return true;
        }
    }
}

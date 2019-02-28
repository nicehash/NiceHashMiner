using NiceHashMiner.Devices.Querying.Amd;
using System;
using NiceHashMiner.Devices.Querying.Amd.OpenCL;

namespace NiceHashMiner.Devices
{
    [Serializable]
    public class AmdGpuDevice
    {
        public const string DefaultParam = "--keccak-unroll 0 --hamsi-expand-big 4 --remove-disabled  ";
        public const string TemperatureParam = " --gpu-fan 30-95 --temp-cutoff 95 --temp-overheat 90 " + " --temp-target 75 --auto-fan --auto-gpu ";

        public string DeviceName { get; } // init this with the ADL
        public string Uuid { get; } // init this with the ADL, use PCI_VEN & DEV IDs

        //public bool UseOptimizedVersion { get; private set; }
        private readonly OpenCLDevice _openClSubset;

        public string InfSection { get; } // has arhitecture string

        // new drivers make some algorithms unusable 21.19.164.1 => driver not working with NeoScrypt and 
        public bool DriverDisableAlgos { get; }

        public int Adl1Index { get; } // init this with the ADL
        public int Adl2Index { get; }

        public ulong DeviceGlobalMemory => _openClSubset._CL_DEVICE_GLOBAL_MEM_SIZE;
        public bool IsEtherumCapable => DeviceGlobalMemory >= ComputeDevice.Memory3Gb;

        public int DeviceID => (int) _openClSubset.DeviceID;
        public int BusID => _openClSubset.AMD_BUS_ID;

        public string Codename => _openClSubset._CL_DEVICE_NAME;

        internal AmdGpuDevice(OpenCLDevice openClSubset, string infSection, bool driverDisableAlgo, string name, string uuid)
        {
            DriverDisableAlgos = driverDisableAlgo;
            InfSection = infSection;

            _openClSubset = openClSubset ?? new OpenCLDevice();

            DeviceName = name;
            Uuid = uuid;

            // Check for optimized version
            // first if not optimized
            Helpers.ConsolePrint("AmdGpuDevice", "List: " + _openClSubset._CL_DEVICE_NAME);
        }

        internal AmdGpuDevice(OpenCLDevice openClSubset, bool driverDisableAlgo, string name, AmdBusIDInfo busIdInfo)
            : this(openClSubset, busIdInfo.InfSection, driverDisableAlgo, name, busIdInfo.Uuid)
        {
            Adl1Index = busIdInfo.Adl1Index;
            Adl2Index = busIdInfo.Adl2Index;
        }
    }
}

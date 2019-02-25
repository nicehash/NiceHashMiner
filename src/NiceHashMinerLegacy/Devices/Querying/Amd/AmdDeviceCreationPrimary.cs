using NiceHashMiner.Devices.OpenCL;
using System.Collections.Generic;

namespace NiceHashMiner.Devices.Querying.Amd
{
    internal class AmdDeviceCreationPrimary : AmdDeviceCreation
    {
        private readonly IReadOnlyDictionary<int, QueryAdl.BusIdInfo> _busIDInfos;

        protected override bool IsFallback => false;

        public AmdDeviceCreationPrimary(IReadOnlyDictionary<int, QueryAdl.BusIdInfo> busIDInfos)
        {
            _busIDInfos = busIDInfos;
        }

        protected override IEnumerable<AmdGpuDevice> CreateGpuDevices(List<OpenCLDevice> amdDevices,
            Dictionary<string, bool> disableAlgos)
        {
            Helpers.ConsolePrint(Tag, "Using AMD device creation DEFAULT Reliable mappings");
            foreach (var dev in amdDevices)
            {
                var busID = dev.AMD_BUS_ID;
                if (busID != -1 && _busIDInfos.ContainsKey(busID))
                {
                    var deviceName = _busIDInfos[busID].Name;
                    var newAmdDev = new AmdGpuDevice(dev,
                        _busIDInfos[busID].InfSection, disableAlgos[deviceName])
                    {
                        DeviceName = deviceName,
                        UUID = _busIDInfos[busID].Uuid,
                        AdapterIndex = _busIDInfos[busID].Adl1Index
                    };

                    yield return newAmdDev;
                }
                else
                {
                    Helpers.ConsolePrint(Tag, $"\tDevice not added, Bus No. {busID} not found:");
                }
            }
        }
    }
}

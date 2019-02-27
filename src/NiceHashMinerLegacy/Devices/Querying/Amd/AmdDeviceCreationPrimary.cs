using System.Collections.Generic;
using NiceHashMiner.Devices.Querying.Amd.OpenCL;

namespace NiceHashMiner.Devices.Querying.Amd
{
    internal class AmdDeviceCreationPrimary : AmdDeviceCreation
    {
        private readonly IReadOnlyDictionary<int, AmdBusIDInfo> _busIDInfos;

        protected override bool IsFallback => false;

        public AmdDeviceCreationPrimary(IReadOnlyDictionary<int, AmdBusIDInfo> busIDInfos)
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
                    disableAlgos.TryGetValue(deviceName, out var disableAlgo);
                    var newAmdDev = new AmdGpuDevice(dev,
                        disableAlgo,
                        deviceName,
                        _busIDInfos[busID]);

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

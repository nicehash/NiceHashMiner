using System;
using System.Collections.Generic;
using System.Linq;
using NiceHashMiner.Devices.Querying.Amd.OpenCL;

namespace NiceHashMiner.Devices.Querying.Amd
{
    internal class AmdDeviceCreationFallback : AmdDeviceCreation
    {
        protected override bool IsFallback => true;

        protected override IEnumerable<AmdGpuDevice> CreateGpuDevices(List<OpenCLDevice> amdDevices, Dictionary<string, bool> disableAlgos)
        {
            Helpers.ConsolePrint(Tag, "Using AMD device creation FALLBACK UnReliable mappings");

            // get video AMD controllers and sort them by RAM
            // (find a way to get PCI BUS Numbers from PNPDeviceID)
            var amdVideoControllers = SystemSpecs.AvailableVideoControllers.Where(vcd => vcd.IsAmd).ToList();
            // sort by ram not ideal 
            amdVideoControllers.Sort((a, b) => (int)(a.AdapterRam - b.AdapterRam));
            amdDevices.Sort((a, b) =>
                (int)(a._CL_DEVICE_GLOBAL_MEM_SIZE - b._CL_DEVICE_GLOBAL_MEM_SIZE));
            var minCount = Math.Min(amdVideoControllers.Count, amdDevices.Count);

            for (var i = 0; i < minCount; ++i)
            {
                var deviceName = amdVideoControllers[i].Name;
                amdVideoControllers[i].SetInfSectionEmptyIfNull();
                var newAmdDev = new AmdGpuDevice(amdDevices[i], 
                    amdVideoControllers[i].InfSection,
                    disableAlgos[deviceName],
                    deviceName,
                    "UNUSED");

                yield return newAmdDev;
            }
        }
    }
}

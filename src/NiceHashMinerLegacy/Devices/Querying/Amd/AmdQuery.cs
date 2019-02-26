using System;
using System.Collections.Generic;
using System.Linq;
using NiceHashMiner.Configs;
using NiceHashMiner.Devices.Querying.Amd.OpenCL;

namespace NiceHashMiner.Devices.Querying.Amd
{
    internal class AmdQuery : QueryGpu
    {
        private const string Tag = "AmdQuery";
        
        private readonly Dictionary<string, bool> _noNeoscryptLyra2 = new Dictionary<string, bool>();

        private readonly int _numDevs;

        private bool _openCLSuccess;
        private OpenCLDeviceDetectionResult _openCLResult;

        protected QueryOpenCL OclQuery;
        protected QueryAdl AdlQuery;

        public AmdQuery(int numCudaDevs)
        {
            _numDevs = numCudaDevs;
            OclQuery = new QueryOpenCL();
            AdlQuery = new QueryAdl();
        }

        public void QueryOpenCLDevices()
        {
            _openCLSuccess = OclQuery.TryQueryOpenCLDevices(out _openCLResult);
        }

        public List<AmdComputeDevice> QueryAmd(out bool failedDriverCheck)
        {
            Helpers.ConsolePrint(Tag, "QueryAMD START");

            failedDriverCheck = DriverCheck();

            var amdDevices = _openCLSuccess ? ProcessDevices(_openCLResult) : null;

            Helpers.ConsolePrint(Tag, "QueryAMD END");

            if (amdDevices != null) SortBusIDs(amdDevices);

            return amdDevices;
        }

        private bool DriverCheck()
        {
            // check the driver version bool EnableOptimizedVersion = true;
            var showWarningDialog = false;
            var sgminerNoNeoscryptLyra2RE = new Version("21.19.164.1");

            foreach (var vidContrllr in SystemSpecs.AvailableVideoControllers)
            {
                if (!vidContrllr.IsAmd) continue;

                Helpers.ConsolePrint(Tag,
                    $"Checking AMD device (driver): {vidContrllr.Name} ({vidContrllr.DriverVersion})");
                
                _noNeoscryptLyra2[vidContrllr.Name] = false;
                
                var amdDriverVersion = new Version(vidContrllr.DriverVersion);

                if (!ConfigManager.GeneralConfig.ForceSkipAMDNeoscryptLyraCheck &&
                    amdDriverVersion >= sgminerNoNeoscryptLyra2RE)
                {
                    _noNeoscryptLyra2[vidContrllr.Name] = true;
                    Helpers.ConsolePrint(Tag,
                        "Driver version seems to be " + sgminerNoNeoscryptLyra2RE +
                        " or higher. NeoScrypt and Lyra2REv2 will be removed from list");
                }

                if (amdDriverVersion.Major >= 15) continue;

                showWarningDialog = true;
            }

            if (showWarningDialog)
            {
                Helpers.ConsolePrint(Tag,
                    "WARNING!!! Old AMD GPU driver detected! All optimized versions disabled, mining " +
                    "speed will not be optimal. Consider upgrading AMD GPU driver. Recommended AMD GPU driver version is 15.7.1.");
            }

            return showWarningDialog;
        }

        private List<AmdComputeDevice> ProcessDevices(OpenCLDeviceDetectionResult openCLData)
        {
            var amdOclDevices = new List<OpenCLDevice>();
            var amdDevices = new List<OpenCLDevice>();

            var amdPlatformNumFound = false;
            foreach (var oclEl in openCLData.Platforms)
            {
                if (!oclEl.PlatformName.Contains("AMD") && !oclEl.PlatformName.Contains("amd")) continue;
                amdPlatformNumFound = true;
                var amdOpenCLPlatformStringKey = oclEl.PlatformName;
                AvailableDevices.AmdOpenCLPlatformNum = oclEl.PlatformNum;
                amdOclDevices = oclEl.Devices;
                Helpers.ConsolePrint(Tag,
                    $"AMD platform found: Key: {amdOpenCLPlatformStringKey}, Num: {AvailableDevices.AmdOpenCLPlatformNum}");
                break;
            }

            if (!amdPlatformNumFound) return null;

            // get only AMD gpus
            {
                foreach (var oclDev in amdOclDevices)
                {
                    if (oclDev._CL_DEVICE_TYPE.Contains("GPU"))
                    {
                        amdDevices.Add(oclDev);
                    }
                }
            }

            if (amdDevices.Count == 0)
            {
                Helpers.ConsolePrint(Tag, "AMD GPUs count is 0");
                return null;
            }

            Helpers.ConsolePrint(Tag, "AMD GPUs count : " + amdDevices.Count);
            Helpers.ConsolePrint(Tag, "AMD Getting device name and serial from ADL");
            // ADL
            var isAdlInit = AdlQuery.TryQuery(out var busIdInfos, out var numDevs);

            var isBusIDOk = true;
            // check if buss ids are unique and different from -1
            {
                var busIDs = new HashSet<int>();
                // Override AMD bus IDs
                var overrides = ConfigManager.GeneralConfig.OverrideAMDBusIds.Split(',');
                for (var i = 0; i < amdDevices.Count; i++)
                {
                    var amdOclDev = amdDevices[i];
                    if (overrides.Count() > i &&
                        int.TryParse(overrides[i], out var overrideBus) &&
                        overrideBus >= 0)
                    {
                        amdOclDev.AMD_BUS_ID = overrideBus;
                    }

                    if (amdOclDev.AMD_BUS_ID < 0 || !busIdInfos.ContainsKey(amdOclDev.AMD_BUS_ID))
                    {
                        isBusIDOk = false;
                        break;
                    }

                    busIDs.Add(amdOclDev.AMD_BUS_ID);
                }

                // check if unique
                isBusIDOk = isBusIDOk && busIDs.Count == amdDevices.Count;
            }
            // print BUS id status
            Helpers.ConsolePrint(Tag,
                isBusIDOk
                    ? "AMD Bus IDs are unique and valid. OK"
                    : "AMD Bus IDs IS INVALID. Using fallback AMD detection mode");

            ///////
            // AMD device creation (in NHM context)
            AmdDeviceCreation devCreator;
            if (isAdlInit && isBusIDOk)
            {
                devCreator = new AmdDeviceCreationPrimary(busIdInfos);
            }
            else
            {
                devCreator = new AmdDeviceCreationFallback();
            }

            return devCreator.CreateDevices(_numDevs, amdDevices, _noNeoscryptLyra2);
        }
    }
}

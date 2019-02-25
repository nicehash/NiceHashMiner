using ATI.ADL;
using NiceHashMiner.Configs;
using NiceHashMiner.Devices.OpenCL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace NiceHashMiner.Devices.Querying
{
    public class AmdQuery
    {
        private const string Tag = "AmdQuery";
        private const int AmdVendorID = 1002;
        
        private readonly Dictionary<string, bool> _driverOld = new Dictionary<string, bool>();
        private readonly Dictionary<string, bool> _noNeoscryptLyra2 = new Dictionary<string, bool>();
        private readonly Dictionary<int, BusIdInfo> _busIdInfos = new Dictionary<int, BusIdInfo>();
        private readonly List<string> _amdDeviceUuid = new List<string>();

        private int _numDevs;

        public AmdQuery(int numDevs)
        {
            _numDevs = numDevs;
        }

        public List<OpenCLDevice> QueryAmd(bool openCLSuccess, OpenCLDeviceDetectionResult openCLData, out bool failedDriverCheck)
        {
            Helpers.ConsolePrint(Tag, "QueryAMD START");

            failedDriverCheck = DriverCheck();

            var amdDevices = openCLSuccess ? ProcessDevices(openCLData) : new List<OpenCLDevice>();

            Helpers.ConsolePrint(Tag, "QueryAMD END");

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

                _driverOld[vidContrllr.Name] = false;
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
                _driverOld[vidContrllr.Name] = true;
            }

            if (showWarningDialog)
            {
                Helpers.ConsolePrint(Tag,
                    "WARNING!!! Old AMD GPU driver detected! All optimized versions disabled, mining " +
                    "speed will not be optimal. Consider upgrading AMD GPU driver. Recommended AMD GPU driver version is 15.7.1.");
            }

            return showWarningDialog;
        }

        private List<OpenCLDevice> ProcessDevices(OpenCLDeviceDetectionResult openCLData)
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

            if (!amdPlatformNumFound) return amdDevices;

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
                return amdDevices;
            }

            Helpers.ConsolePrint(Tag, "AMD GPUs count : " + amdDevices.Count);
            Helpers.ConsolePrint(Tag, "AMD Getting device name and serial from ADL");
            // ADL
            bool isAdlInit;
            try
            {
                isAdlInit = QueryAdl();
            }
            catch (Exception ex)
            {
                Helpers.ConsolePrint(Tag, "AMD ADL exception: " + ex.Message);
                isAdlInit = false;
            }

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

                    if (amdOclDev.AMD_BUS_ID < 0 || !_busIdInfos.ContainsKey(amdOclDev.AMD_BUS_ID))
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
            if (isAdlInit && isBusIDOk)
            {
                return AmdDeviceCreationPrimary(amdDevices);
            }

            return AmdDeviceCreationFallback(amdDevices);
        }

        private List<OpenCLDevice> AmdDeviceCreationPrimary(List<OpenCLDevice> amdDevices)
        {
            Helpers.ConsolePrint(Tag, "Using AMD device creation DEFAULT Reliable mappings");
            Helpers.ConsolePrint(Tag,
                amdDevices.Count == _amdDeviceUuid.Count
                    ? "AMD OpenCL and ADL AMD query COUNTS GOOD/SAME"
                    : "AMD OpenCL and ADL AMD query COUNTS DIFFERENT/BAD");
            var stringBuilder = new StringBuilder();
            stringBuilder.AppendLine("");
            stringBuilder.AppendLine("QueryAMD [DEFAULT query] devices: ");
            foreach (var dev in amdDevices)
            {
                var busID = dev.AMD_BUS_ID;
                if (busID != -1 && _busIdInfos.ContainsKey(busID))
                {
                    var deviceName = _busIdInfos[busID].Name;
                    var newAmdDev = new AmdGpuDevice(dev, _driverOld[deviceName],
                        _busIdInfos[busID].InfSection, _noNeoscryptLyra2[deviceName])
                    {
                        DeviceName = deviceName,
                        UUID = _busIdInfos[busID].Uuid,
                        AdapterIndex = _busIdInfos[busID].Adl1Index
                    };
                    var isDisabledGroup = ConfigManager.GeneralConfig.DeviceDetection
                        .DisableDetectionAMD;
                    var skipOrAdd = isDisabledGroup ? "SKIPED" : "ADDED";
                    var isDisabledGroupStr = isDisabledGroup ? " (AMD group disabled)" : "";
                    var etherumCapableStr = newAmdDev.IsEtherumCapable() ? "YES" : "NO";

                    AvailableDevices.AddDevice(
                        new AmdComputeDevice(newAmdDev, ++_numDevs, false,
                            _busIdInfos[busID].Adl2Index));
                    // just in case 
                    try
                    {
                        stringBuilder.AppendLine($"\t{skipOrAdd} device{isDisabledGroupStr}:");
                        stringBuilder.AppendLine($"\t\tID: {newAmdDev.DeviceID}");
                        stringBuilder.AppendLine($"\t\tNAME: {newAmdDev.DeviceName}");
                        stringBuilder.AppendLine($"\t\tCODE_NAME: {newAmdDev.Codename}");
                        stringBuilder.AppendLine($"\t\tUUID: {newAmdDev.UUID}");
                        stringBuilder.AppendLine(
                            $"\t\tMEMORY: {newAmdDev.DeviceGlobalMemory}");
                        stringBuilder.AppendLine($"\t\tETHEREUM: {etherumCapableStr}");
                    }
                    catch
                    {
                    }
                }
                else
                {
                    stringBuilder.AppendLine($"\tDevice not added, Bus No. {busID} not found:");
                }
            }

            Helpers.ConsolePrint(Tag, stringBuilder.ToString());

            return amdDevices;
        }

        private List<OpenCLDevice> AmdDeviceCreationFallback(List<OpenCLDevice> amdDevices)
        {
            Helpers.ConsolePrint(Tag, "Using AMD device creation FALLBACK UnReliable mappings");
            var stringBuilder = new StringBuilder();
            stringBuilder.AppendLine("");
            stringBuilder.AppendLine("QueryAMD [FALLBACK query] devices: ");

            // get video AMD controllers and sort them by RAM
            // (find a way to get PCI BUS Numbers from PNPDeviceID)
            var amdVideoControllers = SystemSpecs.AvailableVideoControllers.Where(vcd => vcd.IsAmd).ToList();
            // sort by ram not ideal 
            amdVideoControllers.Sort((a, b) => (int) (a.AdapterRam - b.AdapterRam));
            amdDevices.Sort((a, b) =>
                (int) (a._CL_DEVICE_GLOBAL_MEM_SIZE - b._CL_DEVICE_GLOBAL_MEM_SIZE));
            var minCount = Math.Min(amdVideoControllers.Count, amdDevices.Count);

            for (var i = 0; i < minCount; ++i)
            {
                var deviceName = amdVideoControllers[i].Name;
                amdVideoControllers[i].SetInfSectionEmptyIfNull();
                var newAmdDev = new AmdGpuDevice(amdDevices[i], _driverOld[deviceName],
                    amdVideoControllers[i].InfSection,
                    _noNeoscryptLyra2[deviceName])
                {
                    DeviceName = deviceName,
                    UUID = "UNUSED"
                };
                var isDisabledGroup = ConfigManager.GeneralConfig.DeviceDetection
                    .DisableDetectionAMD;
                var skipOrAdd = isDisabledGroup ? "SKIPED" : "ADDED";
                var isDisabledGroupStr = isDisabledGroup ? " (AMD group disabled)" : "";
                var etherumCapableStr = newAmdDev.IsEtherumCapable() ? "YES" : "NO";

                AvailableDevices.AddDevice(
                    new AmdComputeDevice(newAmdDev, ++_numDevs, true, -1));
                // just in case 
                try
                {
                    stringBuilder.AppendLine($"\t{skipOrAdd} device{isDisabledGroupStr}:");
                    stringBuilder.AppendLine($"\t\tID: {newAmdDev.DeviceID}");
                    stringBuilder.AppendLine($"\t\tNAME: {newAmdDev.DeviceName}");
                    stringBuilder.AppendLine($"\t\tCODE_NAME: {newAmdDev.Codename}");
                    stringBuilder.AppendLine($"\t\tUUID: {newAmdDev.UUID}");
                    stringBuilder.AppendLine(
                        $"\t\tMEMORY: {newAmdDev.DeviceGlobalMemory}");
                    stringBuilder.AppendLine($"\t\tETHEREUM: {etherumCapableStr}");
                }
                catch
                {
                }
            }

            Helpers.ConsolePrint(Tag, stringBuilder.ToString());

            return amdDevices;
        }

        private bool QueryAdl()
        {
            // ADL does not get devices in order map devices by bus number
            // bus id, <name, uuid>

            var numberOfAdapters = 0;

            var adapterBuffer = IntPtr.Zero;

            try
            {
                var adlRet = ADL.ADL_Main_Control_Create?.Invoke(ADL.ADL_Main_Memory_Alloc, 1);
                AdlThrowIfException(adlRet, nameof(ADL.ADL_Main_Control_Create));

                adlRet = ADL.ADL_Adapter_NumberOfAdapters_Get?.Invoke(ref numberOfAdapters);
                AdlThrowIfException(adlRet, nameof(ADL.ADL_Adapter_NumberOfAdapters_Get));
                Helpers.ConsolePrint(Tag, "Number Of Adapters: " + numberOfAdapters);

                if (numberOfAdapters <= 0)
                    throw new Exception("Did not find any ADL adapters");

                // Get OS adpater info from ADL
                var osAdapterInfoData = new ADLAdapterInfoArray();

                var size = Marshal.SizeOf(osAdapterInfoData);
                adapterBuffer = Marshal.AllocCoTaskMem(size);
                Marshal.StructureToPtr(osAdapterInfoData, adapterBuffer, false);

                adlRet = ADL.ADL_Adapter_AdapterInfo_Get?.Invoke(adapterBuffer, size);
                AdlThrowIfException(adlRet, nameof(ADL.ADL_Adapter_AdapterInfo_Get));

                osAdapterInfoData = (ADLAdapterInfoArray)Marshal.PtrToStructure(adapterBuffer,
                    osAdapterInfoData.GetType());

                var adl2Info = TryGetAdl2AdapterInfo();

                var isActive = 0;

                for (var i = 0; i < numberOfAdapters; i++)
                {
                    var adapter = osAdapterInfoData.ADLAdapterInfo[i];

                    // Check if the adapter is active
                    adlRet = ADL.ADL_Adapter_Active_Get?.Invoke(adapter.AdapterIndex, ref isActive);

                    if (ADL.ADL_SUCCESS != adlRet) continue;

                    // we are looking for amd
                    // TODO check discrete and integrated GPU separation
                    var vendorID = adapter.VendorID;
                    var devName = adapter.AdapterName;

                    if (vendorID != AmdVendorID && !devName.ToLower().Contains("amd") &&
                        !devName.ToLower().Contains("radeon") &&
                        !devName.ToLower().Contains("firepro")) continue;

                    var pnpStr = adapter.PNPString;
                    // find vi controller pnp
                    var infSection = SystemSpecs.AvailableVideoControllers
                        .FirstOrDefault(vc => vc.PnpDeviceID == pnpStr)?
                        .InfSection ?? "";

                    var backSlashLast = pnpStr.LastIndexOf('\\');
                    var serial = pnpStr.Substring(backSlashLast, pnpStr.Length - backSlashLast);
                    var end0 = serial.IndexOf('&');
                    var end1 = serial.IndexOf('&', end0 + 1);
                    // get serial
                    serial = serial.Substring(end0 + 1, end1 - end0 - 1);

                    var udid = adapter.UDID;
                    const int pciVenIDStrSize = 21; // PCI_VEN_XXXX&DEV_XXXX
                    var uuid = udid.Substring(0, pciVenIDStrSize) + "_" + serial;
                    var busId = adapter.BusNumber;
                    var index = adapter.AdapterIndex;

                    if (_amdDeviceUuid.Contains(uuid)) continue;
                    
                    Helpers.ConsolePrint(Tag, $"ADL device added BusNumber:{busId}  NAME:{devName}  UUID:{uuid}");

                    _amdDeviceUuid.Add(uuid);

                    if (_busIdInfos.ContainsKey(busId)) continue;

                    var adl2Index = -1;
                    if (adl2Info != null)
                    {
                        adl2Index = adl2Info
                            .FirstOrDefault(a => a.UDID == adapter.UDID)
                            .AdapterIndex;
                    }

                    var info = new BusIdInfo
                    {
                        Name = devName,
                        Uuid = uuid,
                        InfSection = infSection,
                        Adl1Index = index,
                        Adl2Index = adl2Index
                    };

                    _busIdInfos[busId] = info;
                }

                return true;
            }
            catch (Exception e)
            {
                Helpers.ConsolePrint(Tag, e.Message);
                Helpers.ConsolePrint(Tag, "Check if ADL is properly installed!");
                return false;
            }
            finally
            {
                if (adapterBuffer != IntPtr.Zero)
                    Marshal.FreeCoTaskMem(adapterBuffer);
            }
        }

        private static List<ADLAdapterInfo> TryGetAdl2AdapterInfo()
        {
            var context = IntPtr.Zero;
            var buffer = IntPtr.Zero;

            try
            {
                var adl2Ret = ADL.ADL2_Main_Control_Create?.Invoke(ADL.ADL_Main_Memory_Alloc, 0, ref context);
                AdlThrowIfException(adl2Ret, nameof(ADL.ADL2_Main_Control_Create));

                var adl2Info = new ADLAdapterInfoArray();
                var size2 = Marshal.SizeOf(adl2Info);
                buffer = Marshal.AllocCoTaskMem(size2);

                Marshal.StructureToPtr(adl2Info, buffer, false);
                adl2Ret = ADL.ADL2_Adapter_AdapterInfo_Get?.Invoke(context, buffer, Marshal.SizeOf(adl2Info));
                AdlThrowIfException(adl2Ret, nameof(ADL.ADL2_Adapter_AdapterInfo_Get));
                
                adl2Info = (ADLAdapterInfoArray) Marshal.PtrToStructure(buffer, adl2Info.GetType());

                return new List<ADLAdapterInfo>(adl2Info.ADLAdapterInfo);
            }
            catch (Exception e)
            {
                Helpers.ConsolePrint(Tag, e.Message);
            }
            finally
            {
                if (context != IntPtr.Zero)
                {
                    ADL.ADL2_Main_Control_Destroy?.Invoke(context);
                }

                if (buffer != IntPtr.Zero)
                {
                    Marshal.FreeCoTaskMem(buffer);
                }
            }

            return null;
        }

        private static void AdlThrowIfException(int? adlCode, string adlFunction)
        {
            if (adlCode != ADL.ADL_SUCCESS)
            {
                throw new AdlException(adlCode, adlFunction);
            }
        }

        private struct BusIdInfo
        {
            public string Name;
            public string Uuid;
            public string InfSection;
            public int Adl1Index;
            public int Adl2Index;
        }

        private class AdlException : Exception
        {
            public int? AdlCode { get; }
            public string AdlFunction { get; }

            public AdlException(int? adlCode, string adlFunction)
                : base($"{adlFunction} {(adlCode == null ? "is null" : $"returned error code {adlCode}")}")
            {
                AdlCode = adlCode;
                AdlFunction = adlFunction;
            }
        }
    }
}

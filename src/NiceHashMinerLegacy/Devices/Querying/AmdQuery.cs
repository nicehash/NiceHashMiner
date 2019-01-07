using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using ATI.ADL;
using NiceHashMiner.Configs;
using NiceHashMiner.Forms;

namespace NiceHashMiner.Devices.Querying
{
    public class AmdQuery
    {
        private const string Tag = "AmdQuery";
        private const int AmdVendorID = 1002;

        private readonly List<VideoControllerData> _availableControllers;
        private readonly Dictionary<string, bool> _driverOld = new Dictionary<string, bool>();
        private readonly Dictionary<string, bool> _noNeoscryptLyra2 = new Dictionary<string, bool>();
        private readonly Dictionary<int, BusIdInfo> _busIdInfos = new Dictionary<int, BusIdInfo>();
        private readonly List<string> _amdDeviceUuid = new List<string>();


        public AmdQuery(List<VideoControllerData> availControllers)
        {
            _availableControllers = availControllers;
        }

        public List<OpenCLDevice> QueryAmd(bool openCLSuccess, IEnumerable<OpenCLJsonData> openCLData)
        {
            Helpers.ConsolePrint(Tag, "QueryAMD START");

            DriverCheck();

            var amdDevices = openCLSuccess ? ProcessDevices(openCLData) : new List<OpenCLDevice>();

            Helpers.ConsolePrint(Tag, "QueryAMD END");

            return amdDevices;
        }

        private void DriverCheck()
        {
            // check the driver version bool EnableOptimizedVersion = true;
            var showWarningDialog = false;

            foreach (var vidContrllr in _availableControllers)
            {
                Helpers.ConsolePrint(Tag,
                    $"Checking AMD device (driver): {vidContrllr.Name} ({vidContrllr.DriverVersion})");

                _driverOld[vidContrllr.Name] = false;
                _noNeoscryptLyra2[vidContrllr.Name] = false;
                var sgminerNoNeoscryptLyra2RE = new Version("21.19.164.1");

                // TODO checking radeon drivers only?
                if ((!vidContrllr.Name.Contains("AMD") && !vidContrllr.Name.Contains("Radeon")) ||
                    showWarningDialog) continue;

                var amdDriverVersion = new Version(vidContrllr.DriverVersion);

                if (!ConfigManager.GeneralConfig.ForceSkipAMDNeoscryptLyraCheck)
                {
                    var greaterOrEqual = amdDriverVersion.CompareTo(sgminerNoNeoscryptLyra2RE) >= 0;
                    if (greaterOrEqual)
                    {
                        _noNeoscryptLyra2[vidContrllr.Name] = true;
                        Helpers.ConsolePrint(Tag,
                            "Driver version seems to be " + sgminerNoNeoscryptLyra2RE +
                            " or higher. NeoScrypt and Lyra2REv2 will be removed from list");
                    }
                }


                if (amdDriverVersion.Major >= 15) continue;

                showWarningDialog = true;
                _driverOld[vidContrllr.Name] = true;
                Helpers.ConsolePrint(Tag,
                    "WARNING!!! Old AMD GPU driver detected! All optimized versions disabled, mining " +
                    "speed will not be optimal. Consider upgrading AMD GPU driver. Recommended AMD GPU driver version is 15.7.1.");
            }

            if (ConfigManager.GeneralConfig.ShowDriverVersionWarning && showWarningDialog)
            {
                Form warningDialog = new DriverVersionConfirmationDialog();
                warningDialog.ShowDialog();
                warningDialog = null;
            }
        }

        private List<OpenCLDevice> ProcessDevices(IEnumerable<OpenCLJsonData> openCLData)
        {
            var amdOclDevices = new List<OpenCLDevice>();
            var amdDevices = new List<OpenCLDevice>();

            var amdPlatformNumFound = false;
            foreach (var oclEl in openCLData)
            {
                if (!oclEl.PlatformName.Contains("AMD") && !oclEl.PlatformName.Contains("amd")) continue;
                amdPlatformNumFound = true;
                var amdOpenCLPlatformStringKey = oclEl.PlatformName;
                ComputeDeviceManager.Available.AmdOpenCLPlatformNum = oclEl.PlatformNum;
                amdOclDevices = oclEl.Devices;
                Helpers.ConsolePrint(Tag,
                    $"AMD platform found: Key: {amdOpenCLPlatformStringKey}, Num: {ComputeDeviceManager.Available.AmdOpenCLPlatformNum}");
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
            var isAdlInit = true;
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
                ComputeDeviceManager.Available.HasAmd = true;

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

                    ComputeDeviceManager.Available.Devices.Add(
                        new AmdComputeDevice(newAmdDev, ++ComputeDeviceManager.Query.GpuCount, false,
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
            var amdVideoControllers = _availableControllers.Where(vcd =>
                vcd.Name.ToLower().Contains("amd") || vcd.Name.ToLower().Contains("radeon") ||
                vcd.Name.ToLower().Contains("firepro")).ToList();
            // sort by ram not ideal 
            amdVideoControllers.Sort((a, b) => (int) (a.AdapterRam - b.AdapterRam));
            amdDevices.Sort((a, b) =>
                (int) (a._CL_DEVICE_GLOBAL_MEM_SIZE - b._CL_DEVICE_GLOBAL_MEM_SIZE));
            var minCount = Math.Min(amdVideoControllers.Count, amdDevices.Count);

            for (var i = 0; i < minCount; ++i)
            {
                ComputeDeviceManager.Available.HasAmd = true;

                var deviceName = amdVideoControllers[i].Name;
                if (amdVideoControllers[i].InfSection == null)
                    amdVideoControllers[i].InfSection = "";
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

                ComputeDeviceManager.Available.Devices.Add(
                    new AmdComputeDevice(newAmdDev, ++ComputeDeviceManager.Query.GpuCount, true, -1));
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
            var isAdlInit = true;

            var adlRet = -1;
            var numberOfAdapters = 0;
            var adl2Control = IntPtr.Zero;

            if (null != ADL.ADL_Main_Control_Create)
                // Second parameter is 1: Get only the present adapters
                adlRet = ADL.ADL_Main_Control_Create(ADL.ADL_Main_Memory_Alloc, 1);
            if (ADL.ADL_SUCCESS == adlRet)
            {
                ADL.ADL_Adapter_NumberOfAdapters_Get?.Invoke(ref numberOfAdapters);
                Helpers.ConsolePrint(Tag, "Number Of Adapters: " + numberOfAdapters);

                if (0 < numberOfAdapters)
                {
                    // Get OS adpater info from ADL
                    var osAdapterInfoData = new ADLAdapterInfoArray();

                    if (null != ADL.ADL_Adapter_AdapterInfo_Get)
                    {
                        var size = Marshal.SizeOf(osAdapterInfoData);
                        var adapterBuffer = Marshal.AllocCoTaskMem(size);
                        Marshal.StructureToPtr(osAdapterInfoData, adapterBuffer, false);

                        adlRet = ADL.ADL_Adapter_AdapterInfo_Get(adapterBuffer, size);

                        var adl2Ret = -1;
                        if (ADL.ADL2_Main_Control_Create != null)
                            adl2Ret = ADL.ADL2_Main_Control_Create(ADL.ADL_Main_Memory_Alloc, 0, ref adl2Control);

                        var adl2Info = new ADLAdapterInfoArray();
                        var size2 = Marshal.SizeOf(adl2Info);
                        var buffer = Marshal.AllocCoTaskMem(size2);
                        if (adl2Ret == ADL.ADL_SUCCESS && ADL.ADL2_Adapter_AdapterInfo_Get != null)
                        {
                            Marshal.StructureToPtr(adl2Info, buffer, false);
                            adl2Ret = ADL.ADL2_Adapter_AdapterInfo_Get(adl2Control, buffer, Marshal.SizeOf(adl2Info));
                        }
                        else
                        {
                            adl2Ret = -1;
                        }

                        if (adl2Ret == ADL.ADL_SUCCESS)
                        {
                            adl2Info = (ADLAdapterInfoArray) Marshal.PtrToStructure(buffer, adl2Info.GetType());
                        }

                        if (ADL.ADL_SUCCESS == adlRet)
                        {
                            osAdapterInfoData =
                                (ADLAdapterInfoArray) Marshal.PtrToStructure(adapterBuffer,
                                    osAdapterInfoData.GetType());
                            var isActive = 0;

                            for (var i = 0; i < numberOfAdapters; i++)
                            {
                                // Check if the adapter is active
                                if (null != ADL.ADL_Adapter_Active_Get)
                                    adlRet = ADL.ADL_Adapter_Active_Get(
                                        osAdapterInfoData.ADLAdapterInfo[i].AdapterIndex, ref isActive);

                                if (ADL.ADL_SUCCESS != adlRet) continue;

                                // we are looking for amd
                                // TODO check discrete and integrated GPU separation
                                var vendorID = osAdapterInfoData.ADLAdapterInfo[i].VendorID;
                                var devName = osAdapterInfoData.ADLAdapterInfo[i].AdapterName;

                                if (vendorID != AmdVendorID && !devName.ToLower().Contains("amd") &&
                                    !devName.ToLower().Contains("radeon") &&
                                    !devName.ToLower().Contains("firepro")) continue;

                                var pnpStr = osAdapterInfoData.ADLAdapterInfo[i].PNPString;
                                // find vi controller pnp
                                var infSection = "";
                                foreach (var vCtrl in _availableControllers)
                                {
                                    if (vCtrl.PnpDeviceID == pnpStr)
                                    {
                                        infSection = vCtrl.InfSection;
                                    }
                                }

                                var backSlashLast = pnpStr.LastIndexOf('\\');
                                var serial = pnpStr.Substring(backSlashLast, pnpStr.Length - backSlashLast);
                                var end0 = serial.IndexOf('&');
                                var end1 = serial.IndexOf('&', end0 + 1);
                                // get serial
                                serial = serial.Substring(end0 + 1, end1 - end0 - 1);

                                var udid = osAdapterInfoData.ADLAdapterInfo[i].UDID;
                                const int pciVenIDStrSize = 21; // PCI_VEN_XXXX&DEV_XXXX
                                var uuid = udid.Substring(0, pciVenIDStrSize) + "_" + serial;
                                var budId = osAdapterInfoData.ADLAdapterInfo[i].BusNumber;
                                var index = osAdapterInfoData.ADLAdapterInfo[i].AdapterIndex;

                                if (_amdDeviceUuid.Contains(uuid)) continue;

                                try
                                {
                                    Helpers.ConsolePrint(Tag,
                                        $"ADL device added BusNumber:{budId}  NAME:{devName}  UUID:{uuid}");
                                }
                                catch (Exception e)
                                {
                                    Helpers.ConsolePrint(Tag, e.Message);
                                }

                                _amdDeviceUuid.Add(uuid);
                                //_busIds.Add(OSAdapterInfoData.ADLAdapterInfo[i].BusNumber);
                                //_amdDeviceName.Add(devName);

                                if (_busIdInfos.ContainsKey(budId)) continue;

                                var adl2Index = -1;
                                if (adl2Ret == ADL.ADL_SUCCESS)
                                {
                                    adl2Index = adl2Info.ADLAdapterInfo
                                        .FirstOrDefault(a => a.UDID == osAdapterInfoData.ADLAdapterInfo[i].UDID)
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

                                _busIdInfos.Add(budId, info);
                            }
                        }
                        else
                        {
                            Helpers.ConsolePrint(Tag,
                                "ADL_Adapter_AdapterInfo_Get() returned error code " +
                                adlRet);
                            isAdlInit = false;
                        }

                        // Release the memory for the AdapterInfo structure
                        if (IntPtr.Zero != adapterBuffer)
                            Marshal.FreeCoTaskMem(adapterBuffer);
                        if (buffer != IntPtr.Zero)
                            Marshal.FreeCoTaskMem(buffer);
                    }
                }

                if (null != ADL.ADL_Main_Control_Destroy && numberOfAdapters <= 0)
                    // Close ADL if it found no AMD devices
                    ADL.ADL_Main_Control_Destroy();
                if (ADL.ADL2_Main_Control_Destroy != null && adl2Control != IntPtr.Zero)
                {
                    ADL.ADL2_Main_Control_Destroy(adl2Control);
                }
            }
            else
            {
                // TODO
                Helpers.ConsolePrint(Tag,
                    "ADL_Main_Control_Create() returned error code " + adlRet);
                Helpers.ConsolePrint(Tag, "Check if ADL is properly installed!");
                isAdlInit = false;
            }

            return isAdlInit;
        }

        private struct BusIdInfo
        {
            public string Name;
            public string Uuid;
            public string InfSection;
            public int Adl1Index;
            public int Adl2Index;
        }
    }
}

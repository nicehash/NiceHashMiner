using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using ATI.ADL;
using NHM.Common;

namespace NHM.DeviceMonitoring.AMD
{
    internal static class QueryAdl
    {
        private const string Tag = "QueryADL";
        private const int AmdVendorID = 1002;

        public static (bool success, List<AmdBusIDInfo> busIdInfos) TryQuery(Dictionary<int, string> amdBusIdAndUuids)
        {
            var busIdInfos = new List<AmdBusIDInfo>();
            var success = false;

            var adapterBuffer = IntPtr.Zero;
            try
            {
                var numberOfAdapters = 0;
                var adlRet = ADL.ADL_Main_Control_Create?.Invoke(ADL.ADL_Main_Memory_Alloc, 1);
                AdlThrowIfException(adlRet, nameof(ADL.ADL_Main_Control_Create));

                adlRet = ADL.ADL_Adapter_NumberOfAdapters_Get?.Invoke(ref numberOfAdapters);
                AdlThrowIfException(adlRet, nameof(ADL.ADL_Adapter_NumberOfAdapters_Get));
                Logger.Info(Tag, $"Number Of Adapters: {numberOfAdapters}");

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
                    if (!IsAmdAdapter(adapter)) continue;
                    if (!amdBusIdAndUuids.ContainsKey(adapter.BusNumber)) continue;

                    var adl2Index = -1;
                    if (adl2Info != null)
                    {
                        adl2Index = adl2Info
                            .FirstOrDefault(a => a.UDID == adapter.UDID)
                            .AdapterIndex;
                    }

                    var info = new AmdBusIDInfo
                    {
                        BusID = adapter.BusNumber,
                        Uuid = amdBusIdAndUuids[adapter.BusNumber],
                        Adl1Index = adapter.AdapterIndex,
                        Adl2Index = adl2Index
                    };
                    busIdInfos.Add(info);
                }

                success = true;
            }
            catch (Exception e)
            {
                Logger.Error(Tag, e.Message);
                Logger.Info(Tag, "Check if ADL is properly installed!");
                success = false;
            }
            finally
            {
                try
                {
                    if (adapterBuffer != IntPtr.Zero)
                        Marshal.FreeCoTaskMem(adapterBuffer);
                }
                catch
                { }
            }
            return (success, busIdInfos);
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

                adl2Info = (ADLAdapterInfoArray)Marshal.PtrToStructure(buffer, adl2Info.GetType());

                return new List<ADLAdapterInfo>(adl2Info.ADLAdapterInfo);
            }
            catch (Exception e)
            {
                Logger.Error(Tag, e.Message);
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

        private static bool IsAmdAdapter(ADLAdapterInfo adapter)
        {
            var vendorID = adapter.VendorID;
            var devName = adapter.AdapterName.ToLower();
            // name contains are probably redundant
            return vendorID == AmdVendorID || devName.Contains("amd") || devName.Contains("radeon") || devName.Contains("firepro");
        }

        private static void AdlThrowIfException(int? adlCode, string adlFunction)
        {
            if (adlCode != ADL.ADL_SUCCESS)
            {
                throw new AdlException(adlCode, adlFunction);
            }
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

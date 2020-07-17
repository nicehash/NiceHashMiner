/*
  Adapted from OpenHardwareMonitor https://github.com/openhardwaremonitor/openhardwaremonitor
 
  This Source Code Form is subject to the terms of the Mozilla Public
  License, v. 2.0. If a copy of the MPL was not distributed with this
  file, You can obtain one at http://mozilla.org/MPL/2.0/.
 
  Copyright (C) 2009-2012 Michael M�ller <mmoeller@openhardwaremonitor.org>
	
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using NHM.Common;
// ReSharper disable All
#pragma warning disable

namespace NVIDIA.NVAPI
{
    #region Enumms
    internal enum NvStatus
    {
        OK = 0,
        ERROR = -1,
        LIBRARY_NOT_FOUND = -2,
        NO_IMPLEMENTATION = -3,
        API_NOT_INTIALIZED = -4,
        INVALID_ARGUMENT = -5,
        NVIDIA_DEVICE_NOT_FOUND = -6,
        END_ENUMERATION = -7,
        INVALID_HANDLE = -8,
        INCOMPATIBLE_STRUCT_VERSION = -9,
        HANDLE_INVALIDATED = -10,
        OPENGL_CONTEXT_NOT_CURRENT = -11,
        NO_GL_EXPERT = -12,
        INSTRUMENTATION_DISABLED = -13,
        EXPECTED_LOGICAL_GPU_HANDLE = -100,
        EXPECTED_PHYSICAL_GPU_HANDLE = -101,
        EXPECTED_DISPLAY_HANDLE = -102,
        INVALID_COMBINATION = -103,
        NOT_SUPPORTED = -104,
        PORTID_NOT_FOUND = -105,
        EXPECTED_UNATTACHED_DISPLAY_HANDLE = -106,
        INVALID_PERF_LEVEL = -107,
        DEVICE_BUSY = -108,
        NV_PERSIST_FILE_NOT_FOUND = -109,
        PERSIST_DATA_NOT_FOUND = -110,
        EXPECTED_TV_DISPLAY = -111,
        EXPECTED_TV_DISPLAY_ON_DCONNECTOR = -112,
        NO_ACTIVE_SLI_TOPOLOGY = -113,
        SLI_RENDERING_MODE_NOTALLOWED = -114,
        EXPECTED_DIGITAL_FLAT_PANEL = -115,
        ARGUMENT_EXCEED_MAX_SIZE = -116,
        DEVICE_SWITCHING_NOT_ALLOWED = -117,
        TESTING_CLOCKS_NOT_SUPPORTED = -118,
        UNKNOWN_UNDERSCAN_CONFIG = -119,
        TIMEOUT_RECONFIGURING_GPU_TOPO = -120,
        DATA_NOT_FOUND = -121,
        EXPECTED_ANALOG_DISPLAY = -122,
        NO_VIDLINK = -123,
        REQUIRES_REBOOT = -124,
        INVALID_HYBRID_MODE = -125,
        MIXED_TARGET_TYPES = -126,
        SYSWOW64_NOT_SUPPORTED = -127,
        IMPLICIT_SET_GPU_TOPOLOGY_CHANGE_NOT_ALLOWED = -128,
        REQUEST_USER_TO_CLOSE_NON_MIGRATABLE_APPS = -129,
        OUT_OF_MEMORY = -130,
        WAS_STILL_DRAWING = -131,
        FILE_NOT_FOUND = -132,
        TOO_MANY_UNIQUE_STATE_OBJECTS = -133,
        INVALID_CALL = -134,
        D3D10_1_LIBRARY_NOT_FOUND = -135,
        FUNCTION_NOT_FOUND = -136
    }
    internal enum NvThermalController
    {
        NONE = 0,
        GPU_INTERNAL,
        ADM1032,
        MAX6649,
        MAX1617,
        LM99,
        LM89,
        LM64,
        ADT7473,
        SBMAX6649,
        VBIOSEVT,
        OS,
        UNKNOWN = -1,
    }
    internal enum NvThermalTarget
    {
        NONE = 0,
        GPU = 1,
        MEMORY = 2,
        POWER_SUPPLY = 4,
        BOARD = 8,
        ALL = 15,
        UNKNOWN = -1
    };

    #endregion

    #region Structs
    [StructLayout(LayoutKind.Sequential)]
    internal struct NvPhysicalGpuHandle
    {
        public readonly IntPtr ptr;
    }
    [StructLayout(LayoutKind.Sequential, Pack = 8)]
    internal struct NvPState
    {
        public bool Present;
        public int Percentage;
    }
    [StructLayout(LayoutKind.Sequential, Pack = 8)]
    internal struct NvPStates
    {
        public uint Version;
        public uint Flags;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = NVAPI.MAX_PSTATES_PER_GPU)]
        public NvPState[] PStates;
    }
    [StructLayout(LayoutKind.Sequential, Pack = 8)]
    internal struct NvLevel
    {
        public int Level;
        public int Policy;
    }
    [StructLayout(LayoutKind.Sequential, Pack = 8)]
    internal struct NvSensor
    {
        public NvThermalController Controller;
        public uint DefaultMinTemp;
        public uint DefaultMaxTemp;
        public uint CurrentTemp;
        public NvThermalTarget Target;
    }
    [StructLayout(LayoutKind.Sequential, Pack = 8)]
    internal struct NvGPUThermalSettings
    {
        public uint Version;
        public uint Count;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = NVAPI.MAX_THERMAL_SENSORS_PER_GPU)]
        public NvSensor[] Sensor;
    }
    [StructLayout(LayoutKind.Sequential, Pack = 8)]
    internal struct NvGPUPowerInfo
    {
        public uint Version;
        public uint Flags;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = NVAPI.MAX_POWER_ENTRIES_PER_GPU)]
        public NvGPUPowerInfoEntry[] Entries;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 8)]
    internal struct NvGPUPowerInfoEntry
    {
        public uint PState;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
        public uint[] Unknown1;
        public uint MinPower;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
        public uint[] Unknown2;
        public uint DefPower;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
        public uint[] Unknown3;
        public uint MaxPower;
        public uint Unknown4;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 8)]
    internal struct NvGPUPowerStatus
    {
        public uint Version;
        public uint Flags;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = NVAPI.MAX_POWER_ENTRIES_PER_GPU)]
        public NvGPUPowerStatusEntry[] Entries;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 8)]
    internal struct NvGPUPowerStatusEntry
    {
        public uint Unknown1;
        public uint Unknown2;
        /// <summary>
        /// Power percentage * 1000 (e.g. 50% is 50000)
        /// </summary>
        public uint Power;
        public uint Unknown4;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 8)]
    internal struct NvGPULevels
    {
        public uint Version;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = NVAPI.MAX_COOLER_PER_GPU)]
        public nv_level_internal[] Levels;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 8)]
    internal struct nv_level_internal
    {
        public int level;
        public int policy;
    };

    [StructLayout(LayoutKind.Sequential, Pack = 8)]
    internal struct nv_fan_internal
    {
        public int type;
        public int controller;
        public int default_min;
        public int default_max;
        public int current_min;
        public int current_max;
        public int current_level;
        public int default_policy;
        public int current_policy;
        public int target;
        public int control_type;
        public int active;
    };

    [StructLayout(LayoutKind.Sequential, Pack = 8)]
    internal struct nv_fandata
    {
        public uint version;
        public uint count;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = NVAPI.MAX_COOLER_PER_GPU)]
        public nv_fan_internal[] internals;
    };

    #endregion

    internal class NVAPI
    {
        internal const int MAX_PHYSICAL_GPUS = 64;
        internal const int MAX_PSTATES_PER_GPU = 8;
        internal const int MAX_COOLER_PER_GPU = 20;
        internal const int MAX_THERMAL_SENSORS_PER_GPU = 3;
        internal const int MAX_POWER_ENTRIES_PER_GPU = 4;
        
        public static readonly uint GPU_PSTATES_VER = (uint)Marshal.SizeOf(typeof(NvPStates)) | 0x10000;
        public static readonly uint GPU_THERMAL_SETTINGS_VER = (uint)Marshal.SizeOf(typeof(NvGPUThermalSettings)) | 0x10000;
        public static readonly uint GPU_POWER_STATUS_VER = (uint) Marshal.SizeOf(typeof(NvGPUPowerStatus)) | 0x10000;
        public static readonly uint GPU_POWER_INFO_VER = (uint) Marshal.SizeOf(typeof(NvGPUPowerInfo)) | 0x10000;

        #region Delegates
        private delegate IntPtr nvapi_QueryInterfaceDelegate(uint id);
        private delegate NvStatus NvAPI_InitializeDelegate();

        internal delegate NvStatus NvAPI_EnumPhysicalGPUsDelegate([Out] NvPhysicalGpuHandle[] gpuHandles, out int gpuCount);
        internal delegate NvStatus NvAPI_GPU_GetBusIdDelegate(NvPhysicalGpuHandle gpuHandle, out int busID);
        internal delegate NvStatus NvAPI_GPU_GetTachReadingDelegate(NvPhysicalGpuHandle gpuHandle, out int value);
        internal delegate NvStatus NvAPI_GPU_GetPStatesDelegate(NvPhysicalGpuHandle gpuHandle, ref NvPStates nvPStates);
        internal delegate NvStatus NvAPI_GPU_GetThermalSettingsDelegate(NvPhysicalGpuHandle gpuHandle, int sensorIndex, ref NvGPUThermalSettings nvGPUThermalSettings);
        internal delegate NvStatus NvAPI_GPU_GetCoolerLevelsDelegate(NvPhysicalGpuHandle gpuHandle, int sensorIndex, ref nv_fandata fandata);
        internal delegate NvStatus NvAPI_GPU_SetCoolerLevelsDelegate(NvPhysicalGpuHandle gpuHandle, int sensorIndex, ref NvGPULevels level);

        internal delegate NvStatus NvAPI_DLL_ClientPowerPoliciesGetInfoDelegate(NvPhysicalGpuHandle gpuHandle, ref NvGPUPowerInfo info);
        internal delegate NvStatus NvAPI_DLL_ClientPowerPoliciesGetStatusDelegate(NvPhysicalGpuHandle gpuHandle, ref NvGPUPowerStatus status);
        internal delegate NvStatus NvAPI_DLL_ClientPowerPoliciesSetStatusDelegate(NvPhysicalGpuHandle gpuHandle, ref NvGPUPowerStatus status);


        private static readonly nvapi_QueryInterfaceDelegate nvapi_QueryInterface;
        private static readonly NvAPI_InitializeDelegate NvAPI_Initialize;
        private static readonly bool available;

        internal static readonly NvAPI_EnumPhysicalGPUsDelegate NvAPI_EnumPhysicalGPUs;
        internal static readonly NvAPI_GPU_GetBusIdDelegate NvAPI_GPU_GetBusID;
        internal static readonly NvAPI_GPU_GetTachReadingDelegate NvAPI_GPU_GetTachReading;
        internal static readonly NvAPI_GPU_GetPStatesDelegate NvAPI_GPU_GetPStates;
        internal static readonly NvAPI_GPU_GetThermalSettingsDelegate NvAPI_GPU_GetThermalSettings;
        internal static readonly NvAPI_GPU_GetCoolerLevelsDelegate NvAPI_GPU_GetCoolerLevels;
        internal static readonly NvAPI_GPU_SetCoolerLevelsDelegate NvAPI_GPU_SetCoolerLevels;

        internal static readonly NvAPI_DLL_ClientPowerPoliciesGetInfoDelegate NvAPI_DLL_ClientPowerPoliciesGetInfo;
        internal static readonly NvAPI_DLL_ClientPowerPoliciesGetStatusDelegate NvAPI_DLL_ClientPowerPoliciesGetStatus;
        internal static readonly NvAPI_DLL_ClientPowerPoliciesSetStatusDelegate NvAPI_DLL_ClientPowerPoliciesSetStatus;

        #endregion

        private static void GetDelegate<T>(uint id, out T newDelegate) 
            where T : class {
            IntPtr ptr = nvapi_QueryInterface(id);
            if (ptr != IntPtr.Zero) {
                newDelegate = Marshal.GetDelegateForFunctionPointer(ptr, typeof(T)) as T;
            } else {
                newDelegate = null;
            }
        }

        static NVAPI() {
            DllImportAttribute attribute = new DllImportAttribute("nvapi64.dll");
            attribute.CallingConvention = CallingConvention.Cdecl;
            attribute.PreserveSig = true;
            attribute.EntryPoint = "nvapi_QueryInterface";
            PInvokeDelegateFactory.CreateDelegate(attribute, out nvapi_QueryInterface);

            try {
                GetDelegate(0x0150E828, out NvAPI_Initialize);
            } catch (Exception e) {
                Logger.Info("NVAPI", e.ToString());
                return;
            }

            if (NvAPI_Initialize() == NvStatus.OK) {
                GetDelegate(0x5F608315, out NvAPI_GPU_GetTachReading);
                GetDelegate(0x60DED2ED, out NvAPI_GPU_GetPStates);
                GetDelegate(0xE3640A56, out NvAPI_GPU_GetThermalSettings);
                GetDelegate(0xE5AC921F, out NvAPI_EnumPhysicalGPUs);
                GetDelegate(0x1BE0B8E5, out NvAPI_GPU_GetBusID);
                GetDelegate(0x34206D86, out NvAPI_DLL_ClientPowerPoliciesGetInfo);
                GetDelegate(0x70916171, out NvAPI_DLL_ClientPowerPoliciesGetStatus);
                GetDelegate(0xAD95F5ED, out NvAPI_DLL_ClientPowerPoliciesSetStatus);
                GetDelegate(0xDA141340, out NvAPI_GPU_GetCoolerLevels);
                GetDelegate(0x891FA0AE, out NvAPI_GPU_SetCoolerLevels);
            }

            available = true;
        }

        public static bool IsAvailable { get { return available; } }

        public static uint MakeNVAPIVersion(object param, uint version)
        {
            return 72 | (version << 16);
        }
    }
}

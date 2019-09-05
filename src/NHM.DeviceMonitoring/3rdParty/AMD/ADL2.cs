using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

namespace ATI.ADL
{
    #region Export Delegates
    internal delegate int ADL2_Main_Control_Create(ADL_Main_Memory_Alloc callback, int enumConnectedAdapters, ref IntPtr context);

    internal delegate int ADL2_Main_Control_Destroy(IntPtr context);

    internal delegate int ADL2_Adapter_AdapterInfo_Get(IntPtr context, IntPtr lpInfo, int iInputSize);

    // Overdrive 

    internal delegate int ADL_Overdrive5_CurrentActivity_Get(int iAdapterIndex, ref ADLPMActivity activity);

    internal delegate int ADL_Overdrive5_Temperature_Get(int adapterIndex, int thermalControllerIndex, ref ADLTemperature temperature);

    internal delegate int ADL_Overdrive5_FanSpeed_Get(int adapterIndex, int thermalControllerIndex, ref ADLFanSpeedValue temperature);

    internal delegate int ADL2_Overdrive6_CurrentPower_Get(IntPtr context, int iAdapterIndex, int iPowerType, ref int lpCurrentValue);

    internal delegate int ADL2_Overdrive_Caps(IntPtr context, int iAdapterIndex, ref int iSupported, ref int iEnabled, ref int iVersion);

    internal delegate int ADL2_OverdriveN_CapabilitiesX2_Get(IntPtr context, int iAdapterIndex, ref ADLODNCapabilitiesX2 lpODCapabilities);

    internal delegate int ADL2_OverdriveN_PowerLimit_Get(IntPtr context, int iAdapterIndex, ref ADLODNPowerLimitSetting lpODPowerLimit);

    internal delegate int ADL2_OverdriveN_PowerLimit_Set(IntPtr context, int iAdapterIndex, ref ADLODNPowerLimitSetting lpODPowerLimit);

    #endregion Export Delegates

    #region Export Struct

    [StructLayout(LayoutKind.Sequential)]
    internal struct ADLPMActivity
    {
        public int Size;
        public int EngineClock;
        public int MemoryClock;
        public int Vddc;
        /// <summary>
        /// GPU Utilization
        /// </summary>
        public int ActivityPercent;
        public int CurrentPerformanceLevel;
        public int CurrentBusSpeed;
        public int CurrentBusLanes;
        public int MaximumBusLanes;
        public int Reserved;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct ADLTemperature
    {
        public int Size;
        /// <summary>
        /// Temperature in millidegrees Celsius
        /// </summary>
        public int Temperature;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct ADLFanSpeedValue
    {
        public int Size;
        public int SpeedType;
        public int FanSpeed;
        public int Flags;
    }

    #region Overdrive

    ///////////////////////////////////////////////////////////////////////////
    // ADLODNControlType Enumeration
    ///////////////////////////////////////////////////////////////////////////
    enum ADLODNControlType : int
    {
        ODNControlType_Current = 0,
        ODNControlType_Default,
        ODNControlType_Auto,
        ODNControlType_Manual
    };

    /////////////////////////////////////////////////////////////////////////////////////////////
    ///\brief Structure containing information about Overdrive N clock range
    ///
    /// This structure is used to store information about Overdrive N clock range
    /// \nosubgrouping
    ////////////////////////////////////////////////////////////////////////////////////////////
    [StructLayout(LayoutKind.Sequential)]
    internal struct ADLODNParameterRange
    {
        /// The starting value of the clock range
        public int iMode;
        /// The starting value of the clock range
        public int iMin;
        /// The ending value of the clock range
        public int iMax;
        /// The minimum increment between clock values
        public int iStep;
        /// The default clock values
        public int iDefault;
    }


    /////////////////////////////////////////////////////////////////////////////////////////////
    ///\brief Structure containing information about Overdrive N capabilities
    ///
    /// This structure is used to store information about Overdrive N capabilities
    /// \nosubgrouping
    ////////////////////////////////////////////////////////////////////////////////////////////
    [StructLayout(LayoutKind.Sequential)]
    internal struct ADLODNCapabilitiesX2
    {
        /// Number of levels which describe the minimum to maximum clock ranges.
        /// The 1st level indicates the minimum clocks, and the 2nd level
        /// indicates the maximum clocks.
        public int iMaximumNumberOfPerformanceLevels;
        /// bit vector, which tells what are the features are supported.
        /// \ref: ADLODNFEATURECONTROL
        public int iFlags;
        /// Contains the hard limits of the sclk range.  Overdrive
        /// clocks cannot be set outside this range.
        public ADLODNParameterRange sEngineClockRange;
        /// Contains the hard limits of the mclk range.  Overdrive
        /// clocks cannot be set outside this range.
        public ADLODNParameterRange sMemoryClockRange;
        /// Contains the hard limits of the vddc range.  Overdrive
        /// clocks cannot be set outside this range.
        public ADLODNParameterRange svddcRange;
        /// Contains the hard limits of the power range.  Overdrive
        /// clocks cannot be set outside this range.
        public ADLODNParameterRange power;
        /// Contains the hard limits of the power range.  Overdrive
        /// clocks cannot be set outside this range.
        public ADLODNParameterRange powerTuneTemperature;
        /// Contains the hard limits of the Temperature range.  Overdrive
        /// clocks cannot be set outside this range.
        public ADLODNParameterRange fanTemperature;
        /// Contains the hard limits of the Fan range.  Overdrive
        /// clocks cannot be set outside this range.
        public ADLODNParameterRange fanSpeed;
        /// Contains the hard limits of the Fan range.  Overdrive
        /// clocks cannot be set outside this range.
        public ADLODNParameterRange minimumPerformanceClock;
        /// Contains the hard limits of the throttleNotification
        public ADLODNParameterRange throttleNotificaion;
        /// Contains the hard limits of the Auto Systemclock
        public ADLODNParameterRange autoSystemClock;
    }

    /////////////////////////////////////////////////////////////////////////////////////////////
    ///\brief Structure containing information about Overdrive N power limit.
    ///
    /// This structure is used to store information about Overdrive power limit.
    /// This structure is used by the ADL_OverdriveN_ODPerformanceLevels_Get() and ADL_OverdriveN_ODPerformanceLevels_Set() functions.
    /// \nosubgrouping
    ////////////////////////////////////////////////////////////////////////////////////////////
    [StructLayout(LayoutKind.Sequential)]
    internal struct ADLODNPowerLimitSetting
    {
        public int iMode;
        public int iTDPLimit;
        public int iMaxOperatingTemperature;
    }

    #endregion Overdrive

    #endregion Export Struct

    internal static partial class ADL
    {
        #region Internal Constant
        internal const int ADL_NOT_SUPPORTED = -8;
        internal const int ADL_DL_FANCTRL_SPEED_TYPE_PERCENT = 1;
        internal const int ADL_DL_FANCTRL_SPEED_TYPE_RPM = 2;
        #endregion Internal Constant


        // ADL2 stuff keep a separate partial classes to split from original sources
        private static partial class ADLImport
        {
            [DllImport(Atiadlxx_FileName, CallingConvention = CallingConvention.Cdecl)]
            internal static extern int ADL2_Main_Control_Create(ADL_Main_Memory_Alloc callback, int enumConnectedAdapters, ref IntPtr context);

            [DllImport(Atiadlxx_FileName, CallingConvention = CallingConvention.Cdecl)]
            internal static extern int ADL2_Main_Control_Destroy(IntPtr context);

            [DllImport(Atiadlxx_FileName, CallingConvention = CallingConvention.Cdecl)]
            internal static extern int ADL2_Adapter_AdapterInfo_Get(IntPtr context, IntPtr lpInfo, int iInputSize);


            // ADL_Overdrive5
            [DllImport(Atiadlxx_FileName, CallingConvention = CallingConvention.Cdecl)]
            internal static extern int ADL_Overdrive5_CurrentActivity_Get(int iAdapterIndex, ref ADLPMActivity activity);

            [DllImport(Atiadlxx_FileName, CallingConvention = CallingConvention.Cdecl)]
            internal static extern int ADL_Overdrive5_Temperature_Get(int adapterIndex, int thermalControllerIndex, ref ADLTemperature temperature);

            [DllImport(Atiadlxx_FileName, CallingConvention = CallingConvention.Cdecl)]
            internal static extern int ADL_Overdrive5_FanSpeed_Get(int adapterIndex, int thermalControllerIndex, ref ADLFanSpeedValue fanSpeedValue);

            [DllImport(Atiadlxx_FileName, CallingConvention = CallingConvention.Cdecl)]
            internal static extern int ADL2_Overdrive_Caps(IntPtr context, int iAdapterIndex, ref int iSupported, ref int iEnabled, ref int iVersion);

            // Overdrive6
            [DllImport(Atiadlxx_FileName, CallingConvention = CallingConvention.Cdecl)]
            internal static extern int ADL2_Overdrive6_CurrentPower_Get(IntPtr context, int iAdapterIndex, int iPowerType, ref int lpCurrentValue);

            // OverdriveN
            [DllImport(Atiadlxx_FileName, CallingConvention = CallingConvention.Cdecl)]
            internal static extern int ADL2_OverdriveN_CapabilitiesX2_Get(IntPtr context, int iAdapterIndex, ref ADLODNCapabilitiesX2 lpODCapabilities);

            [DllImport(Atiadlxx_FileName, CallingConvention = CallingConvention.Cdecl)]
            internal static extern int ADL2_OverdriveN_PowerLimit_Get(IntPtr context, int iAdapterIndex, ref ADLODNPowerLimitSetting lpODPowerLimit);

            [DllImport(Atiadlxx_FileName, CallingConvention = CallingConvention.Cdecl)]
            internal static extern int ADL2_OverdriveN_PowerLimit_Set(IntPtr context, int iAdapterIndex, ref ADLODNPowerLimitSetting lpODPowerLimit);
        }

        internal static AMDDelegateContainer<ADL2_Main_Control_Create> ADL2_Main_Control_Create { get; } = new AMDDelegateContainer<ADL2_Main_Control_Create>(
            "ADL2_Main_Control_Create",
            ADLImport.ADL2_Main_Control_Create);

        internal static AMDDelegateContainer<ADL2_Main_Control_Destroy> ADL2_Main_Control_Destroy { get; } = new AMDDelegateContainer<ADL2_Main_Control_Destroy>(
            "ADL2_Main_Control_Destroy",
            ADLImport.ADL2_Main_Control_Destroy);

        internal static AMDDelegateContainer<ADL2_Adapter_AdapterInfo_Get> ADL2_Adapter_AdapterInfo_Get { get; } = new AMDDelegateContainer<ADL2_Adapter_AdapterInfo_Get>(
            "ADL2_Adapter_AdapterInfo_Get",
            ADLImport.ADL2_Adapter_AdapterInfo_Get);

        internal static AMDDelegateContainer<ADL_Overdrive5_CurrentActivity_Get> ADL_Overdrive5_CurrentActivity_Get { get; } = new AMDDelegateContainer<ADL_Overdrive5_CurrentActivity_Get>(
            "ADL_Overdrive5_CurrentActivity_Get",
            ADLImport.ADL_Overdrive5_CurrentActivity_Get);

        internal static AMDDelegateContainer<ADL_Overdrive5_Temperature_Get> ADL_Overdrive5_Temperature_Get { get; } = new AMDDelegateContainer<ADL_Overdrive5_Temperature_Get>(
            "ADL_Overdrive5_Temperature_Get",
            ADLImport.ADL_Overdrive5_Temperature_Get);

        internal static AMDDelegateContainer<ADL_Overdrive5_FanSpeed_Get> ADL_Overdrive5_FanSpeed_Get { get; } = new AMDDelegateContainer<ADL_Overdrive5_FanSpeed_Get>(
            "ADL_Overdrive5_FanSpeed_Get",
            ADLImport.ADL_Overdrive5_FanSpeed_Get);

        internal static AMDDelegateContainer<ADL2_Overdrive6_CurrentPower_Get> ADL2_Overdrive6_CurrentPower_Get { get; } = new AMDDelegateContainer<ADL2_Overdrive6_CurrentPower_Get>(
            "ADL2_Overdrive6_CurrentPower_Get",
            ADLImport.ADL2_Overdrive6_CurrentPower_Get);

        internal static AMDDelegateContainer<ADL2_Overdrive_Caps> ADL2_Overdrive_Caps { get; } = new AMDDelegateContainer<ADL2_Overdrive_Caps>(
            "ADL2_Overdrive_Caps",
            ADLImport.ADL2_Overdrive_Caps);


        internal static AMDDelegateContainer<ADL2_OverdriveN_CapabilitiesX2_Get> ADL2_OverdriveN_CapabilitiesX2_Get { get; } = new AMDDelegateContainer<ADL2_OverdriveN_CapabilitiesX2_Get>(
            "ADL2_OverdriveN_CapabilitiesX2_Get",
            ADLImport.ADL2_OverdriveN_CapabilitiesX2_Get);

        internal static AMDDelegateContainer<ADL2_OverdriveN_PowerLimit_Get> ADL2_OverdriveN_PowerLimit_Get { get; } = new AMDDelegateContainer<ADL2_OverdriveN_PowerLimit_Get>(
            "ADL2_OverdriveN_PowerLimit_Get",
            ADLImport.ADL2_OverdriveN_PowerLimit_Get);

        internal static AMDDelegateContainer<ADL2_OverdriveN_PowerLimit_Set> ADL2_OverdriveN_PowerLimit_Set { get; } = new AMDDelegateContainer<ADL2_OverdriveN_PowerLimit_Set>(
            "ADL2_OverdriveN_PowerLimit_Set",
            ADLImport.ADL2_OverdriveN_PowerLimit_Set);
    }
}

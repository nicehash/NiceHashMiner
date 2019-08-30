using NHM.DeviceMonitoring.AMD;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ATI.ADL;
using NHM.Common;

namespace NHM.DeviceMonitoring
{
    internal class DeviceMonitorAMD : DeviceMonitor, IFanSpeedRPM, ILoad, IPowerUsage, ITemp
    {
        public int BusID { get; private set; }
        private readonly int _adapterIndex; // For ADL
        private readonly int _adapterIndex2; // For ADL2
        private readonly IntPtr _adlContext;
        private bool _powerHasFailed;

        //private RangeCalculator TDPRangeCalculator;
        private ADLODNCapabilitiesX2 _ADLODNCapabilitiesX2;

        private static readonly TimeSpan _delayedLogging = TimeSpan.FromMinutes(5);

        internal DeviceMonitorAMD(AmdBusIDInfo info)
        {
            UUID = info.Uuid;
            _adapterIndex = info.Adl1Index;
            _adapterIndex2 = info.Adl2Index;
            BusID = info.BusID;
            ADL.ADL2_Main_Control_Create.Delegate?.Invoke(ADL.ADL_Main_Memory_Alloc, 0, ref _adlContext);

            string TEST_TAG = $"ADL_TESTING {BusID}";
            try
            {
                var dADL2_OverdriveN_CapabilitiesX2_Get = ADL.ADL2_OverdriveN_CapabilitiesX2_Get.Delegate;
                if (dADL2_OverdriveN_CapabilitiesX2_Get != null)
                {
                    _ADLODNCapabilitiesX2 = new ADLODNCapabilitiesX2();
                    var ret = dADL2_OverdriveN_CapabilitiesX2_Get(_adlContext, _adapterIndex, ref _ADLODNCapabilitiesX2);
                    if (ret == ADL.ADL_SUCCESS)
                    {
                        Logger.Info(TEST_TAG, _ADLODNCapabilitiesX2.ToString());
                    }
                    else
                    {
                        Logger.Info(TEST_TAG, $"returned {ret}");
                    }
                }
                else
                {
                    // TODO unable to get capabilities
                    Logger.Info(TEST_TAG, $"unable to get capabilities");
                }
            }
            catch (Exception e)
            {
                // TODO log
                Logger.Info(TEST_TAG, e.Message);
            }
        }

        public int FanSpeedRPM
        {
            get
            {
                var adlf = new ADLFanSpeedValue
                {
                    SpeedType = ADL.ADL_DL_FANCTRL_SPEED_TYPE_RPM
                };
                var result = ADL.ADL_Overdrive5_FanSpeed_Get.Delegate(_adapterIndex, 0, ref adlf);
                if (result != ADL.ADL_SUCCESS)
                {
                    Logger.InfoDelayed("ADL", $"ADL fan getting failed with error code {result}", _delayedLogging);
                    return -1;
                }
                return adlf.FanSpeed;
            }
        }

        public float Temp
        {
            get
            {
                var adlt = new ADLTemperature();
                var result = ADL.ADL_Overdrive5_Temperature_Get.Delegate(_adapterIndex, 0, ref adlt);
                if (result != ADL.ADL_SUCCESS)
                {
                    Logger.InfoDelayed("ADL", $"ADL temp getting failed with error code {result}", _delayedLogging);
                    return -1;
                }
                return adlt.Temperature * 0.001f;
            }
        }

        public float Load
        {
            get
            {
                var adlp = new ADLPMActivity();
                var result = ADL.ADL_Overdrive5_CurrentActivity_Get.Delegate(_adapterIndex, ref adlp);
                if (result != ADL.ADL_SUCCESS)
                {
                    Logger.InfoDelayed("ADL", $"ADL load getting failed with error code {result}", _delayedLogging);
                    return -1;
                }
                return adlp.ActivityPercent;
            }
        }

        public double PowerUsage
        {
            get
            {
                var power = -1;
                if (!_powerHasFailed && _adlContext != IntPtr.Zero && ADL.ADL2_Overdrive6_CurrentPower_Get.Delegate != null)
                {
                    var result = ADL.ADL2_Overdrive6_CurrentPower_Get.Delegate(_adlContext, _adapterIndex2, 1, ref power);
                    if (result == ADL.ADL_SUCCESS)
                    {
                        return (double)power / (1 << 8);
                    }

                    // Only alert once
                    Logger.InfoDelayed("ADL", $"ADL power getting failed with code {result} for GPU BusID={BusID}. Turning off power for this GPU.", _delayedLogging);
                    _powerHasFailed = true;
                }

                return power;
            }
        }
    }
}

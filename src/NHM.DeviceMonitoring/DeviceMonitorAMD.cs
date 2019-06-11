using NHM.DeviceMonitoring.AMD;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ATI.ADL;
using NiceHashMinerLegacy.Common;

namespace NHM.DeviceMonitoring
{
    internal class DeviceMonitorAMD : DeviceMonitor, IFanSpeed, ILoad, IPowerUsage, ITemp
    {
        public int BusID { get; private set; }
        private readonly int _adapterIndex; // For ADL
        private readonly int _adapterIndex2; // For ADL2
        private readonly IntPtr _adlContext;
        private bool _powerHasFailed;

        private static readonly TimeSpan _delayedLogging = TimeSpan.FromMinutes(5);

        internal DeviceMonitorAMD(AmdBusIDInfo info)
        {
            UUID = info.Uuid;
            _adapterIndex = info.Adl1Index;
            _adapterIndex2 = info.Adl2Index;
            BusID = info.BusID;
            ADL.ADL2_Main_Control_Create?.Invoke(ADL.ADL_Main_Memory_Alloc, 0, ref _adlContext);
        }

        public int FanSpeed
        {
            get
            {
                var adlf = new ADLFanSpeedValue
                {
                    SpeedType = ADL.ADL_DL_FANCTRL_SPEED_TYPE_RPM
                };
                var result = ADL.ADL_Overdrive5_FanSpeed_Get(_adapterIndex, 0, ref adlf);
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
                var result = ADL.ADL_Overdrive5_Temperature_Get(_adapterIndex, 0, ref adlt);
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
                var result = ADL.ADL_Overdrive5_CurrentActivity_Get(_adapterIndex, ref adlp);
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
                if (!_powerHasFailed && _adlContext != IntPtr.Zero && ADL.ADL2_Overdrive6_CurrentPower_Get != null)
                {
                    var result = ADL.ADL2_Overdrive6_CurrentPower_Get(_adlContext, _adapterIndex2, 1, ref power);
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

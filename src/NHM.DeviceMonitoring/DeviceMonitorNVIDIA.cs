using ManagedCuda.Nvml;
using NHM.Common;
using NHM.DeviceMonitoring.NVIDIA;
using NHM.DeviceMonitoring.TDP;
using NVIDIA.NVAPI;
using System;
using System.Linq;

namespace NHM.DeviceMonitoring
{
    internal class DeviceMonitorNVIDIA : DeviceMonitor, IFanSpeedRPM, ILoad, IPowerUsage, ITemp, ITDP
    {
        private object _lock = new object();

        private static readonly TimeSpan _delayedLogging = TimeSpan.FromMinutes(0.5);

        public int BusID { get; private set; }

        private readonly DeviceMonitorWatchdog _deviceMonitorWatchdog;

        private string LogTag => $"DeviceMonitorNVIDIA-uuid({UUID})-busid({BusID})";

        internal DeviceMonitorNVIDIA(string uuid, int busID, TimeSpan firstMaxTimeoutAfterNvmlRestart)
        {
            UUID = uuid;
            BusID = busID;
            _deviceMonitorWatchdog = new DeviceMonitorWatchdog(firstMaxTimeoutAfterNvmlRestart);
            // recovery backoff attempts
            for (int i = 0; i < 20; i++) _deviceMonitorWatchdog.AppendTimeoutTimeSpan(firstMaxTimeoutAfterNvmlRestart);
            for (int i = 0; i < 20; i++) _deviceMonitorWatchdog.AppendTimeoutTimeSpan(TimeSpan.FromMinutes(1)); // attempt on minute
            for (int i = 0; i < 10; i++) _deviceMonitorWatchdog.AppendTimeoutTimeSpan(TimeSpan.FromHours(1)); // attempt on hour
            for (int i = 0; i < 1; i++) _deviceMonitorWatchdog.AppendTimeoutTimeSpan(TimeSpan.FromDays(1)); // attempt after a day and stop after
        }

        // cache nvmlDevice handles as these calls are expensive
        private nvmlDevice? _nvmlDevice;
        private nvmlDevice GetNvmlDevice()
        {
            if (_nvmlDevice.HasValue) return _nvmlDevice.Value;
            var nvmlHandle = new nvmlDevice();
            var nvmlRet = NvmlNativeMethods.nvmlDeviceGetHandleByUUID(UUID, ref nvmlHandle);
            if (nvmlRet != nvmlReturn.Success)
            {
                throw new NvmlException("nvmlDeviceGetHandleByUUID", nvmlRet);
            }
            _nvmlDevice = nvmlHandle;
            return nvmlHandle;
        }

        // cache NvPhysicalGpuHandle handles as these calls are expensive
        private NvPhysicalGpuHandle? _NvPhysicalGpuHandle;
        private NvPhysicalGpuHandle? GetNvPhysicalGpuHandle()
        {
            if (_NvPhysicalGpuHandle.HasValue) return _NvPhysicalGpuHandle.Value;
            if (NVAPI.NvAPI_EnumPhysicalGPUs == null)
            {
                Logger.DebugDelayed("NVAPI", "NvAPI_EnumPhysicalGPUs unavailable", TimeSpan.FromMinutes(5));
                return null;
            }
            if (NVAPI.NvAPI_GPU_GetBusID == null)
            {
                Logger.DebugDelayed("NVAPI", "NvAPI_GPU_GetBusID unavailable", TimeSpan.FromMinutes(5));
                return null;
            }


            var handles = new NvPhysicalGpuHandle[NVAPI.MAX_PHYSICAL_GPUS];
            var status = NVAPI.NvAPI_EnumPhysicalGPUs(handles, out _);
            if (status != NvStatus.OK)
            {
                Logger.DebugDelayed("NVAPI", $"Enum physical GPUs failed with status: {status}", TimeSpan.FromMinutes(5));
            }
            else
            {
                foreach (var handle in handles)
                {
                    var idStatus = NVAPI.NvAPI_GPU_GetBusID(handle, out var id);

                    if (idStatus == NvStatus.EXPECTED_PHYSICAL_GPU_HANDLE) continue;

                    if (idStatus != NvStatus.OK)
                    {
                        Logger.DebugDelayed("NVAPI", "Bus ID get failed with status: " + idStatus, TimeSpan.FromMinutes(5));
                    }
                    else if (id == BusID)
                    {
                        Logger.DebugDelayed("NVAPI", "Found handle for busid " + id, TimeSpan.FromMinutes(5));
                        _NvPhysicalGpuHandle = handle;
                        return handle;
                    }
                }
            }
            return null;
        }

        public float Load
        {
            get
            {
                var load_perc = NVIDIA_MON.nhm_nvidia_device_get_load_perc(BusID);
                if (load_perc >= 0) return load_perc;
                Logger.InfoDelayed(LogTag, $"nhm_nvidia_device_get_load_perc failed", _delayedLogging);
                return -1;
            }
        }

        public float Temp
        {
            get
            {
                var temp = NVIDIA_MON.nhm_nvidia_device_get_temperature(BusID);
                if (temp < uint.MaxValue) return temp;
                Logger.InfoDelayed(LogTag, $"nhm_nvidia_device_get_temperature failed", _delayedLogging);
                return -1;
            }
        }

        // TODO NVAPI replace with NVML if possible
        public int FanSpeedRPM
        {
            get
            {
                var fan_speed = NVIDIA_MON.nhm_nvidia_device_get_fan_speed_rpm(BusID);
                if (fan_speed >= 0) return fan_speed;
                Logger.InfoDelayed(LogTag, $"nhm_nvidia_device_get_fan_speed_rpm failed", _delayedLogging);
                return -1;
            }
        }

        public double PowerUsage
        {
            get
            {
                
                /*var power_usage = NVIDIA_MON.nhm_nvidia_device_get_power_usage(BusID);
                if (power_usage >= 0) return power_usage;
                Logger.InfoDelayed(LogTag, $"nhm_nvidia_device_get_power_usage failed", _delayedLogging);*/
                return -1;
            }
        }

        // NVML is thread-safe according to the documentation
        private T ExecNvmlProcedure<T>(T failReturn, string tag, Func<T> nvmlExecFun)
        {
            if (!NvidiaMonitorManager.InitalNVMLInitSuccess)
            {
                Logger.ErrorDelayed(LogTag, $"{tag} InitalNVMLInitSuccess==FALSE", TimeSpan.FromMinutes(5));
                return failReturn;
            }
            if (NvidiaMonitorManager.IsNVMLRestarting)
            {
                Logger.ErrorDelayed(LogTag, $"Skipping {tag} NVML IsRestarting", TimeSpan.FromSeconds(5));
                return failReturn;
            }
            try
            {
                var execRet = nvmlExecFun();
                _deviceMonitorWatchdog.Reset(); // if nvmlExecFun doesn't throw we mark this as success
                return execRet;
            }
            catch (Exception e)
            {
                Logger.ErrorDelayed(LogTag, e.ToString(), TimeSpan.FromSeconds(30));
                if (e is NvmlException ne && !SkipNvmlErrorRecovery(ne.ReturnCode))
                {
                    if (_deviceMonitorWatchdog.IsAttemptErrorRecoveryPermanentlyDisabled())
                    {
                        Logger.ErrorDelayed(LogTag, $"{tag} Will NOT RESTART NVML. Recovery for this device is permanently disabled.", TimeSpan.FromSeconds(30));
                        return failReturn;
                    }
                    _deviceMonitorWatchdog.SetErrorTime();
                    var shouldAttemptRestartNvml = _deviceMonitorWatchdog.ShouldAttemptErrorRecovery();
                    if (shouldAttemptRestartNvml)
                    {
                        _deviceMonitorWatchdog.UpdateTickError();
                        Logger.Info(LogTag, $"{tag} Will call NVML restart");
                        NvidiaMonitorManager.AttemptRestartNVML();
                    }
                }
            }
            return failReturn;
        }

        private static nvmlReturn[] _skipRecoveryNvmlErrors = new nvmlReturn[] {
            nvmlReturn.Success,
            nvmlReturn.DriverNotLoaded,
            nvmlReturn.FunctionNotFound,
            nvmlReturn.NotSupported,
            nvmlReturn.NoPermission,
            // check these two
            //nvmlReturn.GPUIsLost, // ????
            //nvmlReturn.ResetRequired, // ????
        };

        private static bool SkipNvmlErrorRecovery(nvmlReturn error)
        {
            return _skipRecoveryNvmlErrors.Contains(error);
        }

        private bool ExecNvmlSetTDP(string calledFrom, double percentage)
        {
            var execRet = ExecNvmlProcedure(false, calledFrom, () => {
                var nvmlDevice = GetNvmlDevice();
                uint minLimit = 0;
                uint maxLimit = 0;
                var ret = NvmlNativeMethods.nvmlDeviceGetPowerManagementLimitConstraints(nvmlDevice, ref minLimit, ref maxLimit);
                if (ret != nvmlReturn.Success)
                    throw new NvmlException($"nvmlDeviceGetPowerManagementLimitConstraints", ret);

                uint defaultLimit = 0;
                ret = NvmlNativeMethods.nvmlDeviceGetPowerManagementDefaultLimit(nvmlDevice, ref defaultLimit);
                if (ret != nvmlReturn.Success)
                    throw new NvmlException($"nvmlDeviceGetPowerManagementDefaultLimit", ret);

                // We limit 100% to the default as max
                var limit = RangeCalculator.CalculateValueNVIDIA(percentage, defaultLimit);
                var setLimit = (uint)limit;
                if (setLimit > maxLimit) setLimit = maxLimit;
                if (setLimit < minLimit) setLimit = minLimit;
                ret = NvmlNativeMethods.nvmlDeviceSetPowerManagementLimit(nvmlDevice, setLimit);
                if (ret != nvmlReturn.Success)
                    throw new NvmlException("nvmlDeviceSetPowerManagementLimit", ret);

                return true;
            });

            return execRet;
        }

        #region ITDP
        public TDPSettingType SettingType { get; set;  } = TDPSettingType.SIMPLE;

        public TDPSimpleType TDPSimple { get; private set; } = TDPSimpleType.HIGH;

        public double TDPPercentage
        {
            get
            {
                var execRet = ExecNvmlProcedure(-1d, $"{nameof(TDPPercentage)}", () => {
                    var nvmlDevice = GetNvmlDevice();
                    uint minLimit = 0;
                    uint maxLimit = 0;
                    var ret = NvmlNativeMethods.nvmlDeviceGetPowerManagementLimitConstraints(nvmlDevice, ref minLimit, ref maxLimit);
                    if (ret != nvmlReturn.Success)
                        throw new NvmlException($"nvmlDeviceGetPowerManagementLimitConstraints", ret);

                    uint defaultLimit = 0;
                    ret = NvmlNativeMethods.nvmlDeviceGetPowerManagementDefaultLimit(nvmlDevice, ref defaultLimit);
                    if (ret != nvmlReturn.Success)
                        throw new NvmlException($"nvmlDeviceGetPowerManagementDefaultLimit", ret);

                    uint currentLimit = 0;
                    ret = NvmlNativeMethods.nvmlDeviceGetPowerManagementLimit(nvmlDevice, ref currentLimit);
                    if (ret != nvmlReturn.Success)
                        throw new NvmlException($"nvmlDeviceGetPowerManagementDefaultLimit", ret);

                    return (double)currentLimit / (double)defaultLimit;
                });
                return execRet;
            }
        }

        private static double? PowerLevelToTDPPercentage(TDPSimpleType level)
        {
            switch (level)
            {
                case TDPSimpleType.LOW: return 0.6d; // 60%
                case TDPSimpleType.MEDIUM: return 0.8d; // 80%
                case TDPSimpleType.HIGH: return 1.0d; // 100%
            }
            return null;
        }
        public bool SetTDPSimple(TDPSimpleType level)
        {
            if (DeviceMonitorManager.DisableDevicePowerModeSettings)
            {
                Logger.InfoDelayed(LogTag, $"SetTDPSimple Disabled DeviceMonitorManager.DisableDevicePowerModeSettings==true", TimeSpan.FromSeconds(30));
                return false;
            }
            var percentage = PowerLevelToTDPPercentage(level);
            if (!percentage.HasValue)
            {
                Logger.Error(LogTag, $"SetTDPSimple unkown PowerLevel {level}. Defaulting to {TDPSimpleType.HIGH}");
                level = TDPSimpleType.HIGH;
                percentage = PowerLevelToTDPPercentage(level);
            }
            Logger.Info(LogTag, $"SetTDPSimple setting PowerLevel to {level}.");
            var execRet = ExecNvmlSetTDP($"{nameof(SetTDPSimple)}({level})", percentage.Value);
            if (execRet) TDPSimple = level;
            Logger.Info(LogTag, $"SetTDPSimple {execRet}.");
            return execRet;
        }

        public bool SetTDPPercentage(double percentage)
        {
            if (DeviceMonitorManager.DisableDevicePowerModeSettings)
            {
                Logger.InfoDelayed(LogTag, $"SetTDPPercentage Disabled DeviceMonitorManager.DisableDevicePowerModeSettings==true", TimeSpan.FromSeconds(30));
                return false;
            }
            if (percentage < 0.0d)
            {
                Logger.Error(LogTag, $"SetTDPPercentage {percentage} out of bounds. Setting to 0.0d");
                percentage = 0.0d;
            }

            Logger.Info(LogTag, $"SetTDPPercentage setting to {percentage}.");
            var execRet = ExecNvmlSetTDP($"{nameof(SetTDPPercentage)}({percentage})", percentage);
            Logger.Info(LogTag, $"SetTDPPercentage {execRet}.");
            return execRet;
        }

        #endregion ITDP
    }
}

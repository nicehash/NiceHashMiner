using ManagedCuda.Nvml;
using NHM.DeviceMonitoring.NVIDIA;
using NHM.Common;
using NHM.Common.Enums;
using NVIDIA.NVAPI;
using System;
using System.Linq;

namespace NHM.DeviceMonitoring
{
    internal class DeviceMonitorNVIDIA : DeviceMonitor,
        IFanSpeed, ILoad, IPowerUsage, ITemp,
        IPowerTarget, IPowerLevel, ISetPowerLevel, ISetPowerTargetPercentage
    {
        public class PowerOutOfRangeException : ArgumentOutOfRangeException
        {
            /// <summary>
            /// The next closest power limit that can be set
            /// </summary>
            public double ClosestValue;

            public PowerOutOfRangeException(double closest)
            {
                ClosestValue = closest;
            }
        }

        private object _lock = new object();

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


            try
            {
                // set to high by default
                var defaultLevel = PowerLevel.High;
                if (!DeviceMonitorManager.DisableDevicePowerModeSettings)
                {
                    var success = SetPowerTarget(defaultLevel);
                    if (!success)
                    {
                        Logger.Info(LogTag, $"Cannot set power target ({defaultLevel.ToString()}) for device with BusID={BusID}");
                    }
                }
                else
                {
                    PowerLevel = PowerLevel.Disabled;
                }
            }
            catch (Exception e)
            {
                Logger.Error(LogTag, $"Getting power info failed with message \"{e.Message}\", disabling power setting");
            }
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
                var execRet = ExecNvmlProcedure(-1f, nameof(Load), () => {
                    var nvmlDevice = GetNvmlDevice();
                    var rates = new nvmlUtilization();
                    var ret = NvmlNativeMethods.nvmlDeviceGetUtilizationRates(nvmlDevice, ref rates);
                    if (ret != nvmlReturn.Success)
                        throw new NvmlException($"nvmlDeviceGetUtilizationRates", ret);

                    var load = (int)rates.gpu;
                    return load;
                });
                return execRet;
            }
        }

        public float Temp
        {
            get
            {
                var execRet = ExecNvmlProcedure(-1f, nameof(Temp), () => {
                    var nvmlDevice = GetNvmlDevice();
                    var utemp = 0u;
                    var ret = NvmlNativeMethods.nvmlDeviceGetTemperature(nvmlDevice, nvmlTemperatureSensors.Gpu,  ref utemp);
                    if (ret != nvmlReturn.Success)
                        throw new NvmlException($"nvmlDeviceGetTemperature", ret);

                    var temp = (float)utemp;
                    return temp;
                });
                return execRet;
            }
        }

        // TODO NVAPI replace with NVML if possible
        public int FanSpeed
        {
            get
            {
                if (!NVAPI.IsAvailable)
                {
                    Logger.ErrorDelayed(LogTag, $"FanSpeed NVAPI.IsAvailable==FALSE", TimeSpan.FromMinutes(5));
                    return -1;
                }
                if (NVAPI.NvAPI_GPU_GetTachReading == null)
                {
                    Logger.ErrorDelayed(LogTag, $"FanSpeed NVAPI.NvAPI_GPU_GetTachReading == null", TimeSpan.FromMinutes(5));
                    return -1;
                }
                var fanSpeed = -1;
                using (var tryLock = new TryLock(_lock))
                {
                    if (!tryLock.HasAcquiredLock)
                    {
                        Logger.Error(LogTag, "FanSpeed Already Locked");
                        return -1;
                    }
                    // we got the lock
                    var nvHandle = GetNvPhysicalGpuHandle();
                    if (!nvHandle.HasValue)
                    {
                        Logger.ErrorDelayed(LogTag, $"FanSpeed nvHandle == null", TimeSpan.FromMinutes(5));
                        return -1;
                    }
                    var result = NVAPI.NvAPI_GPU_GetTachReading(nvHandle.Value, out fanSpeed);
                    if (result != NvStatus.OK && result != NvStatus.NOT_SUPPORTED)
                    {
                        // GPUs without fans are not uncommon, so don't treat as error and just return -1
                        Logger.ErrorDelayed("NVAPI", $"Tach get failed with status: {result}", TimeSpan.FromSeconds(30));
                        // if NVAPI fails... check if we could re-init this as well??
                        return -1;
                    }
                }

                return fanSpeed;
            }
        }

        public double PowerUsage
        {
            get
            {
                var execRet = ExecNvmlProcedure(-1d, nameof(PowerUsage), () => {
                    var nvmlDevice = GetNvmlDevice();
                    var power = 0u;
                    var nvmlRet = NvmlNativeMethods.nvmlDeviceGetPowerUsage(nvmlDevice, ref power);
                    if (nvmlRet != nvmlReturn.Success)
                        throw new NvmlException($"nvmlDeviceGetPowerUsage", nvmlRet);

                    return power * 0.001;
                });
                return execRet;
            }
        }

        public uint PowerTarget { get; private set; }

        public PowerLevel PowerLevel { get; private set; } = PowerLevel.High;

        private static uint GetUintFromPowerLevel(PowerLevel powerLevel)
        {
            switch (powerLevel)
            {
                case PowerLevel.Low: return 0;
                case PowerLevel.Medium: return 1;
                case PowerLevel.High: return 2;
                default: throw new Exception("GetUintFromPowerLevel - Unknown PowerLevel");
            }
        }

        private bool SetTdp(nvmlDevice nvmlDevice, double tdpPerc, uint defaultLimit)
        {
            // TODO we limit to 100%
            uint currentLimit = (uint)(tdpPerc * (double)defaultLimit) / 100;
            var ret = NvmlNativeMethods.nvmlDeviceSetPowerManagementLimit(nvmlDevice, currentLimit);
            if (ret != nvmlReturn.Success)
                throw new NvmlException("nvmlDeviceSetPowerManagementLimit", ret);
            
            return true;
        }

        private bool SetTdpSimple(PowerLevel level)
        {
            var execRet = ExecNvmlProcedure(false, $"{nameof(SetTdpSimple)}({level})", () => {
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

                var limit = minLimit + (defaultLimit - minLimit) * GetUintFromPowerLevel(level) / 2;
                var tdpPerc = (limit * 100.0 / defaultLimit);
                return SetTdp(nvmlDevice, tdpPerc, defaultLimit);
            });
            return execRet;
        }

        private bool SetTdpPercentage(double tdpPerc)
        {
            var execRet = ExecNvmlProcedure(false, $"{nameof(SetTdpPercentage)}({tdpPerc})", () => {
                var nvmlDevice = GetNvmlDevice();
                uint defaultLimit = 0;
                var ret = NvmlNativeMethods.nvmlDeviceGetPowerManagementDefaultLimit(nvmlDevice, ref defaultLimit);
                if (ret != nvmlReturn.Success)
                    throw new NvmlException($"nvmlDeviceGetPowerManagementDefaultLimit", ret);

                return SetTdp(nvmlDevice, tdpPerc, defaultLimit);
            });
            return execRet;
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

        public bool SetPowerTarget(PowerLevel level)
        {
            if (DeviceMonitorManager.DisableDevicePowerModeSettings) {
                Logger.InfoDelayed(LogTag, $"SetPowerTarget Disabled DeviceMonitorManager.DisableDevicePowerModeSettings==true", TimeSpan.FromSeconds(30));
                return false;
            }

            PowerLevel = level;
            return SetTdpSimple(level);
        }

        // percent is in hundreds, e.g. 100%
        public bool SetPowerTarget(double percent)
        {
            if (DeviceMonitorManager.DisableDevicePowerModeSettings)
            {
                Logger.InfoDelayed(LogTag, $"SetPowerTarget Disabled DeviceMonitorManager.DisableDevicePowerModeSettings==true", TimeSpan.FromSeconds(30));
                return false;
            }
            // TODO this Custom thingh shouild be changed so each mode has a custom setting
            PowerLevel = PowerLevel.Custom;
            return SetTdpPercentage(percent);
        }
    }
}

using NHM.Common;
using NHMCore.Mining;
using NHMCore.Utils;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Google.Protobuf;

using static NHMCore.Scripts.LibJSBridge;
using NHM.Common.Enums;
using NHM.DeviceMonitoring;
using NHMCore.Switching;

namespace NHMCore.Scripts
{
    public static class JSBridge
    {
        public delegate void OnJSError(string error, string stack, Int64 script_id);
        public static OnJSError OnJSErrorCallback = null;

        private static DeviceInfo.Types.Status ToProtobufDeviceStatus(DeviceState deviceState)
        {
            switch(deviceState)
            {
                case DeviceState.Stopped: return DeviceInfo.Types.Status.Stopped;
                case DeviceState.Mining: return DeviceInfo.Types.Status.Mining;
                case DeviceState.Benchmarking: return DeviceInfo.Types.Status.Benchmarking;
                case DeviceState.Error: return DeviceInfo.Types.Status.Error;
                case DeviceState.Pending: return DeviceInfo.Types.Status.Pending;
                case DeviceState.Disabled: return DeviceInfo.Types.Status.Disabled;
            }
            return DeviceInfo.Types.Status.NotSetInvalid;
        }
        private static DeviceInfo.Types.Vendor ToProtobufDeviceVendor(ComputeDevice dev)
        {
            if (dev.DeviceType == DeviceType.NVIDIA) return DeviceInfo.Types.Vendor.Nvidia;
            if (dev.DeviceType == DeviceType.AMD) return DeviceInfo.Types.Vendor.Amd;
            if (dev.Name.ToLower().Contains("intel")) return DeviceInfo.Types.Vendor.Intel;
            return DeviceInfo.Types.Vendor.Amd;
        }
        private static DeviceInfo ToDeviceInfo(ComputeDevice dev)
        {
            return new DeviceInfo
            {
                DeviceId = dev.Uuid,
                Name = dev.Name,
                Type = dev.DeviceType == DeviceType.CPU ? DeviceInfo.Types.Type.Cpu : DeviceInfo.Types.Type.Gpu,
                Vendor = ToProtobufDeviceVendor(dev),                
                Enabled = dev.Enabled,
                Load = dev.Load,
                Status = ToProtobufDeviceStatus(dev.State),
                Temperature = dev.Temp,
                FanSpeed = dev.FanSpeed,
                MaxFanSpeed = 100,
                MinFanSpeed = 0,
            };
        }

        private static MinerAlgorithmPair ToMinerAlgorithmPair(AlgorithmContainer algo)
        {
            var minerAlgoPair = new MinerAlgorithmPair {
                //ActiveMiningSpeeds -> for active only
                //AlgorithmIds -> fill in loop
                AlgorithmName = algo.AlgorithmName,
                //BenchmarkedSpeeds 
                ExtraLaunchParameters = algo.ExtraLaunchParameters,
                MinerId = algo.PluginContainer.PluginUUID,
                MinerName = algo.PluginName,
                PowerConsumption = (int)algo.PowerUsage, // TODO get back to this
                
            };
            minerAlgoPair.AlgorithmIds.AddRange(algo.IDs.Select(id => (int)id));
            minerAlgoPair.BenchmarkedSpeeds.AddRange(algo.Speeds);
            return minerAlgoPair;
        }

        private static DeviceAlgorithmsInfo ToDeviceAlgorithmsInfo(ComputeDevice dev)
        {
            var ret = new DeviceAlgorithmsInfo
            {
                DeviceId = dev.Uuid,
                // assume NO algorithm is active
                MiningAlgorithm = new MinerAlgorithmPair
                {
                    AlgorithmName = "NONE",
                    MinerId = "NONE",
                    MinerName = "NONE"
                },
            };
            // check active algo
            var miningAlgo = dev.AlgorithmSettings.Where(algo => algo.Status == AlgorithmStatus.Mining).FirstOrDefault();
            if (miningAlgo != null && dev.State == DeviceState.Mining)
            {
                ret.MiningAlgorithm = ToMinerAlgorithmPair(miningAlgo);
                var speeds = MiningDataStats.GetSpeedForDevice(dev.Uuid);
                if (speeds != null)
                {
                    ret.MiningAlgorithm.ActiveMiningSpeeds.AddRange(speeds.Select(pair => pair.speed));
                }
            }

            ret.Algorithms.AddRange(dev.AlgorithmSettings.Select(algo => ToMinerAlgorithmPair(algo)));

            return ret;
        }

        public static void RegisterNHN_CSharp_JS_Bridge()
        {
            nhms_reg_runtime_error_log_cb((string error) =>
            {
                Logger.Error("JSBridge.Log", $"DLL error {error}");
            });

            nhms_reg_unhandeled_js_error_cb((string error, string stack, Int64 script_id) => {
                Logger.Error("JSBridge.Log", $"Unhandeled JavaScript error {error}.\nStack {stack}.\nScriptID {script_id}");
                OnJSErrorCallback?.Invoke(error, stack, script_id);
            });

            var ok = nhms_init_runtime_and_context();
            if (ok != 0) {
                Logger.Error("JSBridge.Log", $"nhms_init_runtime_and_context error code  {ok}.");
                return;
            }
            #region JS regs
            bridge_nhms_reg_js_console_print((IntPtr buffer, long in_size, ref long out_size) => {
                try
                {
                    var in_buff = new byte[in_size];
                    Marshal.Copy(buffer, in_buff, 0, (int)in_size);
                    ConsolePrint in_msg = ConsolePrint.Parser.ParseFrom(in_buff);
                    Logger.Info("JSBridge.Log", $"JS_LOG:\n\t: {in_msg.Message}.");
                    Void out_msg = new Void { };
                    out_size = out_msg.CalculateSize();
                    Marshal.Copy(out_msg.ToByteArray(), 0, buffer, out_msg.CalculateSize());
                    return 0;
                }
                catch (Exception e)
                {
                    Logger.Error("JSBridge.Log", $"bridge_nhms_reg_js_console_print_cb error {e}.");
                    return -1;
                }
            });

            bridge_nhms_reg_js_get_devices_info((IntPtr buffer, long in_size, ref long out_size) => {
                try
                {
                    DevicesInfos out_msg = new DevicesInfos { };
                    var add_devices = AvailableDevices.Devices.Select(dev => ToDeviceInfo(dev));
                    out_msg.Devices.AddRange(add_devices);
                    out_size = out_msg.CalculateSize();
                    Marshal.Copy(out_msg.ToByteArray(), 0, buffer, out_msg.CalculateSize());
                    return 0;
                }
                catch (Exception e)
                {
                    Logger.Error("JSBridge.Log", $"bridge_nhms_reg_js_get_devices_info_cb error {e}.");
                    return -1;
                }
            });
            bridge_nhms_reg_js_get_device_info((IntPtr buffer, long in_size, ref long out_size) => {
                try
                {
                    var in_buff = new byte[in_size];
                    Marshal.Copy(buffer, in_buff, 0, (int)in_size);
                    DeviceID in_msg = DeviceID.Parser.ParseFrom(in_buff);
                    IMessage out_msg;
                    int ret = 0;
                    var targetDev = AvailableDevices.Devices.Where(dev => dev.Uuid == in_msg.DeviceId).FirstOrDefault();
                    if (targetDev == null)
                    {
                        StatusMessage err = new StatusMessage();
                        err.Status = -1;
                        err.Message = "Device not found.";
                        out_msg = err;
                        ret = -1;
                    }
                    else
                    {
                        DeviceInfo out_ok = ToDeviceInfo(targetDev);
                        out_msg = out_ok;
                    }
                    out_size = out_msg.CalculateSize();
                    Marshal.Copy(out_msg.ToByteArray(), 0, buffer, out_msg.CalculateSize());
                    return ret;
                }
                catch (Exception e)
                {
                    Logger.Error("JSBridge.Log", $"bridge_nhms_reg_js_get_device_info error {e}.");
                    return -1;
                }
            });
            bridge_nhms_reg_js_set_device_fan_speed((IntPtr buffer, long in_size, ref long out_size) => {
                try
                {
                    var in_buff = new byte[in_size];
                    Marshal.Copy(buffer, in_buff, 0, (int)in_size);
                    SetDeviceFanSpeed in_msg = SetDeviceFanSpeed.Parser.ParseFrom(in_buff);
                    StatusMessage out_msg = new StatusMessage();
                    var targetDev = AvailableDevices.Devices.Where(dev => dev.Uuid == in_msg.DeviceId).FirstOrDefault();
                    if (targetDev == null)
                    {
                        out_msg.Status = -1;
                        out_msg.Message = "Device not found.";
                    }
                    else
                    {
                        if (targetDev.DeviceMonitor is IFanSpeedRPM setFanSpeed)
                        {
                            out_msg.Status = 0;
                            out_msg.Message = $"";
                            try
                            {
                                setFanSpeed.SetFanSpeedPercentage(in_msg.FanSpeed);
                            }
                            catch (Exception e)
                            {
                                out_msg.Status = 0;
                                out_msg.Message = $"Device {targetDev.Uuid} failed while setting fan speed {e}";
                            }
                        }
                        else
                        {
                            out_msg.Status = -1;
                            out_msg.Message = $"Device {targetDev.Uuid} doesn't support set fan speed";
                        }
                    }
                    out_size = out_msg.CalculateSize();
                    Marshal.Copy(out_msg.ToByteArray(), 0, buffer, out_msg.CalculateSize());
                    return 0;
                }
                catch (Exception e)
                {
                    Logger.Error("JSBridge.Log", $"bridge_nhms_reg_js_set_device_fan_speed error {e}.");
                    return -1;
                }
            });
            bridge_nhms_reg_js_get_sma_data((IntPtr buffer, long in_size, ref long out_size) => {
                try
                {
                    SMAEntries out_msg = new SMAEntries();
                    foreach (var pair in NHSmaData.CurrentPayingRatesSnapshot())
                    {
                        out_msg.Entries.Add(new SMAEntry {
                            AlgorithmId = (int)pair.Key,
                            Paying = pair.Value,
                            IsStable = NHSmaData.IsAlgorithmStable(pair.Key),
                        });
                    }
                    out_size = out_msg.CalculateSize();
                    Marshal.Copy(out_msg.ToByteArray(), 0, buffer, out_msg.CalculateSize());
                    return 0;
                }
                catch (Exception e)
                {
                    Logger.Error("JSBridge.Log", $"bridge_nhms_reg_js_get_sma_data error {e}.");
                    return -1;
                }
            });
            bridge_nhms_reg_js_get_devices_algorithm_info((IntPtr buffer, long in_size, ref long out_size) => {
                try
                {
                    DevicesAlgorithms out_msg = new DevicesAlgorithms();
                    out_msg.EnabledDevices.AddRange(AvailableDevices.Devices.Select(dev => ToDeviceAlgorithmsInfo(dev)));
                    out_size = out_msg.CalculateSize();
                    Marshal.Copy(out_msg.ToByteArray(), 0, buffer, out_msg.CalculateSize());
                    return 0;
                }
                catch (Exception e)
                {
                    Logger.Error("JSBridge.Log", $"bridge_nhms_reg_js_get_devices_algorithm_info error {e}.");
                    return -1;
                }
            });
            bridge_nhms_reg_js_update_device_mining_state((IntPtr buffer, long in_size, ref long out_size) => {
                try
                {
                    
                    var in_buff = new byte[in_size];
                    Marshal.Copy(buffer, in_buff, 0, (int)in_size);
                    UpdateDeviceMiningState in_msg = UpdateDeviceMiningState.Parser.ParseFrom(in_buff);
                    StatusMessage out_msg = new StatusMessage();
                    // don't wait promise change
                    MiningManager.SetSwitchScriptMineState(in_msg.DeviceId, in_msg.MinerId, in_msg.AlgorithmIds.Select(id => (AlgorithmType)id).ToList());
                    out_size = out_msg.CalculateSize();
                    Marshal.Copy(out_msg.ToByteArray(), 0, buffer, out_msg.CalculateSize());
                    return 0;
                }
                catch (Exception e)
                {
                    Logger.Error("JSBridge.Log", $"bridge_nhms_reg_js_update_device_mining_state error {e}.");
                    return -1;
                }
            });
            bridge_nhms_reg_js_set_device_enabled_state((IntPtr buffer, long in_size, ref long out_size) => {
                try
                {
                    var in_buff = new byte[in_size];
                    Marshal.Copy(buffer, in_buff, 0, (int)in_size);
                    SetDeviceEnabledState in_msg = SetDeviceEnabledState.Parser.ParseFrom(in_buff);
                    StatusMessage out_msg = new StatusMessage();
                    var device = AvailableDevices.GetDeviceWithUuidOrB64Uuid(in_msg.DeviceId);
                    if (device == null)
                    {
                        out_msg.Status = -1;
                        out_msg.Message = $"Error unable to find device with ID '{in_msg.DeviceId}'";
                    }
                    else if (device.Enabled == in_msg.Enabled)
                    {
                        out_msg.Status = -1; // is this error?
                        out_msg.Message = $"Device with ID '{in_msg.DeviceId}' already set to desired state";
                    }
                    else 
                    {
                        Task.Run(() => ApplicationStateManager.SetDeviceEnabledState(null, (device.Uuid, in_msg.Enabled))).Wait();
                        out_msg.Status = 0; 
                        out_msg.Message = $"Device set enabled='{in_msg.Enabled}' with ID '{in_msg.DeviceId}' Success";
                    }
                    out_size = out_msg.CalculateSize();
                    Marshal.Copy(out_msg.ToByteArray(), 0, buffer, out_msg.CalculateSize());
                    return 0;
                }
                catch (Exception e)
                {
                    Logger.Error("JSBridge.Log", $"bridge_nhms_reg_js_set_device_enabled_state error {e}.");
                    return -1;
                }
            });
            bridge_nhms_reg_js_start_device((IntPtr buffer, long in_size, ref long out_size) => {
                try
                {
                    var in_buff = new byte[in_size];
                    Marshal.Copy(buffer, in_buff, 0, (int)in_size);
                    DeviceID in_msg = DeviceID.Parser.ParseFrom(in_buff);
                    StatusMessage out_msg = new StatusMessage();
                    var (success, msg, code) = Task.Run(() => ApplicationStateManager.StartDeviceWithUUIDTask(in_msg.DeviceId)).Result;
                    if (!success)
                    {
                        out_msg.Message = msg;
                        out_msg.Status = -(int)(code);
                    }
                    else
                    {
                        out_msg.Message = "";
                        out_msg.Status = 0;
                    }
                    out_size = out_msg.CalculateSize();
                    Marshal.Copy(out_msg.ToByteArray(), 0, buffer, out_msg.CalculateSize());
                    return 0;
                }
                catch (Exception e)
                {
                    Logger.Error("JSBridge.Log", $"bridge_nhms_reg_js_start_device error {e}.");
                    return -1;
                }
            });

            bridge_nhms_reg_js_stop_device((IntPtr buffer, long in_size, ref long out_size) => {
                try
                {
                    var in_buff = new byte[in_size];
                    Marshal.Copy(buffer, in_buff, 0, (int)in_size);
                    DeviceID in_msg = DeviceID.Parser.ParseFrom(in_buff);
                    StatusMessage out_msg = new StatusMessage();
                    var (success, msg, code) = Task.Run(() => ApplicationStateManager.StopDeviceWithUUIDTask(in_msg.DeviceId)).Result;
                    if (!success)
                    {
                        out_msg.Message = msg;
                        out_msg.Status = -(int)(code);
                    }
                    else
                    {
                        out_msg.Message = "";
                        out_msg.Status = 0;
                    }
                    out_size = out_msg.CalculateSize();
                    Marshal.Copy(out_msg.ToByteArray(), 0, buffer, out_msg.CalculateSize());
                    return 0;
                }
                catch (Exception e)
                {
                    Logger.Error("JSBridge.Log", $"bridge_nhms_reg_js_stop_device error {e}.");
                    return -1;
                }
            });

            bridge_nhms_reg_js_set_device_miner_algorithm_pair_enabled_state((IntPtr buffer, long in_size, ref long out_size) => {
                try
                {
                    var in_buff = new byte[in_size];
                    Marshal.Copy(buffer, in_buff, 0, (int)in_size);
                    SetDeviceMinerAlgorithmPairEnabledState in_msg = SetDeviceMinerAlgorithmPairEnabledState.Parser.ParseFrom(in_buff);
                    StatusMessage out_msg = new StatusMessage();

                    var deviceWithUUID = AvailableDevices.GetDeviceWithUuidOrB64Uuid(in_msg.DeviceId);
                    if (deviceWithUUID == null)
                    {
                        out_msg.Status = -1;
                        out_msg.Message = $"Error unable to find device with ID '{in_msg.DeviceId}'";
                    }
                    else
                    {
                        var targetMinerAlgorithmPair = deviceWithUUID.AlgorithmSettings.Where(algo => algo.MinerUUID == in_msg.MinerId)
                                                        .Where(algo => algo.IDs.Count() == in_msg.AlgorithmIds.Count)
                                                        .Where(algo => algo.IDs.Zip(in_msg.AlgorithmIds, (a,b) => (int)a == b).All(equal => equal))
                                                        .FirstOrDefault();
                        if (targetMinerAlgorithmPair == null)
                        {
                            out_msg.Status = -2;
                            var targetMinerAlgoPair = $"{in_msg.MinerId}-[{string.Join(",", in_msg.AlgorithmIds.Select(id => id.ToString()))}]";
                            out_msg.Message = $"Error unable to find miner algorithm pair {targetMinerAlgoPair} for device with ID '{in_msg.DeviceId}'";
                        }
                        else
                        {
                            targetMinerAlgorithmPair.Enabled = in_msg.Enabled;
                        }
                    }

                    out_size = out_msg.CalculateSize();
                    Marshal.Copy(out_msg.ToByteArray(), 0, buffer, out_msg.CalculateSize());
                    return 0;
                }
                catch (Exception e)
                {
                    Logger.Error("JSBridge.Log", $"bridge_nhms_reg_js_set_device_miner_algorithm_pair_enabled_state error {e}.");
                    return -1;
                }
            });
            #endregion JS regs
            // commit to context after we hook our callbacks
            ok = nhms_commit_javascript_callbacks_to_context();
            if (ok != 0) {
                Logger.Error("JSBridge.Log", $"nhms_commit_javascript_callbacks_to_context error code  {ok}.");
                return;
            }
        }

        private static void InitNhmScript()
        {
            try
            {
                RegisterNHN_CSharp_JS_Bridge();
            }
            catch (Exception e)
            {
                Console.WriteLine($"Message: {e.Message}");
                Console.WriteLine($"StackTrace: {e.StackTrace}");
                throw;
            }
        }

        public static Task RunninLoops { get; private set; } = null;

        public static void StartLoops(CancellationToken stop)
        {
            RunninLoops = Start(stop);
        }

        static private async Task Start(CancellationToken token)
        {
            try
            {
                Logger.Info("JSBridge", "Starting JavaScript runtime watchdog");
                while (!token.IsCancellationRequested)
                {
                    try
                    {
                        InitNhmScript();
                        await NewJSLoop(token);
                    }
                    catch (TaskCanceledException e)
                    {
                        Logger.Debug("JSBridge", $"TaskCanceledException {e.Message}");
                        return;
                    }
                    catch (Exception e)
                    {
                        Logger.Error("JSBridge", $"Error occured: {e.Message}");
                    }
                    finally
                    {
                        nhms_de_init_runtime_and_context();
                    }
                }
            }
            finally
            {
                Logger.Info("JSBridge", "Ending JS runtime watchdog");
            }
        }

        public static long AddJSScript(string jsCode, bool isSwitching)
        {
            try
            {
                if (isSwitching) return nhms_add_switching_js_script(jsCode); 
                return nhms_add_js_script(jsCode);
            }
            catch (SEHException e)
            {
                Logger.Error("JSBridge", $"SEHException {e.Message}");
                Logger.Error("JSBridge", $"SEHException {e.HResult}");
                Logger.Error("JSBridge", $"SEHException {e.StackTrace}");
            }
            catch
            {
                Logger.Error("JSBridge", $"SEHException???");
            }
            return -1;
        }

        public static long RemoveJSScrip(long scriptID)
        {
            return nhms_remove_js_script(scriptID);
        }

        public static void RemoveScript(int id)
        {
            nhms_remove_js_script(id);
        }

        public static void AddScriptAndTick(string jsCode)
        {
            nhms_add_js_script(jsCode);
            nhms_js_tick();
        }

        static private async Task NewJSLoop(CancellationToken stop)
        {
            Logger.Info("JSBridge", "STARTING JS Loop");
            try
            {
                var jsTickTime = TimeSpan.FromSeconds(1);
                Logger.Info("JSBridge", "Starting Loop");
                while (!stop.IsCancellationRequested)
                {
                    if (!stop.IsCancellationRequested) await TaskHelpers.TryDelay(jsTickTime, stop);
                    nhms_js_tick();
                }
                Logger.Info("JSBridge", "Exited Loop");
            }
            catch (TaskCanceledException e)
            {
                Logger.Debug("JSBridge", $"TaskCanceledException {e.Message}");
            }
        }
    }
}

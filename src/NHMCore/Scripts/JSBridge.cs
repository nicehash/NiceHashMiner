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
    public static partial class JSBridge
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
                JSLogicDelegate logic = (byte[] in_buff) => {
                    ConsolePrint in_msg = ConsolePrint.Parser.ParseFrom(in_buff);
                    Logger.Info("JSBridge.Log", $"JS_LOG:\n\t: {in_msg.Message}.");
                    return new Void {};
                };
                return HandleProtoMessageHelper("bridge_nhms_reg_js_console_print_cb", logic, buffer, in_size, ref out_size);
            });

            bridge_nhms_reg_js_get_devices_info((IntPtr buffer, long in_size, ref long out_size) => {
                JSLogicDelegate logic = (byte[] in_buff) => {
                    DevicesInfos out_msg = new DevicesInfos { };
                    var add_devices = AvailableDevices.Devices.Select(dev => ToDeviceInfo(dev));
                    out_msg.Devices.AddRange(add_devices);
                    return out_msg;
                };
                return HandleProtoMessageHelper("bridge_nhms_reg_js_get_devices_info_cb", logic, buffer, in_size, ref out_size);
            });
            bridge_nhms_reg_js_get_device_info((IntPtr buffer, long in_size, ref long out_size) => {
                JSLogicDelegate logic = (byte[] in_buff) => {
                    DeviceID in_msg = DeviceID.Parser.ParseFrom(in_buff);
                    var targetDev = AvailableDevices.Devices.Where(dev => dev.Uuid == in_msg.DeviceId).FirstOrDefault();
                    if (targetDev == null) throw new JSBridgeAppException { Status = -1, Message = "Device not found." };
                    return ToDeviceInfo(targetDev);
                };
                return HandleProtoMessageHelper("bridge_nhms_reg_js_get_device_info", logic, buffer, in_size, ref out_size);
            });
            bridge_nhms_reg_js_set_device_fan_speed((IntPtr buffer, long in_size, ref long out_size) => {
                JSLogicDelegate logic = (byte[] in_buff) => {
                    SetDeviceFanSpeed in_msg = SetDeviceFanSpeed.Parser.ParseFrom(in_buff);
                    var targetDev = AvailableDevices.Devices.Where(dev => dev.Uuid == in_msg.DeviceId).FirstOrDefault();
                    if (targetDev == null) return  new StatusMessage { Status = -1, Message = "Device not found." };
                    if (targetDev.DeviceMonitor is IFanSpeedRPM setFanSpeed) {
                        try
                        {
                            setFanSpeed.SetFanSpeedPercentage(in_msg.FanSpeed);
                            return new StatusMessage { Status = 0, Message = "" };
                        }
                        catch (Exception e)
                        {
                            return new StatusMessage { Status = -2, Message = $"Device {targetDev.Uuid} failed while setting fan speed {e}" };
                        }
                    }
                    return new StatusMessage { Status = -1, Message = $"Device {targetDev.Uuid} doesn't support set fan speed" };
                };
                return HandleProtoMessageHelper("bridge_nhms_reg_js_set_device_fan_speed", logic, buffer, in_size, ref out_size);
            });
            bridge_nhms_reg_js_get_sma_data((IntPtr buffer, long in_size, ref long out_size) => {
                JSLogicDelegate logic = (byte[] in_buff) => {
                    SMAEntries out_msg = new SMAEntries();
                    foreach (var pair in NHSmaData.CurrentPayingRatesSnapshot())
                    {
                        out_msg.Entries.Add(new SMAEntry
                        {
                            AlgorithmId = (int)pair.Key,
                            Paying = pair.Value,
                            IsStable = NHSmaData.IsAlgorithmStable(pair.Key),
                        });
                    }
                    return out_msg;
                };
                return HandleProtoMessageHelper("bridge_nhms_reg_js_get_sma_data", logic, buffer, in_size, ref out_size);
            });
            bridge_nhms_reg_js_get_devices_algorithm_info((IntPtr buffer, long in_size, ref long out_size) => {
                JSLogicDelegate logic = (byte[] in_buff) => {
                    DevicesAlgorithms out_msg = new DevicesAlgorithms();
                    out_msg.EnabledDevices.AddRange(AvailableDevices.Devices.Select(dev => ToDeviceAlgorithmsInfo(dev)));
                    return out_msg;
                };
                return HandleProtoMessageHelper("bridge_nhms_reg_js_get_devices_algorithm_info", logic, buffer, in_size, ref out_size);
            });
            bridge_nhms_reg_js_update_device_mining_state((IntPtr buffer, long in_size, ref long out_size) => {
                JSLogicDelegate logic = (byte[] in_buff) => {
                    UpdateDeviceMiningState in_msg = UpdateDeviceMiningState.Parser.ParseFrom(in_buff);
                    MiningManager.SetSwitchScriptMineState(in_msg.DeviceId, in_msg.MinerId, in_msg.AlgorithmIds.Select(id => (AlgorithmType)id).ToList());
                    // TODO return some message
                    StatusMessage out_msg = new StatusMessage();
                    return out_msg;
                };
                return HandleProtoMessageHelper("bridge_nhms_reg_js_update_device_mining_state", logic, buffer, in_size, ref out_size);
            });
            bridge_nhms_reg_js_set_device_enabled_state((IntPtr buffer, long in_size, ref long out_size) => {
                JSLogicDelegate logic = (byte[] in_buff) => {
                    SetDeviceEnabledState in_msg = SetDeviceEnabledState.Parser.ParseFrom(in_buff);
                    var device = AvailableDevices.GetDeviceWithUuidOrB64Uuid(in_msg.DeviceId);
                    if (device == null) return new StatusMessage { Status = -1, Message = $"Error unable to find device with ID '{in_msg.DeviceId}'" };
                    // is this error?
                    if (device.Enabled == in_msg.Enabled) return new StatusMessage { Status = -1, Message = $"Device with ID '{in_msg.DeviceId}' already set to desired state" };

                    Task.Run(() => ApplicationStateManager.SetDeviceEnabledState(null, (device.Uuid, in_msg.Enabled))).Wait();
                    return new StatusMessage { Status = 0, Message = $"Device set enabled='{in_msg.Enabled}' with ID '{in_msg.DeviceId}' Success" };
                };
                return HandleProtoMessageHelper("bridge_nhms_reg_js_set_device_enabled_state", logic, buffer, in_size, ref out_size);
            });
            bridge_nhms_reg_js_start_device((IntPtr buffer, long in_size, ref long out_size) => {
                JSLogicDelegate logic = (byte[] in_buff) => {
                    DeviceID in_msg = DeviceID.Parser.ParseFrom(in_buff);
                    StatusMessage out_msg = new StatusMessage { Status = 0, Message = "" };
                    var (success, msg, code) = Task.Run(() => ApplicationStateManager.StartDeviceWithUUIDTask(in_msg.DeviceId)).Result;
                    if (!success) out_msg = new StatusMessage { Status = -(int)(code), Message = msg };
                    return out_msg;
                };
                return HandleProtoMessageHelper("bridge_nhms_reg_js_start_device", logic, buffer, in_size, ref out_size);
            });

            bridge_nhms_reg_js_stop_device((IntPtr buffer, long in_size, ref long out_size) => {
                JSLogicDelegate logic = (byte[] in_buff) => {
                    DeviceID in_msg = DeviceID.Parser.ParseFrom(in_buff);
                    StatusMessage out_msg = new StatusMessage { Status = 0, Message = "" };
                    var (success, msg, code) = Task.Run(() => ApplicationStateManager.StopDeviceWithUUIDTask(in_msg.DeviceId)).Result;
                    if (!success) out_msg = new StatusMessage { Status = -(int)(code), Message = msg };
                    return out_msg;
                };
                return HandleProtoMessageHelper("bridge_nhms_reg_js_stop_device", logic, buffer, in_size, ref out_size);
            });

            //bridge_nhms_reg_js_set_device_miner_algorithm_pair_enabled_state((IntPtr buffer, long in_size, ref long out_size) => {
            //    JSLogicDelegate logic = (byte[] in_buff) => {
            //        SetDeviceMinerAlgorithmPairEnabledState in_msg = SetDeviceMinerAlgorithmPairEnabledState.Parser.ParseFrom(in_buff);

            //        var deviceWithUUID = AvailableDevices.GetDeviceWithUuidOrB64Uuid(in_msg.DeviceId);
            //        if (deviceWithUUID == null) return new StatusMessage { Status = -1, Message = $"Error unable to find device with ID '{in_msg.DeviceId}'" };

            //        StatusMessage out_msg = new StatusMessage();
            //        var targetMinerAlgorithmPair = deviceWithUUID.AlgorithmSettings.Where(algo => algo.MinerUUID == in_msg.MinerId)
            //                                            .Where(algo => algo.IDs.Count() == in_msg.AlgorithmIds.Count)
            //                                            .Where(algo => algo.IDs.Zip(in_msg.AlgorithmIds, (a, b) => (int)a == b).All(equal => equal))
            //                                            .FirstOrDefault();
            //        if (targetMinerAlgorithmPair == null)
            //        {
            //            out_msg.Status = -2;
            //            var targetMinerAlgoPair = $"{in_msg.MinerId}-[{string.Join(",", in_msg.AlgorithmIds.Select(id => id.ToString()))}]";
            //            out_msg.Message = $"Error unable to find miner algorithm pair {targetMinerAlgoPair} for device with ID '{in_msg.DeviceId}'";
            //        }
            //        targetMinerAlgorithmPair.Enabled = in_msg.Enabled;
            //        return new StatusMessage { Status = 0, Message = "" };
            //    };
            //    return HandleProtoMessageHelper("bridge_nhms_reg_js_set_device_miner_algorithm_pair_enabled_state", logic, buffer, in_size, ref out_size);
            //});
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

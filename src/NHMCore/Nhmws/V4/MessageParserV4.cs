using HidSharp;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NHM.Common;
using NHM.Common.Device;
using NHM.Common.Enums;
using NHM.DeviceMonitoring;
using NHM.DeviceMonitoring.Core_clock;
using NHM.DeviceMonitoring.Core_voltage;
using NHM.DeviceMonitoring.Memory_clock;
using NHM.DeviceMonitoring.TDP;
using NHMCore.Configs;
using NHMCore.Configs.Data;
using NHMCore.Configs.Managers;
using NHMCore.Mining;
using NHMCore.Mining.MiningStats;
using NHMCore.Notifications;
using NHMCore.Schedules;
using NHMCore.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace NHMCore.Nhmws.V4
{
    static class MessageParserV4
    {
        private static readonly string _TAG = "MessageParserV4";
        private static Dictionary<string, List<DeviceDynamicProperties>> IgnoredValues = new();
        internal static IMethod ParseMessage(string jsonData)
        {
            var method = MessageParser.ParseMessageData(jsonData);
            return method switch
            {
                // non rpc
                "sma" => JsonConvert.DeserializeObject<SmaMessage>(jsonData),
                "markets" => new ObsoleteMessage { Method = method },
                "balance" => JsonConvert.DeserializeObject<BalanceMessage>(jsonData),
                "versions" => JsonConvert.DeserializeObject<VersionsMessage>(jsonData),
                "burn" => JsonConvert.DeserializeObject<BurnMessage>(jsonData),
                "exchange_rates" => JsonConvert.DeserializeObject<ExchangeRatesMessage>(jsonData),
                // rpc
                "mining.set.username" => JsonConvert.DeserializeObject<MiningSetUsername>(jsonData),
                "mining.set.worker" => JsonConvert.DeserializeObject<MiningSetWorker>(jsonData),
                "mining.set.group" => JsonConvert.DeserializeObject<MiningSetGroup>(jsonData),
                "mining.enable" => JsonConvert.DeserializeObject<MiningEnable>(jsonData),
                "mining.disable" => JsonConvert.DeserializeObject<MiningDisable>(jsonData),
                "mining.start" => JsonConvert.DeserializeObject<MiningStart>(jsonData),
                "mining.stop" => JsonConvert.DeserializeObject<MiningStop>(jsonData),
                "mining.set.power_mode" => JsonConvert.DeserializeObject<MiningSetPowerMode>(jsonData),
                "miner.reset" => JsonConvert.DeserializeObject<MinerReset>(jsonData),
                "miner.call.action" => JsonConvert.DeserializeObject<MinerCallAction>(jsonData),
                "miner.set.mutable" => JsonConvert.DeserializeObject<MinerSetMutable>(jsonData),
                // non supported
                _ => throw new Exception($"Unable to deserialize '{jsonData}' got method '{method}'."),
            };
        }
        internal static int NHMDeviceTypeToNHMWSDeviceType(DeviceType dt)
        {
            //rig manager enum
            //const deviceClasses = ['UNKNOWN','CPU','NVIDIA','AMD','ASIC',5,6,7,8,9,'ASIC']
            return dt switch
            {
                DeviceType.CPU => 1,
                DeviceType.NVIDIA => 2,
                DeviceType.AMD => 3,
                DeviceType.INTEL => 4, 
                _ => 0
            };
        }

        internal static IOrderedEnumerable<ComputeDevice> SortedDevices(this IEnumerable<ComputeDevice> devices)
        {
            return devices.OrderBy(d => d.DeviceType)
                .ThenBy(d => d.BaseDevice is IGpuDevice gpu ? gpu.PCIeBusID : int.MinValue);
        }

        private static string GetDevicePlugin(string UUID)
        {
            var data = MiningDataStats.GetDevicesMiningStats();
            var devData = data.FirstOrDefault(dev => dev.DeviceUUID == UUID);
            if (devData == null)
            {
                var fallback = AvailableDevices.Devices
                    .Where(d => d.Uuid == UUID)?
                    .Where(d => d.State == DeviceState.Mining
                        || d.State == DeviceState.Benchmarking
                        || d.State == DeviceState.Testing)?
                    .SelectMany(d => d.AlgorithmSettings)?
                    .Where(a => a.IsCurrentlyMining)?
                    .FirstOrDefault();
                return fallback == null ? string.Empty : fallback.PluginName;
            }
            return devData.MinerName;
        }

        private static (List<(string name, string? unit)> properties, JArray values) GetDeviceOptionalDynamic(ComputeDevice d, bool isLogin = false)
        {
            if(!IgnoredValues.ContainsKey(d.B64Uuid)) IgnoredValues.Add(d.B64Uuid, new List<DeviceDynamicProperties> { });
            string getValue<T>(T o) => (typeof(T).Name, o) switch
            {
                (nameof(ILoad), ILoad g) => $"{(int)g.Load}",
                //(nameof(IMemControllerLoad), IMemControllerLoad g) => $"{g.MemoryControllerLoad}",
                (nameof(ITemp), ITemp g) => $"{g.Temp}",
                (nameof(IGetFanSpeedPercentage), IGetFanSpeedPercentage g) => $"{g.GetFanSpeedPercentage().percentage}",
                (nameof(IFanSpeedRPM), IFanSpeedRPM g) => $"{g.FanSpeedRPM}",
                (nameof(IPowerUsage), IPowerUsage g) => $"{g.PowerUsage}",
                (nameof(IVramTemp), IVramTemp g) => $"{g.VramTemp}",
                (nameof(IHotspotTemp), IHotspotTemp g) => $"{g.HotspotTemp}",
                (nameof(ICoreClock), ICoreClock g) => $"{g.CoreClock}",
                //(nameof(ICoreClockDelta), ICoreClockDelta g) => $"{g.CoreClockDelta}",
                (nameof(IMemoryClock), IMemoryClock g) => $"{g.MemoryClock}",
                //(nameof(IMemoryClockDelta), IMemoryClockDelta g) => $"{g.MemoryClockDelta}",
                (nameof(ITDP), ITDP g) => $"{g.TDPPercentage * 100}",
                (nameof(ITDPWatts), ITDPWatts g) => $"{g.TDPWatts}",
                (nameof(ICoreVoltage), ICoreVoltage g) => $"{g.CoreVoltage}",
                (_, _) => null,
            };

            string getValueForName(string name) => name switch
            {
                "Miner" => $"{GetDevicePlugin(d.Uuid)}",
                "OC profile" => $"{d.OCProfile}",
                "OC profile ID" => $"{d.OCProfileID}",
                "Fan profile" => $"{d.FanProfile}",
                "Fan profile ID" => $"{d.FanProfileID}",
                "ELP profile" => $"{d.ELPProfile}",
                "ELP profile ID" => $"{d.ELPProfileID}",
                _ => null,
            };

            (DeviceDynamicProperties type, string name, string unit, string value)? pairOrNull<T>(DeviceDynamicProperties type, string name, string unit)
            {
                if (d.DeviceMonitor is T sensor) return (type, name, unit, getValue<T>(sensor));
                if (typeof(T) == typeof(string)) return (type, name, unit, getValueForName(name));
                return null;
            }

            // here sort manually by type 
            var dynamicPropertiesWithValues = new List<(DeviceDynamicProperties type, string name, string unit, string value)?>
            {
                pairOrNull<ITemp>(DeviceDynamicProperties.Temperature ,"Temperature","°C"),
                pairOrNull<IVramTemp>(DeviceDynamicProperties.VramTemp,"Memory Temperature","°C"),
                pairOrNull<ILoad>(DeviceDynamicProperties.Load,"Load","%"),
                //pairOrNull<IMemControllerLoad>(DeviceDynamicProperties.MemoryControllerLoad, "MemCtrl Load","%"),
                pairOrNull<IGetFanSpeedPercentage>(DeviceDynamicProperties.FanSpeedPercentage, "Fan speed","%"),
                pairOrNull<IFanSpeedRPM>(DeviceDynamicProperties.FanSpeedRPM, "Fan speed","RPM"),
                pairOrNull<IPowerUsage>(DeviceDynamicProperties.PowerUsage, "Power usage","W"),
                pairOrNull<ICoreClock>(DeviceDynamicProperties.CoreClock, "Core clock", "MHz"),
                //pairOrNull<ICoreClockDelta>(DeviceDynamicProperties.CoreClockDelta, "Core clock delta", "MHz"),
                pairOrNull<IMemoryClock>(DeviceDynamicProperties.MemClock, "Memory clock", "MHz"),
                //pairOrNull<IMemoryClockDelta>(DeviceDynamicProperties.MemClockDelta, "Memory clock", "MHz"),
                pairOrNull<ICoreVoltage>(DeviceDynamicProperties.CoreVoltage, "Core voltage", "mV"),
                pairOrNull<ITDP>(DeviceDynamicProperties.TDP, "Power Limit", "%"),
                pairOrNull<ITDPWatts>(DeviceDynamicProperties.TDPWatts, "Power Limit", "W"),
                pairOrNull<string>(DeviceDynamicProperties.NONE, "Miner", null),
                pairOrNull<string>(DeviceDynamicProperties.NONE, "OC profile", null),
                pairOrNull<string>(DeviceDynamicProperties.NONE, "OC profile ID", null),
                pairOrNull<string>(DeviceDynamicProperties.NONE, "Fan profile", null),
                pairOrNull<string>(DeviceDynamicProperties.NONE, "Fan profile ID", null),
                pairOrNull<string>(DeviceDynamicProperties.NONE, "ELP profile", null),
                pairOrNull<string>(DeviceDynamicProperties.NONE, "ELP profile ID", null),
            };
            var deviceOptionalDynamic = dynamicPropertiesWithValues
                .Where(p => p.HasValue)
                .Where(p => p.Value.value != null)
                .Select(p => p.Value)
                .ToList();

            if (isLogin)
            {
                foreach (var od in deviceOptionalDynamic)
                {
                    if ((od.type == DeviceDynamicProperties.Temperature ||
                        od.type == DeviceDynamicProperties.HotspotTemp ||
                        od.type == DeviceDynamicProperties.VramTemp ||
                        od.type == DeviceDynamicProperties.PowerUsage ||
                        od.type == DeviceDynamicProperties.TDP ||
                        od.type == DeviceDynamicProperties.TDPWatts ||
                        od.type == DeviceDynamicProperties.CoreVoltage) &&
                        Int32.TryParse(od.value, out var lessOrEqual) && lessOrEqual <= 0)
                    {
                        if (IgnoredValues.TryGetValue(d.B64Uuid, out var list) &&
                            !list.Contains(od.type))
                        {
                            list.Add(od.type);
                        }
                    }
                    if ((od.type == DeviceDynamicProperties.Load ||
                        od.type == DeviceDynamicProperties.FanSpeedRPM ||
                        od.type == DeviceDynamicProperties.FanSpeedPercentage ||
                        od.type == DeviceDynamicProperties.CoreClock ||
                        od.type == DeviceDynamicProperties.MemClock) &&
                        Int32.TryParse(od.value, out var less) && less < 0)
                    {
                        if (IgnoredValues.TryGetValue(d.B64Uuid, out var list) &&
                            !list.Contains(od.type))
                        {
                            list.Add(od.type);
                        }
                    }
                }
            }

            bool shouldRemoveDynamicVal(string b64uuid, (DeviceDynamicProperties type, string name, string unit, string value) dynamicVal)
            {
                //if (dynamicVal.unit == String.Empty) return false;
                if (dynamicVal.type == DeviceDynamicProperties.NONE) return false;
                if(IgnoredValues.TryGetValue(d.B64Uuid, out var list) && list.Contains(dynamicVal.type))
                {
                    return true;
                }
                return false;
            };
            deviceOptionalDynamic.RemoveAll(dynamVal => shouldRemoveDynamicVal(d.B64Uuid, dynamVal));
            //deviceOptionalDynamic.ForEach(dynamVal => d.SupportedDynamicProperties.Add(dynamVal.type));
            //if (isLogin) deviceOptionalDynamic.ForEach(dynamVal => d.SupportedDynamicProperties.Add(dynamVal.type));
            //foreach (DeviceDynamicProperties i in Enum.GetValues(typeof(DeviceDynamicProperties)))
            //{
            //    if (!d.SupportedDynamicProperties.Contains(i)) deviceOptionalDynamic.RemoveAll(prop => prop.type == i);
            //}
            List<(string name, string? unit)> optionalDynamicProperties = deviceOptionalDynamic.Select(p => (p.name, p.unit)).ToList();
            var values_odv = new JArray(deviceOptionalDynamic.Select(p => p.value));
            return (optionalDynamicProperties, values_odv);
        }

        // we cache device properties so we persevere  property IDs
        private static readonly Dictionary<ComputeDevice, List<OptionalMutableProperty>> _cachedDevicesOptionalMutable = new Dictionary<ComputeDevice, List<OptionalMutableProperty>>();
        private static (List<OptionalMutableProperty> properties, JArray values) GetDeviceOptionalMutable(ComputeDevice d, bool isLogin)
        {
            OptionalMutableProperty valueOrNull<T>(OptionalMutableProperty v) => d.DeviceMonitor is T ? v : null;
            List<OptionalMutableProperty> getOptionalMutableProperties(ComputeDevice d)
            {
                var optionalProperties = new List<OptionalMutableProperty>();
                // TODO sort by type
                optionalProperties.Add(new OptionalMutablePropertyString
                {
                    PropertyID = OptionalMutableProperty.NextPropertyId(),
                    DisplayGroup = 0,
                    DisplayName = "Miners settings",
                    DefaultValue = "",
                    Range = (8092, ""),
                    ExecuteTask = async (object p) =>
                    {
                        if (p is not string prop) return -1;
                        var newState = JsonConvert.DeserializeObject<MinerAlgoState>(prop);
                        return d.ApplyNewAlgoStates(newState);
                    },
                    GetValue = () =>
                    {
                        string ret = null;
                        ret = string.Empty;
                        ret += GetMinersForDeviceDynamic(d);
                        return ret;
                    },
                    ComputeDev = d
                });
                optionalProperties.Add(new OptionalMutablePropertyString
                {
                    PropertyID = OptionalMutableProperty.NextPropertyId(),
                    DisplayGroup = 0,
                    DisplayName = "Benchmark settings",
                    DefaultValue = "",
                    Range = (8092, ""),
                    ExecuteTask = async (object p) =>
                    {
                        if (p is not string prop) return -1;
                        var newSpeed = JsonConvert.DeserializeObject<MinerAlgoSpeed>(prop);
                        return d.ApplyNewAlgoSpeeds(newSpeed);
                    },
                    GetValue = () =>
                    {
                        var ret = string.Empty;
                        ret += GetMinersSpeedsForDeviceDynamic(d);
                        return ret;
                    },
                    ComputeDev = d
                });
                if (isLogin) optionalProperties.ForEach(i => ActionMutableMap.MutableList.Add(i));
                return optionalProperties
                    .Where(p => p != null)
                    .ToList();
            }

            List<OptionalMutableProperty> getOptionalMutablePropertiesCached(ComputeDevice d)
            {
                if (_cachedDevicesOptionalMutable.TryGetValue(d, out var cachedProps)) return cachedProps;
                return getOptionalMutableProperties(d);
            }

            var props = getOptionalMutablePropertiesCached(d);
            var selectedValues = props
                .Where(p => p.GetValue() != null)?
                .Select(p => p.GetValue());
            JArray values_omv = null;
            if (selectedValues.Any())
            {
                values_omv = new JArray(selectedValues);
            }
            return (props, values_omv);
        }



        public static List<List<string>> DeviceOptionalDynamicToList(List<(string name, string? unit)> properties)
        {
            List<List<string>> result = new List<List<string>>();
            foreach (var property in properties)
            {
                if (property.unit == null)
                {
                    result.Add(new List<string> { property.name });
                    continue;
                }
                result.Add(new List<string> { property.name, property.unit });
            }
            return result;
        }


        public static LoginMessage CreateLoginMessage(string btc, string worker, string rigID, IOrderedEnumerable<ComputeDevice> devices)
        {
            var sorted = SortedDevices(devices);
            //if (_loginMessage != null) return _loginMessage;
            Device mapComputeDevice(ComputeDevice d)
            {
                return new Device
                {
                    StaticProperties = new Dictionary<string, object>
                    {
                        { "device_id", d.B64Uuid },
                        { "class", $"{NHMDeviceTypeToNHMWSDeviceType(d.DeviceType)}" },
                        { "name", d.Name },
                        { "optional", GetStaticPropertiesOptionalValues(d) },
                    },
                    Actions = CreateDefaultDeviceActions(d.B64Uuid),
                    OptionalDynamicProperties = DeviceOptionalDynamicToList(GetDeviceOptionalDynamic(d, true).properties),
                    OptionalMutableProperties = GetDeviceOptionalMutable(d, true).properties,
                };
            }
            var DevicesProperties = devices.Select(mapComputeDevice).ToList(); //needs to execute first
            return new LoginMessage
            {
                Btc = btc,
                Worker = worker,
                RigID = rigID,
                Version = new List<string> { $"NHM/{NHMApplication.ProductVersion}", Environment.OSVersion.ToString() },
                OptionalMutableProperties = GetRigOptionalMutableValues(true).properties,
                OptionalDynamicProperties = GetRigOptionalDynamicValues().properties,
                Actions = CreateDefaultRigActions(),
                Devices = DevicesProperties,
                MinerState = GetMinerStateValues(worker, devices),
            };
        }
        private static (List<OptionalMutableProperty> properties, JArray values) GetRigOptionalMutableValues(bool isLogin)
        {
            List<OptionalMutableProperty> getOptionalMutableProperties()
            {
                var optionalProperties = new List<OptionalMutableProperty>()
                {
                    new OptionalMutablePropertyString
                    {
                        PropertyID = OptionalMutableProperty.NextPropertyId(),
                        DisplayGroup = 0,
                        DisplayName = "User name",
                        DefaultValue = CredentialsSettings.Instance.BitcoinAddress,
                        Range = (64, String.Empty),
                        GetValue = () =>
                        {
                            return CredentialsSettings.Instance.BitcoinAddress;
                        },
                        //ExecuteTask = (object p) =>
                        //{
                        //    var userSetResult = await ApplicationStateManager.SetBTCIfValidOrDifferent(btc, true);
                        //    return userSetResult switch
                        //    {
                        //        NhmwsSetResult.CHANGED => true, // we return executed
                        //        NhmwsSetResult.INVALID => throw new RpcException("Mining address invalid", ErrorCode.InvalidUsername),
                        //        NhmwsSetResult.NOTHING_TO_CHANGE => throw new RpcException($"Nothing to change btc \"{btc}\" already set", ErrorCode.RedundantRpc),
                        //        _ => throw new RpcException($"", ErrorCode.InternalNhmError),
                        //    };
                        //}
                    },
                    new OptionalMutablePropertyString
                    {
                        PropertyID = OptionalMutableProperty.NextPropertyId(),
                        DisplayGroup = 0,
                        DisplayName = "Worker name",
                        DefaultValue = CredentialsSettings.Instance.WorkerName,
                        Range = (64, String.Empty),
                        GetValue = () =>
                        {
                            return CredentialsSettings.Instance.WorkerName;
                        },
                        //ExecuteTask = (object p) =>
                        //{
                        //    var workerSetResult = ApplicationStateManager.SetWorkerIfValidOrDifferent(worker, true);
                        //    return workerSetResult switch
                        //    {
                        //        NhmwsSetResult.CHANGED => Task.FromResult(true), // we return executed
                        //        NhmwsSetResult.INVALID => throw new RpcException("Worker name invalid", ErrorCode.InvalidWorker),
                        //        NhmwsSetResult.NOTHING_TO_CHANGE => throw new RpcException($"Nothing to change worker name \"{worker}\" already set", ErrorCode.RedundantRpc),
                        //        _ => throw new RpcException($"", ErrorCode.InternalNhmError),
                        //    };
                        //}
                    },
                    new OptionalMutablePropertyString
                    {
                        PropertyID = OptionalMutableProperty.NextPropertyId(),
                        DisplayGroup = 0,
                        DisplayName = "Miners settings",
                        DefaultValue = "",
                        Range = (65536, String.Empty),
                        GetValue = () =>
                        {
                            string ret = string.Empty;
                            var minersSettingsGlobal = new MinerAlgoStateRig();
                            var mutables = ActionMutableMap.MutableList.Where(m => m.ComputeDev != null && m.DisplayName == "Miners settings");
                            if(mutables == null || mutables.Count() <= 0) return ret;
                            foreach (var mutable in mutables)
                            {
                                if (mutable.GetValue() is not string val) continue;
                                minersSettingsGlobal.Miners.Add(JsonConvert.DeserializeObject<MinerAlgoState>(val));
                            }
                            ret += JsonConvert.SerializeObject(minersSettingsGlobal);
                            return ret;
                        },
                        ExecuteTask = async (object p) =>
                        {
                            if(p is not string prop) return -1;
                            var newStates = JsonConvert.DeserializeObject<MinerAlgoStateRig>(prop);
                            //for each device thats inside apply new algo state
                            var devices = AvailableDevices.Devices.Where(d => newStates.Miners.Any(m => m.DeviceID.Contains(d.B64Uuid)));
                            if(devices == null) return -2;
                            var successCount = 0;
                            foreach(var ns in newStates.Miners)
                            {
                                var targetDev = AvailableDevices.Devices.FirstOrDefault(d => d.B64Uuid == ns.DeviceID);
                                if(targetDev == null) continue;
                                var tempRes = targetDev.ApplyNewAlgoStates(ns);
                                if(tempRes != 0) continue;
                                successCount++;
                            }
                            return successCount == newStates.Miners.Count ? 0 : -3;
                        }
                    },
                    new OptionalMutablePropertyString
                    {
                        PropertyID = OptionalMutableProperty.NextPropertyId(),
                        DisplayGroup = 0,
                        DisplayName = "Scheduler settings",
                        DefaultValue = "",
                        Range = (4096, string.Empty),
                        GetValue = () =>
                        {
                            string ret = SchedulesManager.Instance.ScheduleToJSON();
                            return ret;
                        },
                        ExecuteTask = async (object p) =>
                        {
                            if(p is not string prop) return -1;
                            var (schedulerEnabled, returnedSchedules) = SchedulesManager.Instance.ScheduleFromJSON(prop);
                            SchedulesManager.Instance.ClearScheduleList();
                            MiningSettings.Instance.UseScheduler = schedulerEnabled;
                            if(returnedSchedules != null)
                            {
                                foreach(var returnedSchedule in returnedSchedules)
                                {
                                    returnedSchedule.From = DateTime.Parse(returnedSchedule.From).ToLocalTime().ToString("HH:mm");
                                    returnedSchedule.To = DateTime.Parse(returnedSchedule.To).ToLocalTime().ToString("HH:mm");
                                    SchedulesManager.Instance.AddScheduleToList(returnedSchedule);
                                }
                            }
                            _ = Task.Run(async () => await NHWebSocketV4.UpdateMinerStatus());
                            return 0;
                        }
                    },
                    new OptionalMutablePropertyBool
                    {
                        PropertyID = OptionalMutableProperty.NextPropertyId(),
                        DisplayGroup = 0,
                        DisplayName = "Auto update",
                        DefaultValue = false,
                        GetValue = () =>
                        {
                            return UpdateSettings.Instance.AutoUpdateMinerPlugins && UpdateSettings.Instance.AutoUpdateNiceHashMiner;
                        },
                        ExecuteTask = async (object p) =>
                        {
                            if(p is not bool prop) return -1;
                            UpdateSettings.Instance.AutoUpdateMinerPlugins = prop;
                            UpdateSettings.Instance.AutoUpdateNiceHashMiner = prop;
                            _ = Task.Run(async () => await NHWebSocketV4.UpdateMinerStatus());
                            return 0;
                        }
                    },
                    new OptionalMutablePropertyString
                    {
                        PropertyID = OptionalMutableProperty.NextPropertyId(),
                        DisplayGroup = 0,
                        DisplayName = "Benchmark settings",
                        DefaultValue = "",
                        Range = (65536, ""),
                        GetValue = () =>
                        {
                            var ret = string.Empty;
                            var minerSpeedsGlobal = new MinerAlgoSpeedRig();
                            var mutables = ActionMutableMap.MutableList.Where(m => m.ComputeDev != null && m.DisplayName == "Benchmark settings");
                            if(mutables == null || mutables.Count() <= 0) return ret;
                            foreach (var mutable in mutables)
                            {
                                if (mutable.GetValue() is not string val) continue;
                                minerSpeedsGlobal.Miners.Add(JsonConvert.DeserializeObject<MinerAlgoSpeed>(val));
                            }
                            ret += JsonConvert.SerializeObject(minerSpeedsGlobal);
                            return ret;
                        },
                        ExecuteTask = async (object p) =>
                        {
                            if(p is not string prop) return -1;
                            var newSpeeds = JsonConvert.DeserializeObject<MinerAlgoSpeedRig>(prop);
                            //for each device thats inside apply new algo state
                            var devices = AvailableDevices.Devices.Where(d => newSpeeds.Miners.Any(m => m.DeviceID.Contains(d.B64Uuid)));
                            if(devices == null) return -2;
                            var successCount = 0;
                            foreach(var ns in newSpeeds.Miners)
                            {
                                var targetDev = AvailableDevices.Devices.FirstOrDefault(d => d.B64Uuid == ns.DeviceID);
                                if(targetDev == null) continue;
                                var tempRes = targetDev.ApplyNewAlgoSpeeds(ns);
                                if(tempRes != 0) continue;
                                successCount++;
                            }
                            return successCount == newSpeeds.Miners.Count ? 0 : -3;
                        }
                    }
                };
                if (isLogin)
                    optionalProperties.ForEach(i => ActionMutableMap.MutableList.Add(i));
                return optionalProperties
                    .Where(p => p != null)
                    .ToList();

            };

            List<OptionalMutableProperty> getOptionalMutablePropertiesCached()
            {
                //if (_cachedDevicesOptionalMutable.TryGetValue(out var cachedProps)) return cachedProps;
                return getOptionalMutableProperties();
            }

            var props = getOptionalMutablePropertiesCached();
            var selectedValues = props
                .Where(p => p.GetValue() != null)?
                .Select(p => p.GetValue());
            JArray values_omv = null;
            if (selectedValues.Any())
            {
                values_omv = new JArray(selectedValues);
            }
            return (props, values_omv);
        }
        private static (List<List<string>> properties, JArray values) GetRigOptionalDynamicValues()
        {
            var dynamic = new List<(List<string> prop, string val)>
            {
                (new List<string>
                {
                    "Uptime",
                    "s"
                }, Helpers.GetElapsedSecondsSinceStart().ToString()),
                (new List<string>
                {
                    "IP address"
                }, Helpers.GetLocalIP().ToString()),
                (new List<string>
                {
                    "Profiles bundle id"
                }, BundleManager.GetBundleInfo().BundleID),
                (new List<string>
                {
                    "Profiles bundle name"
                }, BundleManager.GetBundleInfo().BundleName)
            };
            var props = dynamic.Select(d => d.prop).ToList();
            var vals = dynamic.Select(d => d.val);
            return (props, new JArray(vals));
        }

        private static JObject GetMinerStateValues(string workerName, IOrderedEnumerable<ComputeDevice> devices)
        {
            var json = JObject.FromObject(GetMinerState(workerName, devices));
            var delProp = json.Property("method");
            delProp.Remove();
            return json;
        }

        internal static MinerState GetMinerState(string workerName, IOrderedEnumerable<ComputeDevice> devices)
        {
            var rig = ApplicationStateManager.CalcRigStatus();

            int rigStateToInt(RigStatus s) => s switch
            {
                RigStatus.Stopped => 1, // READY/IDLE/STOPPED
                RigStatus.Mining => 2, // MINING/WORKING
                RigStatus.Benchmarking => 3, // BENCHMARKING
                RigStatus.Error => 5, // ERROR
                RigStatus.Pending => 0, // NOT DEFINED
                RigStatus.Disabled => 4, // DISABLED
                _ => 0, // UNKNOWN
            };


            MinerState.DeviceState toDeviceState(ComputeDevice d)
            {                
                int deviceStateToInt(DeviceState s) => s switch
                {
                    DeviceState.Stopped => 1, // READY/IDLE/STOPPED
                    DeviceState.Mining => 2, // MINING/WORKING
                    DeviceState.Benchmarking => 3, // BENCHMARKING
                    DeviceState.Error => 5, // ERROR
                    DeviceState.Pending => 0, // NOT DEFINED
                    DeviceState.Disabled => 4, // DISABLED
#if NHMWS4
                    //DeviceState.Gaming => 6, //GAMING
                    DeviceState.Testing => (MiscSettings.Instance.EnableGPUManagement ? 7 : 2), //TESTING
#endif
                    _ => 0, // UNKNOWN
                };

                JArray mdv(ComputeDevice d)
                {
                    var state = deviceStateToInt(d.State);
                    var speeds = MiningDataStats.GetSpeedForDevice(d.Uuid);
                    return new JArray(state, new JArray(speeds.Select(kvp => new JArray((int)kvp.type, kvp.speed))));
                }
                JArray mmv(ComputeDevice d)
                {
                    return new JArray(deviceStateToInt(d.State));
                }
                //Logger.Warn(_TAG, $"\t[{d.BaseDevice.Name}](deviceState):{d.State} -- converted (int):{deviceStateToInt(d.State)}");

                return new MinerState.DeviceState
                {
                    MandatoryDynamicValues = mdv(d),
                    OptionalDynamicValues = GetDeviceOptionalDynamic(d).values, // odv
                    MandatoryMutableValues = mmv(d),
                    OptionalMutableValues = GetDeviceOptionalMutable(d, false).values, // omv
                };
            }
            //Logger.Warn(_TAG, $"Miner state (rigstatus):{rig} -- converted (int):{rigStateToInt(rig)}");
            return new MinerState
            {
                MutableDynamicValues = new JArray(rigStateToInt(rig)),
                OptionalDynamicValues = GetRigOptionalDynamicValues().values,
                MandatoryMutableValues = new JArray(rigStateToInt(rig), workerName),
                OptionalMutableValues = GetRigOptionalMutableValues(false).values,
                Devices = devices.Select(toDeviceState).ToList(),
            };
        }


        private static List<NhmwsAction> CreateDefaultDeviceActions(string uuid)
        {
            return new List<NhmwsAction>
            {
                NhmwsAction.ActionDeviceEnable(uuid),
                NhmwsAction.ActionDeviceDisable(uuid),
                NhmwsAction.ActionDeviceRebenchmark(uuid),
                NhmwsAction.ActionOcProfileTest(uuid),
                NhmwsAction.ActionOcProfileTestStop(uuid),
                NhmwsAction.ActionFanProfileTest(uuid),
                NhmwsAction.ActionFanProfileTestStop(uuid),
                NhmwsAction.ActionElpProfileTest(uuid),
                NhmwsAction.ActionElpProfileTestStop(uuid),
            };
        }
        private static List<NhmwsAction> CreateDefaultRigActions()
        {
            return new List<NhmwsAction>
            {
                NhmwsAction.ActionStartMining(),
                NhmwsAction.ActionStopMining(),
                NhmwsAction.ActionRebenchmark(),
                NhmwsAction.ActionProfilesBundleSet(),
                NhmwsAction.ActionProfilesBundleReset(),
                NhmwsAction.ActionRigShutdown(),
                NhmwsAction.ActionRigRestart(),
                NhmwsAction.ActionSystemDump(),
            };
        }
        private static List<JArray> GetStaticPropertiesOptionalValues(ComputeDevice d)
        {
            return d.BaseDevice switch
            {
                IGpuDevice gpu => new List<JArray>
                    {
                        new JArray("bus_id", $"{gpu.PCIeBusID}"),
                        new JArray("vram", $"{gpu.GpuRam}"),
                        new JArray("miners", GetMinersForDeviceStatic(d)),
                        new JArray("limits", GetLimitsForDevice(d)),
                    },
                _ => new List<JArray>
                    {
                        new JArray("miners", GetMinersForDeviceStatic(d)),
                        new JArray("limits", GetLimitsForDevice(d)),
                    },
            };
        }
        private static string GetMinersForDeviceDynamic(ComputeDevice d)
        {
            var minersObject = new MinerAlgoState();
            var containers = d.AlgorithmSettings;
            if (containers == null) return String.Empty;
            var grouped = containers.GroupBy(c => c.PluginName).ToList();
            if (grouped == null) return String.Empty;
            foreach (var group in grouped)
            {
                var containerEnabled = group.Any(c => c.Enabled);
                var miner = new MinerDynamic() { Id = group.Key, Enabled = containerEnabled };
                var algos = new List<Algo>();
                foreach (var algo in group)
                {
                    var tempAlgo = new Algo() { Id = algo.AlgorithmName, Enabled = algo.Enabled };
                    algos.Add(tempAlgo);
                }
                miner.Algos = algos;
                minersObject.Miners.Add(miner);
            }
            minersObject.DeviceID = d.B64Uuid;
            minersObject.DeviceName = d.Name;
            var json = JsonConvert.SerializeObject(minersObject);
            return json;
        }

        private static string GetMinersSpeedsForDeviceDynamic(ComputeDevice d)
        {
            var minersObject = new MinerAlgoSpeed();
            var containers = d.AlgorithmSettings;
            if (containers == null) return string.Empty;
            var grouped = containers.GroupBy(c => c.PluginName).ToList();
            if (grouped == null) return string.Empty;
            foreach (var group in grouped)
            {
                var combinations = new List<Combination>();
                foreach (var algo in group)
                {
                    var algorithms = new List<AlgoSpeed>()
                    {
                        new AlgoSpeed()
                        {
                            Id = Convert.ToString((int)algo.IDs[0]),
                            Speed = algo.BenchmarkSpeed.ToString()
                        }

                    };
                    var combination = new Combination()
                    {
                        Id = algo.AlgorithmName,
                        Algos = algorithms
                    };
                    combinations.Add(combination);
                }
                var miner = new MinerSpeedDynamic() { Id = group.Key, Combinations = combinations };
                minersObject.Miners.Add(miner);
            }
            minersObject.DeviceID = d.B64Uuid;
            minersObject.DeviceName = d.Name;
            var json = JsonConvert.SerializeObject(minersObject);
            return json;
        }

        private static string GetMinersForDeviceStatic(ComputeDevice d)
        {
            MinersStatic miners = new MinersStatic();
            var uniquePlugins = d.AlgorithmSettings?.Select(item => item.PluginName)?.Distinct()?.Where(item => !string.IsNullOrEmpty(item));
            if (uniquePlugins == null) return String.Empty;
            foreach (var plugin in uniquePlugins)
            {
                var uniqueAlgos = d.AlgorithmSettings?.Where(item => item.PluginName == plugin)?.Select(item => item.AlgorithmName)?.Distinct();
                if (uniqueAlgos == null) uniqueAlgos = new List<string>();
                miners.Miners.Add(new MinerStatic() { Id = plugin, AlgoList = uniqueAlgos.ToList() });
            }
            var json = JsonConvert.SerializeObject(miners);
            return json;
        }

        private static int CheckOrCorrectDefaultValue(int min, int max, int def)
        {
            if (def > min && def < max) return def;
            return (min + max) / 2;
        }
        private static bool AreLimitsWrong(int min, int max)
        {
            return max - min <= 20;
        }

        private static string GetLimitsForDevice(ComputeDevice d)
        {
            //todo granulate
            ComplexLimit limit = new ComplexLimit();
            if (d.DeviceMonitor is ITDP && d.DeviceMonitor is ITDPLimits tdpLim)
            {
                var lims = tdpLim.GetTDPLimits();
                if (lims.ok)
                {
                    string unit = d.DeviceType == DeviceType.AMD ? "%" : "W";
                    int defVal = CheckOrCorrectDefaultValue(lims.min, lims.max, lims.def);
                    limit.limits.Add(new Limit { Name = "Power Limit", Unit = unit, Def = defVal, Range = ((int)lims.min, (int)lims.max) });
                }
            }
            if (d.DeviceMonitor is ICoreClockSet)
            {
                if (d.DeviceType == DeviceType.NVIDIA && d.DeviceMonitor is ICoreClockRangeDelta ccLimDelta)
                {
                    var lims = ccLimDelta.CoreClockRangeDelta;
                    if (lims.ok)
                    {
                        int defVal = CheckOrCorrectDefaultValue(lims.min, lims.max, lims.def);
                        limit.limits.Add(new Limit { Name = "Core clock delta", Unit = "MHz", Def = defVal, Range = (lims.min, lims.max) });
                    }
                }
                if (d.DeviceMonitor is ICoreClockRange ccLim && !d.IsNvidiaAndSub2KSeries())
                {
                    var lims = ccLim.CoreClockRange;
                    if (lims.ok)
                    {
                        var range = AreLimitsWrong(lims.min, lims.max) ? (300, 3000) : (lims.min, lims.max); //default, if range fails
                        int defVal = CheckOrCorrectDefaultValue(lims.min, lims.max, lims.def);
                        if (AreLimitsWrong(lims.min, lims.max)) AvailableNotifications.CreateCoreClockRangeGetFailed(d.BaseDevice.ID, d.FullName);
                        limit.limits.Add(new Limit { Name = "Core clock", Unit = "MHz", Def = defVal, Range = range });
                    }
                }
            }
            if (d.DeviceMonitor is IMemoryClockSet)
            {
                if (d.DeviceType == DeviceType.NVIDIA && d.DeviceMonitor is IMemoryClockRangeDelta mcLimDelta)
                {
                    var lims = mcLimDelta.MemoryClockRangeDelta;
                    if (lims.ok)
                    {
                        int defVal = CheckOrCorrectDefaultValue(lims.min, lims.max, lims.def);
                        limit.limits.Add(new Limit { Name = "Memory clock delta", Unit = "MHz", Def = defVal, Range = (lims.min, lims.max) });
                    }
                }
                if (d.DeviceMonitor is IMemoryClockRange mcLim && !d.IsNvidiaAndSub2KSeries())
                {
                    var lims = mcLim.MemoryClockRange;
                    if (lims.ok)
                    {
                        var range = AreLimitsWrong(lims.min, lims.max) ? (300, 10000) : (lims.min, lims.max); //default, if range fails
                        int defVal = CheckOrCorrectDefaultValue(lims.min, lims.max, lims.def);
                        if (AreLimitsWrong(lims.min, lims.max)) AvailableNotifications.CreateMemClockRangeGetFailed(d.BaseDevice.ID, d.FullName);
                        limit.limits.Add(new Limit { Name = "Memory clock", Unit = "MHz", Def = defVal, Range = range });
                    }
                }
            }
            if(d.DeviceMonitor is ICoreVoltageSet && d.DeviceMonitor is ICoreVoltageRange cvRange)
            {
                var lims = cvRange.CoreVoltageRange;
                if (lims.ok && d.DeviceMonitor is ICoreVoltage cvGet)
                {
                    var def = d.DeviceType == DeviceType.INTEL ? lims.def : cvGet.CoreVoltage;
                    def = CheckOrCorrectDefaultValue(lims.min, lims.max, def);
                    limit.limits.Add(new Limit { Name = "Core Voltage", Unit = "mV", Def = def, Range = (lims.min, lims.max) });
                }
            }
            var json = JsonConvert.SerializeObject(limit);
            return json;
        }
    }
}

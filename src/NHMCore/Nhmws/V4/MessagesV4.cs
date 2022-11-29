using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Security.Policy;
using System.Threading.Tasks;
using System.Windows.Media.Animation;
using Windows.Devices.Sensors;

namespace NHMCore.Nhmws.V4
{
    public enum Type : int
    {
        Int = 0,
        Bool = 1,
        Enum = 2,
        String = 3,
    }
    public enum SupportedAction : int
    {
        ActionUnsupported = -1,
        ActionStartMining,
        ActionStopMining,
        ActionProfilesBundleSet,
        ActionProfilesBundleReset,
        ActionDeviceEnable,
        ActionDeviceDisable,
        ActionOcProfileTest,
        ActionFanProfileTest,
        ActionElpProfileTest,
        ActionElpProfileTestStop,
        ActionOcProfileTestStop,
        ActionFanProfileTestStop,
    }
    internal class LoginMessage : ISendMessage
    {
        [JsonProperty("method")]
        public string Method => "login";
        [JsonProperty("protocol")]
        public int Protocol => 4;
        [JsonProperty("version")]
        public List<string> Version { get; set; } = new List<string> { };
        [JsonProperty("btc")]
        public string Btc { get; set; } = "";
        [JsonProperty("rig_id")]
        public string RigID { get; set; } = "";
        [JsonProperty("worker")]
        public string Worker { get; set; } = "";
        [JsonProperty("optional_dynamic_properties")]
        public List<List<string>> OptionalDynamicProperties { get; set; }
        [JsonProperty("optional_mutable_properties")]
        public List<OptionalMutableProperty> OptionalMutableProperties { get; set; }

        [JsonProperty("actions")]
        public List<NhmwsAction> Actions { get; set; }

        [JsonProperty("devices")]
        public List<Device> Devices { get; set; }

        [JsonProperty("miner.state")]
        public JObject MinerState { get; set; }
    }

    internal interface IStaticMandatoryProperty { }

    internal interface IStaticOptionalProperty { }

    internal interface IDynamicMandatoryProperty { }

    internal interface IDynamicOptionalProperty { }

    internal interface IMutableMandatoryProperty { }

    internal interface IMutableOptionalProperty { }

    internal interface IAction { }

    internal abstract class OptionalMutableProperty
    {
        private static int _nextPropertyId = 100;
        internal static int NextPropertyId() => _nextPropertyId++;

        [JsonProperty("prop_id")]
        public int PropertyID { get; set; }

        [JsonProperty("display_name")]
        public string DisplayName { get; set; }

        [JsonProperty("display_group")]
        public int? DisplayGroup { get; set; } = 0;

        [JsonProperty("display_unit", NullValueHandling = NullValueHandling.Ignore)]
        public string? DisplayUnit { get; set; }

        [JsonProperty("type")]
        abstract public Type PropertyType { get; }
        [JsonIgnore]
        public Func<object, Task<object>> ExecuteTask { get; set; }

        [JsonIgnore]
        public Func<object> GetValue { get; set; }
    }

    internal class OptionalMutablePropertyInt : OptionalMutableProperty
    {
        [JsonProperty("type")]
        public override Type PropertyType => Type.Int;

        [JsonProperty("default")]
        public int DefaultValue { get; set; }

        [JsonProperty("range")]
        [JsonConverter(typeof(Nhmws4JSONConverter))]
        public (int min, int max) Range { get; set; }
    }


    internal class OptionalMutablePropertyBool : OptionalMutableProperty
    {
        [JsonProperty("type")]
        public override Type PropertyType => Type.Bool;

        [JsonProperty("default")]
        public bool DefaultValue { get; set; }
    }

    internal class OptionalMutablePropertyEnum : OptionalMutableProperty
    {
        [JsonProperty("type")]
        public override Type PropertyType => Type.Enum;

        [JsonProperty("default")]
        public string DefaultValue { get; set; }

        [JsonProperty("range")]
        public List<string> Range { get; set; }
    }

    internal class OptionalMutablePropertyString : OptionalMutableProperty
    {
        [JsonProperty("type")]
        public override Type PropertyType => Type.String;

        [JsonProperty("default")]
        public string DefaultValue { get; set; }

        [JsonProperty("range")]
        [JsonConverter(typeof(Nhmws4JSONConverter))]
        public (int len, string charset) Range { get; set; }
    }

    public class NhmwsAction
    {
        private static int _actionId = 0;
        internal static int NextActionId() => _actionId++;

        [JsonProperty("action_id")]
        public int ActionID { get; set; }

        [JsonProperty("display_name")]
        public string DisplayName { get; set; }

        [JsonProperty("display_group")]
        public int DisplayGroup { get; set; }
        [JsonProperty("parameters")]
        public List<ParameterLogin> Parameters { get; set; } = new();
        [JsonIgnore]
        public SupportedAction ActionType { get; set; }
        [JsonIgnore]
        public Func<object, Task<object>> ExecuteTask { get; set; }
        public string DeviceUUID = String.Empty;
        public static NhmwsAction ActionDeviceEnable(string uuid)
        {
            var action = new NhmwsAction
            {
                ActionID = NhmwsAction.NextActionId(),
                DisplayName = "Device enable",
                DisplayGroup = 0,
                ActionType = SupportedAction.ActionDeviceEnable,
                DeviceUUID = uuid
            };
            ActionMap.ActionList.Add(action);
            return action;
        }
        public static NhmwsAction ActionDeviceDisable(string uuid)
        {
            var action = new NhmwsAction
            {
                ActionID = NhmwsAction.NextActionId(),
                DisplayName = "Device disable",
                DisplayGroup = 0,
                ActionType = SupportedAction.ActionDeviceDisable,
                DeviceUUID = uuid
            };
            ActionMap.ActionList.Add(action);
            return action;
        }
        public static NhmwsAction ActionOcProfileTest(string uuid)
        {
            var action = new NhmwsAction
            {
                ActionID = NhmwsAction.NextActionId(),
                DisplayName = "OC profile test",
                DisplayGroup = 1,
                Parameters = new List<ParameterLogin>()
                {
                    new ParameterStringLogin()
                    {
                        DisplayName = "OC profile",
                        DefaultValue = "",
                        Range = (1024, "")
                    }
                },
                ActionType = SupportedAction.ActionOcProfileTest,
                DeviceUUID = uuid
            };
            ActionMap.ActionList.Add(action);
            return action;
        }
        public static NhmwsAction ActionOcProfileTestStop(string uuid)
        {
            var action = new NhmwsAction
            {
                ActionID = NhmwsAction.NextActionId(),
                DisplayName = "OC profile stop test",
                DisplayGroup = 1,
                Parameters = new(),
                ActionType = SupportedAction.ActionOcProfileTestStop,
                DeviceUUID = uuid
            };
            ActionMap.ActionList.Add(action);
            return action;
        }
        public static NhmwsAction ActionFanProfileTest(string uuid)
        {
            var action = new NhmwsAction
            {
                ActionID = NhmwsAction.NextActionId(),
                DisplayName = "Fan profile test",
                DisplayGroup = 1,
                Parameters = new List<ParameterLogin>()
                {
                    new ParameterStringLogin()
                    {
                        DisplayName = "Fan profile",
                        DefaultValue = "",
                        Range = (1024, "")
                    }
                },
                ExecuteTask = async (object p) =>
                {
                    if (p is FanBundle fb && fb is not null) //todo fan bundle call here
                    {
                        var a = 0;
                    }
                    return 0;
                },
                ActionType = SupportedAction.ActionFanProfileTest,
                DeviceUUID = uuid
            };
            ActionMap.ActionList.Add(action);
            return action;
        }
        public static NhmwsAction ActionFanProfileTestStop(string uuid)
        {
            var action = new NhmwsAction
            {
                ActionID = NhmwsAction.NextActionId(),
                DisplayName = "Fan profile stop test",
                DisplayGroup = 1,
                Parameters = new(),
                ActionType = SupportedAction.ActionFanProfileTestStop,
                DeviceUUID = uuid
            };
            ActionMap.ActionList.Add(action);
            return action;
        }
        public static NhmwsAction ActionElpProfileTest(string uuid)
        {
            var action = new NhmwsAction
            {
                ActionID = NhmwsAction.NextActionId(),
                DisplayName = "ELP profile test",
                DisplayGroup = 1,
                Parameters = new List<ParameterLogin>()
                {
                    new ParameterStringLogin()
                    {
                        DisplayName = "ELP profile",
                        DefaultValue = "",
                        Range = (1024, "")
                    }
                },
                ActionType= SupportedAction.ActionElpProfileTest,
                DeviceUUID = uuid
            };
            ActionMap.ActionList.Add(action);
            return action;
        }
        public static NhmwsAction ActionElpProfileTestStop(string uuid)
        {
            var action = new NhmwsAction
            {
                ActionID = NhmwsAction.NextActionId(),
                DisplayName = "ELP profile stop test",
                DisplayGroup = 1,
                Parameters = new(),
                ActionType= SupportedAction.ActionElpProfileTestStop,
                DeviceUUID = uuid
            };
            ActionMap.ActionList.Add(action);
            return action;
        }
        public static NhmwsAction ActionStartMining()
        {
            var action = new NhmwsAction
            {
                ActionID = NhmwsAction.NextActionId(),
                DisplayName = "Mining start",
                DisplayGroup = 1,
                ActionType = SupportedAction.ActionStartMining,
            };
            ActionMap.ActionList.Add(action);
            return action;
        }
        public static NhmwsAction ActionStopMining()
        {
            var action = new NhmwsAction
            {
                ActionID = NhmwsAction.NextActionId(),
                DisplayName = "Mining stop",
                DisplayGroup = 1,
                ActionType = SupportedAction.ActionStopMining,
            };
            ActionMap.ActionList.Add(action);
            return action;
        }
        public static NhmwsAction ActionProfilesBundleSet()
        {
            var action = new NhmwsAction
            {
                ActionID = NhmwsAction.NextActionId(),
                DisplayName = "Profiles bundle set",
                DisplayGroup = 1,
                Parameters = new List<ParameterLogin>()
                {
                    new ParameterStringLogin()
                    {
                        DisplayName = "Bundle profiles",
                        DefaultValue = "",
                        Range = (4096, "")
                    }
                },
                ActionType = SupportedAction.ActionProfilesBundleSet,
            };
            ActionMap.ActionList.Add(action);
            return action;
        }
        public static NhmwsAction ActionProfilesBundleReset()
        {
            var action = new NhmwsAction
            {
                ActionID = NhmwsAction.NextActionId(),
                DisplayName = "Profiles bundle reset",
                DisplayGroup = 1,
                ActionType= SupportedAction.ActionProfilesBundleReset,
            };
            ActionMap.ActionList.Add(action);
            return action;
        }
    }
    internal class Device
    {
        [JsonProperty("static_properties")]
        public Dictionary<string, object> StaticProperties { get; set; }

        [JsonProperty("optional_dynamic_properties")]
        //[JsonConverter(typeof(Nhmws4JSONConverter))]
        public List<List<string>> OptionalDynamicProperties { get; set; }
        [JsonProperty("optional_mutable_properties")]
        public List<OptionalMutableProperty> OptionalMutableProperties { get; set; }

        [JsonProperty("actions")]
        public List<NhmwsAction> Actions { get; set; }

    }

    internal class MinerState : ISendMessage
    {
        internal class DeviceState
        {
            [JsonProperty("mdv")]
            public JArray MandatoryDynamicValues { get; set; }

            [JsonProperty("odv", NullValueHandling = NullValueHandling.Ignore)]
            public JArray OptionalDynamicValues { get; set; }

            [JsonProperty("mmv")]
            public JArray MandatoryMutableValues { get; set; }

            [JsonProperty("omv", NullValueHandling = NullValueHandling.Ignore)]
            public JArray OptionalMutableValues { get; set; }
        }

        [JsonProperty("method")]
        public string Method => "miner.state";

        [JsonProperty("mdv")]
        public JArray MutableDynamicValues { get; set; }

        [JsonProperty("odv", NullValueHandling = NullValueHandling.Ignore)]
        public JArray OptionalDynamicValues { get; set; }

        [JsonProperty("mmv")]
        public JArray MandatoryMutableValues { get; set; }

        [JsonProperty("omv", NullValueHandling = NullValueHandling.Ignore)]
        public JArray OptionalMutableValues { get; set; }

        [JsonProperty("devices")]
        public List<DeviceState> Devices { get; set; }
    }

    public abstract class ParameterLogin
    {
        [JsonProperty("display_name")]
        public string DisplayName { get; set; }
        [JsonProperty("display_group", NullValueHandling = NullValueHandling.Ignore)]
        public int? DisplayGroup { get; set; }
        [JsonProperty("display_unit", NullValueHandling = NullValueHandling.Ignore)]
        public string? DisplayUnit { get; set; }
        [JsonProperty("type")]
        abstract public Type PropertyType { get; }
    }
    internal class ParameterIntegerLogin : ParameterLogin
    {
        [JsonProperty("type")]
        public override Type PropertyType => Type.Int;

        [JsonProperty("default")]
        public int DefaultValue { get; set; }

        [JsonProperty("range")]
        [JsonConverter(typeof(Nhmws4JSONConverter))]
        public (int min, int max) Range { get; set; }
    }
    internal class ParameterBoolLogin : ParameterLogin
    {
        [JsonProperty("type")]
        public override Type PropertyType => Type.Bool;

        [JsonProperty("default")]
        public bool DefaultValue { get; set; }
    }
    internal class ParameterEnumLogin : ParameterLogin
    {
        [JsonProperty("type")]
        public override Type PropertyType => Type.Enum;

        [JsonProperty("default")]
        public string DefaultValue { get; set; }

        [JsonProperty("range")]
        public List<string> Range { get; set; }
    }
    internal class ParameterStringLogin : ParameterLogin
    {
        [JsonProperty("type")]
        public override Type PropertyType => Type.String;

        [JsonProperty("default")]
        public string DefaultValue { get; set; }

        [JsonProperty("range")]
        [JsonConverter(typeof(Nhmws4JSONConverter))]
        public (int len, string charset) Range { get; set; }
    }
    internal abstract class ActionParameter
    {
        [JsonProperty("device_name")]
        public string DeviceName { get; set; }
        [JsonProperty("name")]
        public string Name { get; set; }
        [JsonProperty("type")]
        public int Type { get; set; }
    }
    internal class Property
    {
        [JsonProperty("prop_id")]
        public int PropId { get; set; }
    }
    internal class PropertyInt : Property
    {
        [JsonProperty("value")]
        public int Value { get; set; }
    }
    internal class PropertyBool : Property
    {
        [JsonProperty("value")]
        public bool Value { get; set; }
    }
    internal class PropertyEnum : Property
    {
        [JsonProperty("value")]
        public Type Value { get; set; }
    }
    internal class PropertyString : Property
    {
        [JsonProperty("value")]
        public string Value { get; set; }
    }

    internal class MinerLogin
    {
        [JsonProperty("miners")]
        public List<MinerDynamic> Miners { get; set; } = new List<MinerDynamic>();
    }
    internal class MinerDynamic
    {
        [JsonProperty("id")]
        public string Id { get; set; }
        [JsonProperty("enabled")]
        public bool Enabled { get; set; }
        [JsonProperty("algorithms")]
        public List<Algo> Algos { get; set; } = new List<Algo>();
    }
    internal class MinerStatic
    {
        [JsonProperty("id")]
        public string Id { get; set; }
        [JsonProperty("algorithms")]
        public List<string> AlgoList { get; set; } = new List<string>();
    }
    internal class Algo
    {
        [JsonProperty("id")]
        public string Id { get; set; }
        [JsonProperty("enabled", NullValueHandling = NullValueHandling.Ignore)]
        public bool? Enabled { get; set; }
    }

    internal class Bundle
    {
        [JsonProperty("id")]
        public string Id { get; set; }
        [JsonProperty("name")]
        public string Name { get; set; }
        [JsonProperty("oc", NullValueHandling = NullValueHandling.Ignore)]
        public List<OcBundle>? OcBundles { get; set; }
        [JsonProperty("fan", NullValueHandling = NullValueHandling.Ignore)]
        public List<FanBundle>? FanBundles { get; set; }
        [JsonProperty("elp", NullValueHandling = NullValueHandling.Ignore)]
        public List<ElpBundle>? ElpBundles { get; set; }
    }
    internal class ElpBundle
    {
        [JsonProperty("name")]
        public string Name { get; set; }
        [JsonProperty("device_name")]
        public List<string>? DeviceName { get; set; }
        [JsonProperty("miner_id", NullValueHandling = NullValueHandling.Ignore)]
        public List<string>? MinerId { get; set; }
        [JsonProperty("algorithm_id", NullValueHandling = NullValueHandling.Ignore)]
        public string AlgoId { get; set; }
        [JsonProperty("elp")]
        public string Elp { get; set; }
    }
    public class OcBundle
    {
        [JsonProperty("name")]
        public string Name { get; set; }
        [JsonProperty("id")]
        public string Id { get; set; }
        [JsonProperty("device_name")]
        public string DeviceName { get; set; }
        [JsonProperty("miner_id", NullValueHandling = NullValueHandling.Ignore)]
        public List<string>? MinerId { get; set; }
        [JsonProperty("algorithm_id", NullValueHandling = NullValueHandling.Ignore)]
        public List<string>? AlgoId { get; set; }
        [JsonProperty("core_clock")]
        public int CoreClock { get; set; }
        [JsonProperty("memory_clock")]
        public int MemoryClock { get; set; }
        [JsonProperty("power_mode")]
        public int TDP { get; set; }
    }
    internal class FanBundle
    {
        [JsonProperty("name")]
        public string Name { get; set; }
        [JsonProperty("device_name")]
        public string DeviceName { get; set; }
        [JsonProperty("miner_id", NullValueHandling = NullValueHandling.Ignore)]
        public List<string>? MinerId { get; set; }
        [JsonProperty("algorithm_id", NullValueHandling = NullValueHandling.Ignore)]
        public List<string>? AlgoId { get; set; }
        [JsonProperty("type")]
        public int Type { get; set; }
        [JsonProperty("fan_speed")]
        public int FanSpeed { get; set; }
        [JsonProperty("gpu_temp")]
        public int GpuTemp { get; set; }
        [JsonProperty("vram_temp")]
        public int VramTemp { get; set; }
        [JsonProperty("max_fan_speed")]
        public int MaxFanSpeed { get; set; }
    }
    internal class Limit
    {
        [JsonProperty("name")]
        public string Name { get; set; }
        [JsonProperty("unit")]
        public string Unit { get; set; }
        [JsonProperty("default")]
        public int Def { get; set; }
        [JsonProperty("range")]
        [JsonConverter(typeof(Nhmws4JSONConverter))]
        public (int min, int max) Range { get; set; }
    }

}

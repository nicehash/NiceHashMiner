using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Security.Policy;
using System.Threading.Tasks;
using System.Windows.Media.Animation;

namespace NHMCore.Nhmws.V4
{
    public enum Type : int
    {
        Int = 0,
        Bool = 1,
        Enum = 2,
        String = 3,
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
        //[JsonProperty("group")]
        //public string Group { get; set; } = "";

        // "static_properties": { ... },

        // "optional_dynamic_properties": [ ... ],
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


    //internal class LoginMessageBreak : ISendMessage
    //{
    //    [JsonProperty("method")]
    //    public string Method => "loginBreak2";
    //}


    // new stuff

    //internal class MinerBye : ISendMessage
    //{
    //    [JsonProperty("method")]
    //    public string Method => "miner.bye";
    //    [JsonProperty("params")]
    //    public List<object> Params = new List<object>();
    //}

    //internal class ServerBye : IReceiveMessage
    //{
    //    [JsonProperty("method")]
    //    public string Method => "server.bye";
    //    [JsonProperty("params")]
    //    public List<object> Params = new List<object>();
    //}




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

        [JsonProperty("display_unit")]
        public string DisplayUnit { get; set; }

        [JsonProperty("type")]
        abstract public Type PropertyType { get; }

        //[JsonProperty("default")]
        //public T DefaultValue { get; set; }

        //[JsonProperty("range")]
        //public TR Range { get; set; }

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

    internal class NhmwsAction
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
        public List<Parameter> Parameters { get; set; } = new();

        [JsonIgnore]
        public Func<Task<object>> ExecuteTask { get; set; }
    }

    internal class Device
    {
        [JsonProperty("static_properties")]
        public Dictionary<string, object> StaticProperties { get; set; }

        [JsonProperty("optional_dynamic_properties")]
        [JsonConverter(typeof(Nhmws4JSONConverter))]
        public List<(string name, string unit)> OptionalDynamicProperties { get; set; }

        //
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

            [JsonProperty("odv")]
            public JArray OptionalDynamicValues { get; set; }

            [JsonProperty("mmv")]
            public JArray MandatoryMutableValues { get; set; }

            [JsonProperty("omv")]
            public JArray OptionalMutableValues { get; set; }
        }

        [JsonProperty("method")]
        public string Method => "miner.state";

        [JsonProperty("mdv")]
        public JArray MutableDynamicValues { get; set; }

        [JsonProperty("odv")]
        public JArray OptionalDynamicValues { get; set; }

        [JsonProperty("mmv")]
        public JArray MandatoryMutableValues { get; set; }

        [JsonProperty("omv")]
        public JArray OptionalMutableValues { get; set; }

        [JsonProperty("devices")]
        public List<DeviceState> Devices { get; set; }
    }

    internal abstract class Parameter
    {
        [JsonProperty("display_name")]
        public string DisplayName { get; set; }
        [JsonProperty("display_group")]
        public int DisplayGroup { get; set; }
        [JsonProperty("display_unit")]
        public string DisplayUnit { get; set; }
        [JsonProperty("type")]
        abstract public Type PropertyType { get; }
    }
    internal class ParameterInteger : Parameter
    {
        [JsonProperty("type")]
        public override Type PropertyType => Type.Int;

        [JsonProperty("default")]
        public int DefaultValue { get; set; }

        [JsonProperty("range")]
        [JsonConverter(typeof(Nhmws4JSONConverter))]
        public (int min, int max) Range { get; set; }
    }
    internal class ParameterBool : Parameter
    {
        [JsonProperty("type")]
        public override Type PropertyType => Type.Bool;

        [JsonProperty("default")]
        public bool DefaultValue { get; set; }
    }
    internal class ParameterEnum : Parameter
    {
        [JsonProperty("type")]
        public override Type PropertyType => Type.Enum;

        [JsonProperty("default")]
        public string DefaultValue { get; set; }

        [JsonProperty("range")]
        public List<string> Range { get; set; }
    }
    internal class ParameterString : Parameter
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
    internal class Miner
    {
        [JsonProperty("id")]
        public string Id { get; set; }
        [JsonProperty("algorithms")]
        public List<string> Algos { get; set; } = new List<string>();
    }

    internal class Bundle
    {
        [JsonProperty("id")]
        public string Id { get; set; }
        [JsonProperty("name")]
        public string Name { get; set; }
        [JsonProperty("oc")]
        public List<OcBundle> OcBundles { get; set; } = new List<OcBundle>();
        [JsonProperty("fan")]
        public List<FanBundle> FanBundles { get; set; } = new List<FanBundle>();
        [JsonProperty("elp")]
        public List<ElpBundle> ElpBundles { get; set; } = new List<ElpBundle>();
    }
    internal class ElpBundle
    {
        [JsonProperty("name")]
        public string Name { get; set; }
        [JsonProperty("device_name")]
        public string DeviceName { get; set; }
        [JsonProperty("miner_id")]
        public string MinerId { get; set; }
        [JsonProperty("algorithm_id")]
        public string AlgoId { get; set; }
        [JsonProperty("elp")]
        public string Elp { get; set; }
    }
    internal class OcBundle
    {
        [JsonProperty("name")]
        public string Name { get; set; }
        [JsonProperty("device_name")]
        public string DeviceName { get; set; }
        [JsonProperty("miner_id", Required = Required.AllowNull)]
        public string? MinerId { get; set; }
        [JsonProperty("algorithm_id", Required = Required.AllowNull)]
        public string? AlgoId { get; set; }
        [JsonProperty("core_clock")]
        public int CoreClock { get; set; }
        [JsonProperty("memory_clock")]
        public int MemoryClock { get; set; }
        [JsonProperty("tdp")]
        public int TDP { get; set; }
    }

    internal abstract class FanBundle
    {
        [JsonProperty("name")]
        public string Name { get; set; }
        [JsonProperty("device_name")]
        public string DeviceName { get; set; }
        [JsonProperty("miner_id", Required = Required.AllowNull)]
        public string? MinerId { get; set; }
        [JsonProperty("algorithm_id", Required = Required.AllowNull)]
        public string? AlgoId { get; set; }
        [JsonProperty("type")]
        abstract public int Type { get; set; }
    }
    internal class FanBundleFixed : FanBundle
    {
        [JsonProperty("type")]
        public override int Type { get; set; } = 0;
        [JsonProperty("fan_speed")]
        public int FanSpeed { get; set; }
    }
    internal class FanBundleGPUTemp : FanBundle
    {
        [JsonProperty("type")]
        public override int Type { get; set; } = 1;
        [JsonProperty("gpu_temp")]
        public int GpuTemp { get; set; }
    }
    internal class FanBundleVramAndGPUTemp : FanBundle
    {
        [JsonProperty("type")]
        public override int Type { get; set; } = 2;
        [JsonProperty("gpu_temp")]
        public int GpuTemp { get; set; }
        [JsonProperty("vram_temp")]
        public int VramTemp { get; set; }
        [JsonProperty("max_fan_speed")]
        public int MaxFanSpeed { get; set; }
    }
    //internal class Limit
    //{
    //    [JsonProperty("name")]
    //    public string Name { get; set; }
    //    [JsonProperty("unit")]
    //    public string Unit { get; set; }
    //    [JsonProperty("default")]
    //    public int def { get; set; }
    //    [JsonProperty("range")]
    //    [JsonConverter(typeof(Nhmws4JSONConverter))]
    //    public (int min, int max) range { get; set; }
    //}

}

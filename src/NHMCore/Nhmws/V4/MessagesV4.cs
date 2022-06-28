using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NHMCore.Nhmws.V4
{
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

        [JsonProperty("optional_mutable_properties")]
        public List<OptionalMutableProperty> OptionalMutableProperties { get; set; }

        [JsonProperty("actions")]
        public List<NhnwsAction> Actions { get; set; }

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
        public enum Type : int
        {
            Int = 0,
            Bool = 1,
            Enum = 2,
            String = 3,
        }

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

    internal class NhnwsAction
    {
        private static int _actionId = 0;
        internal static int NextActionId() => _actionId++;

        [JsonProperty("action_id")]
        public int ActionID { get; set; }

        [JsonProperty("display_name")]
        public string DisplayName { get; set; }

        [JsonProperty("display_group")]
        public int DisplayGroup { get; set; }

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
        public List<NhnwsAction> Actions { get; set; }

    }

    internal class MinerState : ISendMessage
    {
        internal class DeviceState
        {
            [JsonProperty("mdv")]
            public JArray MutableDynamicValues { get; set; }

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

}

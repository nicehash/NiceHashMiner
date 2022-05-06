using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace NHMCore.Nhmws.V4
{
    internal interface IMethod
    {
        string Method { get; }
    }

    internal interface IReceiveMessage : IMethod { }

    // RPC message is named as Action
    internal interface IReceiveRpcMessage : IReceiveMessage
    {
        int Id { get; }
    }

    internal interface ISetCredentialsMessage : IReceiveRpcMessage { }

    internal interface ISetDeviceMessage : IReceiveRpcMessage
    {
        string Device { get; }
    }

    internal interface ISendMessage : IMethod { }

    internal class ObsoleteMessage : IReceiveMessage
    {
        [JsonProperty("method")]
        public string Method { get; set; }
    }

    internal class UnknownMessage : IReceiveMessage
    {
        [JsonProperty("method")]
        public string Method { get; set; }
    }

    internal class SmaMessage : IReceiveMessage
    {
        [JsonProperty("method")]
        public string Method => "sma";
        [JsonProperty("data")]
        public List<List<object>> Data { get; set; }
        [JsonProperty("stable")]
        public string Stable { get; set; }
    }

    internal class BalanceMessage : IReceiveMessage
    {
        [JsonProperty("method")]
        public string Method => "balance";
        [JsonProperty("value")]
        public string Value { get; set; }
    }

    internal class VersionsMessage : IReceiveMessage
    {
        [JsonProperty("method")]
        public string Method => "versions";
        // ignore v2
        // ignore legacy
        [JsonProperty("v3")]
        public string V3 { get; set; }
    }

    internal class BurnMessage : IReceiveMessage
    {
        [JsonProperty("method")]
        public string Method => "burn";
        [JsonProperty("message")]
        public string Message { get; set; }
    }

    internal class ExchangeRatesMessage : IReceiveMessage
    {
        public class ExchangeRatesData
        {
            [JsonProperty("exchanges")]
            public List<Dictionary<string, string>> Exchanges { get; set; }
            [JsonProperty("exchanges_fiat")]
            public Dictionary<string, double> ExchangesFiat { get; set; }
        }

        [JsonProperty("method")]
        public string Method => "exchange_rates";
        [JsonProperty("data")]
        public string Data { get; set; }
    }

    // RPC

    internal class MiningSetUsername : ISetCredentialsMessage
    {
        [JsonProperty("method")]
        public string Method => "mining.set.username";
        [JsonProperty("id")]
        public int Id { get; set; }
        [JsonProperty("username")]
        public string Btc { get; set; }
    }

    internal class MiningSetWorker : ISetCredentialsMessage
    {
        [JsonProperty("method")]
        public string Method => "mining.set.worker";
        [JsonProperty("id")]
        public int Id { get; set; }
        [JsonProperty("worker")]
        public string Worker { get; set; }
    }

    internal class MiningSetGroup : ISetCredentialsMessage
    {
        [JsonProperty("method")]
        public string Method => "mining.set.group";
        [JsonProperty("id")]
        public int Id { get; set; }
        [JsonProperty("group")]
        public string Group { get; set; }
    }

    internal class MiningEnable : ISetDeviceMessage
    {
        [JsonProperty("method")]
        public string Method => "mining.enable";
        [JsonProperty("id")]
        public int Id { get; set; }
        [JsonProperty("device")]
        public string Device { get; set; }
    }

    internal class MiningDisable : ISetDeviceMessage
    {
        [JsonProperty("method")]
        public string Method => "mining.disable";
        [JsonProperty("id")]
        public int Id { get; set; }
        [JsonProperty("device")]
        public string Device { get; set; }
    }

    internal class MiningStart : ISetDeviceMessage
    {
        [JsonProperty("method")]
        public string Method => "mining.start";
        [JsonProperty("id")]
        public int Id { get; set; }
        [JsonProperty("device")]
        public string Device { get; set; }
    }

    internal class MiningStop : ISetDeviceMessage
    {
        [JsonProperty("method")]
        public string Method => "mining.stop";
        [JsonProperty("id")]
        public int Id { get; set; }
        [JsonProperty("device")]
        public string Device { get; set; }
    }

    internal class MiningSetPowerMode : ISetDeviceMessage
    {
        [JsonProperty("method")]
        public string Method => "mining.set.power_mode";
        [JsonProperty("id")]
        public int Id { get; set; }
        [JsonProperty("device")]
        public string Device { get; set; }
        [JsonProperty("power_mode")]
        public int PowerMode { get; set; }
    }

    internal class MinerReset : IReceiveRpcMessage
    {
        [JsonProperty("method")]
        public string Method => "miner.reset";
        [JsonProperty("id")]
        public int Id { get; set; }
        [JsonProperty("level")]
        public string Level { get; set; }
    }

    // RPC response
    internal class ExecutedCall : ISendMessage
    {
        [JsonProperty("method")]
        public string Method => "executed";
        [JsonProperty("params")]
        public List<object> Params = new List<object>();

        public ExecutedCall(int messageID, int errorCode, string message = null)
        {
            Params.Add(messageID);
            Params.Add(errorCode);
            if (message != null) Params.Add(message);
        }
    }

    internal class MinerStatusMessage : ISendMessage
    {
        [JsonProperty("method")]
        public string Method => "miner.status";
        [JsonProperty("params")]
        public List<JToken> Params { get; set; }
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

        [JsonProperty("optional_mutable_properties")]
        public List<OptionalMutableProperty> OptionalMutableProperties { get; set; }

        [JsonProperty("actions")]
        public List<NhnwsAction> Actions { get; set; }

        [JsonProperty("devices")]
        public List<Device> Devices { get; set; }

        [JsonProperty("miner.state")]
        public Dictionary<string, string> MinerState { get; set; } = new Dictionary<string, string>();
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
        public int? DisplayGroup { get; set; }

        [JsonProperty("display_unit")]
        public string DisplayUnit { get; set; }

        [JsonProperty("type")]
        abstract public Type PropertyType { get; }

        //[JsonProperty("default")]
        //public T DefaultValue { get; set; }

        //[JsonProperty("range")]
        //public TR Range { get; set; }
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
    }

    internal class Device
    {
        [JsonProperty("static_properties")]
        public Dictionary<string, object> StaticProperties { get; set; }

        [JsonProperty("optional_dynamic_properties")]
        public List<(string name, string unit)> OptionalDynamicProperties { get; set; }

        //
        [JsonProperty("optional_mutable_properties")]
        public List<OptionalMutableProperty> OptionalMutableProperties { get; set; }

        [JsonProperty("actions")]
        public List<NhnwsAction> Actions { get; set; }

    }

}

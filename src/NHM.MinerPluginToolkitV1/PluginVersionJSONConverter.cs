using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace NHM.MinerPluginToolkitV1
{
    public class PluginVersionJSONConverter : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException("Not needed to implement");
        }


        public override object ReadJson(JsonReader reader, Type objectType, object existingValue,
            JsonSerializer serializer)
        {
            JObject jObject = JObject.Load(reader);

            // Create target object based on JObject
            var v = new Version((int)jObject.GetValue("major"), (int)jObject.GetValue("minor"));

            return v;
        }

        public override bool CanRead => true;

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof((int major, int minor));
        }
    }
}

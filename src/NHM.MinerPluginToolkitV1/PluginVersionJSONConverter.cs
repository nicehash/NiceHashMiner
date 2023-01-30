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
            var version = new Version(value.ToString());
            var jObject = new JObject();
            jObject.Add("major", version.Major);
            jObject.Add("minor", version.Minor);
            serializer.Serialize(writer, jObject);
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

using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodexLib
{
    public class LocalizedStringConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(LocalizedString);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var jt = JToken.Load(reader);
            if (jt.Type != JTokenType.String)
                return new LocalizedString();

            var val = jt.ToObject<string>().Split(':');
            if (val.Length < 3)
                return new LocalizedString();

            return new LocalizedString() { m_Key = val[1] };
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            if (value is LocalizedString loc)
                writer.WriteValue($"LocalizedString:{loc.m_Key}:");
            else
                writer.WriteNull();
        }
    }
}

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;

#pragma warning disable CS1591

namespace Shared
{
    [Serializable]
    public readonly struct StringHash : IEquatable<StringHash>, IEquatable<string>, IComparable<StringHash>, IComparable<string>, IComparable
    {
        public readonly string Value;
        [NonSerialized] public readonly int Hash;

        public StringHash(string value)
        {
            this.Value = value;
            this.Hash = value.GetHashCode();

            //byte[] hash = _SHA.ComputeHash(Encoding.UTF8.GetBytes(value));
            //string.Concat(hash.Select(b => b.ToString("x2")));
        }

        public override int GetHashCode()
        {
            return Hash;
        }

        public override string ToString()
        {
            return Value;
        }

        public override bool Equals(object obj)
        {
            if (obj is StringHash otherHash)
                return this.Hash == otherHash.Hash;
            if (obj is string otherString)
                return this.Value == otherString;
            return false;
        }

        public bool Equals(StringHash other) => this.Hash == other.Hash;
        public bool Equals(string other) => this.Value == other;
        public int CompareTo(StringHash other) => this.Hash - other.Hash;
        public int CompareTo(string other) => this.Hash - other.GetHashCode();
        public int CompareTo(object obj) => this.Hash - obj?.GetHashCode() ?? 0;

        //private static readonly SHA1 _SHA = SHA1.Create();

        public static implicit operator StringHash(string text)
        {
            return new(text);
        }

        public static implicit operator string(StringHash hash)
        {
            return hash.Value;
        }
    }

    internal class StringHashConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(StringHash);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var jt = JToken.Load(reader);
            switch (jt.Type)
            {
                case JTokenType.String:
                    return jt.ToString();
                case JTokenType.Object:
                    //var type = jt["$type"]?.ToString();
                    //Type.GetType(type);
                    return jt["Value"]?.ToString();
            }
            return null;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            if (value is null)
            {
                writer.WriteNull();
            }
            else if (value is StringHash hash)
            {
                writer.WriteValue(hash.ToString());
            }
            else
            {
                writer.WriteStartObject();
                writer.WritePropertyName("$type");
                writer.WriteValue(value.GetType());
                writer.WritePropertyName("Value");
                writer.WriteValue(value.ToString());
                writer.WriteEndObject();
            }
        }
    }
}

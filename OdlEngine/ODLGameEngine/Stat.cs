using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace ODLGameEngine
{
    /// <summary>
    /// Defines a stat, has a value and a modifier (e.g. buff/debuff)
    /// </summary>
    [JsonConverter(typeof(StatJsonConverter))]
    [JsonObject(MemberSerialization.OptIn)]
    public class Stat : ICloneable, IHashable
    {
        protected int _minTotalCap = 0;
        int _baseValue = 0;
        [JsonProperty]
        public int BaseValue
        {
            get
            {
                return _baseValue;
            }
            set
            {
                _baseValue = (value < _minTotalCap) ? _minTotalCap : value;
            }
        }
        int _modifier = 0;
        [JsonProperty]
        public int Modifier
        {
            get
            {
                return _modifier;
            }
            set
            {
                _modifier = ((BaseValue + value) < _minTotalCap) ? (_minTotalCap - BaseValue) : value;
            }
        }
        public int Total
        {
            get { return BaseValue + Modifier; }
        }
        public object Clone()
        {
            return MemberwiseClone();
        }
        public override string ToString()
        {
            return BaseValue.ToString() + ((Modifier >= 0) ? "+" : "") + Modifier.ToString() + $" ({Total})";
        }
        /// <summary>
        /// Gets Hash of stat
        /// </summary>
        /// <returns>Hash, didn't add dirty flag because it's probably pointless</returns>
        public int GetGameStateHash()
        {
            HashCode hash = new HashCode();
            hash.Add(_baseValue);
            hash.Add(_modifier);
            return hash.ToHashCode();
        }
    }
    [JsonConverter(typeof(StatJsonConverter))]
    public class Min1Stat : Stat
    {
        public Min1Stat()
        {
            BaseValue = _minTotalCap = 1;
        }
        public override string ToString()
        {
            return base.ToString();
        }
    }
    [JsonConverter(typeof(StatJsonConverter))]
    public class Min0Stat : Stat
    {
        public Min0Stat()
        {
            BaseValue = _minTotalCap = 0;
        }
        public override string ToString()
        {
            return base.ToString();
        }
    }

    /// <summary>
    /// Deserialzies stats in whichever way we can/want, either as a single value (easy to read) or the complete thing if has been modified
    /// </summary>
    public class StatJsonConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(Stat);
        }
        public override Stat ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            Stat ret = (Stat)Activator.CreateInstance(objectType);
            if (reader.TokenType == JsonToken.Integer)
            {
                // Deserialize from a single int
                ret.BaseValue = Convert.ToInt32(reader.Value);
            }
            else if (reader.TokenType == JsonToken.StartObject)
            {
                // Read the JSON object manually
                while (reader.TokenType != JsonToken.EndObject)
                {
                    if (reader.TokenType == JsonToken.PropertyName)
                    {
                        string propertyName = reader.Value.ToString();
                        reader.Read(); // Move to the value token
                        switch(propertyName)
                        {
                            case "BaseValue":
                                ret.BaseValue = Convert.ToInt32(reader.Value);
                                break;
                            case "Modifier":
                                ret.Modifier = Convert.ToInt32(reader.Value);
                                break;
                            default:
                                break;
                        }
                    }
                    reader.Read(); // Move to the next
                }
            }
            return ret;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            Stat theStat = (Stat)value;
            if (theStat.Modifier == 0)
            {
                writer.WriteValue(theStat.BaseValue);
            }
            else
            {
                writer.WriteStartObject();
                writer.WritePropertyName("BaseValue");
                writer.WriteValue(theStat.BaseValue);
                writer.WritePropertyName("Modifier");
                writer.WriteValue(theStat.Modifier);
                writer.WriteEndObject();
            }
            
        }
    }
}

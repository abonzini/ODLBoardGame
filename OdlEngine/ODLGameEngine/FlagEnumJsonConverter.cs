using Newtonsoft.Json;

namespace ODLGameEngine
{
    public class FlagEnumJsonConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            // Can convert only if its a flagged enum
            return objectType.IsDefined(typeof(FlagsAttribute), false) && objectType.IsEnum;
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            string enumString = reader.Value.ToString();
            // Get sub-strings split by | and trim
            string[] values = enumString.Split("|", StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

            int result = 0; // Work in int
            foreach (string value in values)
            {
                result |= (int)Enum.Parse(objectType, value, true);
            } // Assemble enum

            return Enum.ToObject(objectType, result); // Finally return the real enum
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            writer.WriteValue(value.ToString().Replace(", ", " | "));
        }
    }
}

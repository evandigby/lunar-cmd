using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Data.Converters
{
    public abstract class EnumToAbstractConverter<TAbstract, TEnum> : JsonConverter<TAbstract> where TEnum : Enum
    {
        public override bool CanConvert(Type typeToConvert)
        {
            var result = typeof(TAbstract).IsAssignableFrom(typeToConvert);
            return result;
        }

        public override TAbstract Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType != JsonTokenType.StartObject)
                throw new JsonException();

            using var jsonDocument = JsonDocument.ParseValue(ref reader);

            var jsonObject = jsonDocument.RootElement;

            var propName = options.PropertyNamingPolicy?.ConvertName(TypeFieldName) ?? TypeFieldName;

            if (options.PropertyNameCaseInsensitive)
            {
                foreach (var item in jsonObject.EnumerateObject())
                {
                    if (item.Name.Equals(propName, StringComparison.OrdinalIgnoreCase))
                    {
                        propName = item.Name;
                        break;
                    }
                }
            }

            var prop = jsonObject.GetProperty(propName);

            var rawText = prop.GetRawText();

            var payloadType = JsonSerializer.Deserialize<TEnum>(rawText, options);

            return Deserializer(jsonDocument, payloadType, options);
        }

        public override void Write(Utf8JsonWriter writer, TAbstract value, JsonSerializerOptions options)
        {
            writer.WriteStartObject();
            foreach (var prop in value.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                if (prop.GetMethod == null)
                    continue;

                writer.WritePropertyName(options.PropertyNamingPolicy?.ConvertName(prop.Name) ?? prop.Name);
                JsonSerializer.Serialize(writer, prop.GetValue(value), options);
            }
            writer.WriteEndObject();
        }

        public abstract string TypeFieldName { get; }
        public abstract TAbstract Deserializer(JsonDocument document, TEnum type, JsonSerializerOptions options);

    }
}

using Data.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Data.Converters
{
    public class CommandPayloadConverter : EnumToAbstractConverter<CommandPayload, PayloadType>
    {
        public override string TypeFieldName => nameof(CommandPayload.PayloadType);

        public override CommandPayload Deserializer(JsonDocument document, PayloadType type, JsonSerializerOptions options)
        {
            return type switch
            {
                PayloadType.Plaintext => JsonSerializer.Deserialize<PlaintextPayloadValue>(document.RootElement.GetRawText(), options),
                PayloadType.Binary => JsonSerializer.Deserialize<BinaryPayloadValue>(document.RootElement.GetRawText(), options),
                _ => throw new JsonException(),
            };
        }
    }
}

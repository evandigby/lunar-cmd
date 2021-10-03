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
                PayloadType.Plaintext => JsonSerializer.Deserialize<PlaintextPayload>(document.RootElement.GetRawText(), options),
                PayloadType.BinaryReference => JsonSerializer.Deserialize<BinaryReferencePayload>(document.RootElement.GetRawText(), options),
                PayloadType.Empty => JsonSerializer.Deserialize<EmptyPayload>(document.RootElement.GetRawText(), options),
                _ => throw new JsonException(),
            };
        }
    }
}

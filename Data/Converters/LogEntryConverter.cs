using Data.Log;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Data.Converters
{
    public class LogEntryConverter : EnumToAbstractConverter<LogEntry, LogEntryType>
    {
        public override string TypeFieldName => nameof(LogEntry.EntryType);

        public override LogEntry Deserializer(JsonDocument document, LogEntryType type, JsonSerializerOptions options)
        {
            return type switch
            {
                LogEntryType.Plaintext => JsonSerializer.Deserialize<PlaintextLogEntry>(document.RootElement.GetRawText(), options),
                LogEntryType.Placeholder => JsonSerializer.Deserialize<PlaceholderLogEntry>(document.RootElement.GetRawText(), options),
                _ => throw new JsonException(),
            };
        }
    }
}

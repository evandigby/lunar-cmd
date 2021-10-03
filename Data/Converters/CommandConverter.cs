using Data.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Data.Converters
{
    public class CommandConverter : EnumToAbstractConverter<Command, CommandType>
    {
        public override string TypeFieldName => nameof(Command.CommandType);

        public override Command Deserializer(JsonDocument document, CommandType type, JsonSerializerOptions options)
        {
            return type switch
            {
                CommandType.AppendLogItem => JsonSerializer.Deserialize<AppendLogEntryCommand>(document.RootElement.GetRawText(), options),
                CommandType.UpdateLogItem => JsonSerializer.Deserialize<UpdateLogEntryCommand>(document.RootElement.GetRawText(), options),  
                CommandType.UploadAttachment => JsonSerializer.Deserialize<UploadAttachmentCommand>(document.RootElement.GetRawText(), options),
                CommandType.FinalizeUserLogEntries => JsonSerializer.Deserialize<FinalizeUserLogEntriesCommand>(document.RootElement.GetRawText(), options),
                _ => throw new JsonException(),
            };
        }
    }
}

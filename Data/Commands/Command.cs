using Data.Converters;
using Data.Users;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace Data.Commands
{
    [JsonConverter(typeof(CommandConverter))]
    public abstract class Command
    {
        public Guid MissionId {  get; set; }
        public Guid Id { get; set; }
        public User User { get; set; }
        public abstract string Name { get; }
        public DateTime CreatedAt { get; set; }
        public DateTime ReceivedAt { get; set; }
        public abstract CommandType CommandType { get; }
        public CommandPayload Payload { get; set; }
        public Command()
        {
            Id = Guid.NewGuid();
            CreatedAt = DateTime.UtcNow;
        }

        public static Command Deserialize(string payload)
        {
            return JsonSerializer.Deserialize<Command>(payload, ConverterOptions.JsonSerializerOptions);
        }

        public static ValueTask<Command> DeserializeAsync(Stream payload, CancellationToken cancellationToken)
        {
            return JsonSerializer.DeserializeAsync<Command>(payload, ConverterOptions.JsonSerializerOptions, cancellationToken);
        }

        public static ValueTask<List<Command>> DeserializeListAsync(Stream payload, CancellationToken cancellationToken)
        {
            return JsonSerializer.DeserializeAsync<List<Command>>(payload, ConverterOptions.JsonSerializerOptions, cancellationToken);
        }

        public static List<Command> DeserializeList(string payload)
        {
            return JsonSerializer.Deserialize<List<Command>>(payload, ConverterOptions.JsonSerializerOptions);
        }

        public static string SerializeList(List<Command> cmds)
        {
            return JsonSerializer.Serialize(cmds, ConverterOptions.JsonSerializerOptions);
        }
    }
}

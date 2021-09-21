using Data.Converters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Data.Commands
{
    [JsonConverter(typeof(CommandConverter))]
    public abstract class Command
    {
        public Guid MissionId {  get; set; }
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string Name { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime ReceivedAt { get; set; }
        public abstract CommandType CommandType { get; }
        public CommandPayload Payload { get; set; }

        public Command()
        {
            Id = Guid.NewGuid();
            CreatedAt = DateTime.UtcNow;
        }
    }
}

using Data.Converters;
using Data.Users;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Data.Log
{

    [JsonConverter(typeof(LogEntryConverter))]
    public abstract class LogEntry
    {
        public Guid Id {  get; set; }
        public Guid MissionId { get; set; }
        public LogEntryType EntryType { get; set; }
        public User User {  get; set; }
        public DateTime LoggedAt { get; set; }
    }
}

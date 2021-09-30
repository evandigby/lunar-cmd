using Data.Commands;
using Data.Converters;
using Data.Log;
using Data.Notifications;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace LunarAPIClient.CommandProcessors
{
    internal class AppendLogEntryCommandProcessor : LogEntryProducingCommandProcessor
    {
        public Task ProcessCommand(AppendLogEntryCommand cmd, CancellationToken cancellationToken)
        {
            LogEntry entry;

            if (cmd.Payload is PlaintextPayloadValue plaintextPayloadValue)
            {
                entry = new PlaintextLogEntry
                {
                    EntryType = LogEntryType.Plaintext,
                    Value = plaintextPayloadValue.Value
                };
            }
            else
            {
                throw new Exception("Unknown log entry type");
            }

            entry.Id = Guid.NewGuid();
            entry.MissionId = cmd.MissionId;
            entry.User = cmd.User;
            entry.LoggedAt = DateTime.UtcNow;
            entry.UpdatedAt = DateTime.UtcNow;
            entry.EditHistory = Enumerable.Empty<LogEntry>().ToList();

            ProduceLogEntry(entry);
            ProduceNotification(new Notification
            {
                CommandTarget = SignalRCommands.NewLogEntry,
                Audience = Audience.Everyone,
                Message = entry.Serialize()
            });

            return Task.CompletedTask;
        }

        public override Task ProcessCommand(Command cmd, CancellationToken cancellationToken) => 
            ProcessCommand((AppendLogEntryCommand)cmd, cancellationToken);
    }
}

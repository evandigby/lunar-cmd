using Data.Commands;
using Data.Converters;
using Data.Log;
using Data.Notifications;
using LunarAPIClient.LogEntryRepository;
using LunarAPIClient.NotificationClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace LunarAPIClient.CommandProcessors
{
    internal class UpdateLogEntryCommandProcessor : LogEntryProducingCommandProcessor
    {
        private readonly ILogEntryRepository logEntryRepository;

        public UpdateLogEntryCommandProcessor(ILogEntryRepository logEntryRepository)
        {
            this.logEntryRepository = logEntryRepository;
        }
        
        public async Task ProcessCommand(UpdateLogEntryCommand cmd, CancellationToken cancellationToken)
        {
            var existingEntry = await logEntryRepository.GetById(cmd.LogEntryId, cmd.MissionId, cancellationToken);

            if (existingEntry.IsFinalized)
            {
                throw new Exception("cannot edit a finalized log entry");
            }

            if (existingEntry.MissionId != cmd.MissionId)
            {
                throw new Exception("cannot transfer log entry to another mission");
            }

            if (existingEntry.User.Id != cmd.User.Id)
            {
                throw new Exception("user can only update their own log entries");
            }

            LogEntry newEntry;
            if (cmd.Payload is PlaintextPayload plaintextPayloadValue)
            {
                newEntry = new PlaintextLogEntry
                {
                    Value = plaintextPayloadValue.Value,
                };
            }
            else
            {
                throw new Exception("Unknown log entry type");
            }

            if (existingEntry.UpdatedAt == default)
                existingEntry.UpdatedAt = existingEntry.LoggedAt;

            newEntry.Id = existingEntry.Id;
            newEntry.MissionId = existingEntry.MissionId;
            newEntry.User = cmd.User;
            newEntry.LoggedAt = existingEntry.LoggedAt;
            newEntry.UpdatedAt = DateTime.UtcNow;
            newEntry.EditHistory = new[] { existingEntry }.Concat(existingEntry.EditHistory ?? Enumerable.Empty<LogEntry>()).ToList();

            ProduceLogEntry(newEntry);
            ProduceNotification(new Notification
            {
                Audience = Audience.Everyone,
                CommandTarget = NotificationCommands.UpdateLogEntry,
                Message = newEntry
            });
        }

        public override Task ProcessCommand(Command cmd, CancellationToken cancellationToken) =>
            ProcessCommand((UpdateLogEntryCommand)cmd, cancellationToken);
    }
}

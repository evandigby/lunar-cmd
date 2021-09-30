using Data.Commands;
using Data.Converters;
using Data.Log;
using LunarAPIClient;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.SignalRService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace api.CommandProcessing
{
    internal static class ProcessAppendLogEntry
    {
        public static async Task Process(
            AppendLogEntryCommand appendLogEntryCommand,
            IAsyncCollector<string> logEntries,
            IAsyncCollector<SignalRMessage> messages,
            CancellationToken cancellationToken)
        {
            LogEntry entry;

            if (appendLogEntryCommand.Payload is PlaintextPayloadValue plaintextPayloadValue)
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
            entry.MissionId = appendLogEntryCommand.MissionId;
            entry.User = appendLogEntryCommand.User;
            entry.LoggedAt = DateTime.UtcNow;
            entry.UpdatedAt = DateTime.UtcNow;
            entry.EditHistory = Enumerable.Empty<LogEntry>().ToList();

            await logEntries.AddAsync(entry.Serialize(), cancellationToken);

            await messages.AddAsync(
                new SignalRMessage
                {
                    Target = SignalRCommands.NewLogEntry,
                    Arguments = new[] 
                    { 
                        entry
                    }
                }, 
                cancellationToken);
        }
    }
}

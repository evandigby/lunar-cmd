using Data.Commands;
using Data.Converters;
using Data.Log;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.SignalRService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace api.CommandProcessing
{
    internal static class ProcessAppendLogItem
    {
        public static async Task Process(
            AppendLogItemCommand appendLogItemCommand,
            IAsyncCollector<string> logEntries,
            IAsyncCollector<SignalRMessage> messages)
        {
            LogEntry entry = null;

            if (appendLogItemCommand.Payload is PlaintextPayloadValue plaintextPayloadValue)
            {
                entry = new PlaintextLogEntry
                {
                    Id = Guid.NewGuid(),
                    MissionId = appendLogItemCommand.MissionId,
                    EntryType = LogEntryType.Plaintext,
                    User = appendLogItemCommand.User,
                    LoggedAt = DateTime.UtcNow,
                    Value = plaintextPayloadValue.Value
                };
            }
            else
            {
                throw new Exception("Unknown log entry type");
            }

            await logEntries.AddAsync(entry.Serialize());

            await messages.AddAsync(
                new SignalRMessage
                {
                    Target = "newMessage",
                    Arguments = new[] { entry }
                }
            );
        }
    }
}

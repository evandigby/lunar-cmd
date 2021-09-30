using Data.Commands;
using Data.Converters;
using Data.Log;
using LunarAPIClient;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
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
    internal static class ProcessUpdateLogEntry
    {
        private static readonly string logEntryDb = Environment.GetEnvironmentVariable("CosmosDBDatabaseName");
        private static readonly string logEntryCollection = Environment.GetEnvironmentVariable("CosmosDBLogEntriesCollectionName");

        public static async Task Process(
            UpdateLogEntryCommand updateLogEntryCommand,
            DocumentClient logEntryDocumentClient,
            IAsyncCollector<string> logEntries,
            IAsyncCollector<SignalRMessage> messages,
            CancellationToken cancellationToken)
        {
            var docLink = $"dbs/{logEntryDb}/colls/{logEntryCollection}/docs/{updateLogEntryCommand.LogEntryId}";

            var currentLogEntry = await logEntryDocumentClient.ReadDocumentAsync(docLink, new RequestOptions
            {
                PartitionKey =  new PartitionKey(updateLogEntryCommand.MissionId.ToString())
            }, cancellationToken: cancellationToken);

            var existingEntry = LogEntry.Deserialize(currentLogEntry.Resource.ToString());

            if (existingEntry.MissionId != updateLogEntryCommand.MissionId)
            {
                throw new Exception("cannot transfer log entry to another mission");
            }

            if (existingEntry.User.Id != updateLogEntryCommand.User.Id)
            {
                throw new Exception("user can only update their own log entries");
            }

            LogEntry newEntry;
            if (updateLogEntryCommand.Payload is PlaintextPayloadValue plaintextPayloadValue)
            {
                newEntry = new PlaintextLogEntry
                {
                    EntryType = LogEntryType.Plaintext,
                    Value = plaintextPayloadValue.Value,
                };
            }
            else
            {
                throw new Exception("Unknown log entry type");
            }

            newEntry.Id = existingEntry.Id;
            newEntry.MissionId = existingEntry.MissionId;
            newEntry.User = updateLogEntryCommand.User;
            newEntry.LoggedAt = existingEntry.LoggedAt;
            newEntry.UpdatedAt = DateTime.UtcNow;
            newEntry.EditHistory = new[] { existingEntry }.Concat(existingEntry.EditHistory ?? Enumerable.Empty<LogEntry>()).ToList();

            await logEntries.AddAsync(newEntry.Serialize(), cancellationToken);

            await messages.AddAsync(
                new SignalRMessage
                {
                    Target = SignalRCommands.UpdateLogEntry,
                    Arguments = new[] 
                    {
                        newEntry
                    }
                }, 
                cancellationToken);
        }
    }
}

using Data.Converters;
using Data.Log;
using LunarAPIClient.LogEntryRepository;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.WebJobs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace api.LogEntryRepository
{
    internal class CosmosDBLogEntryRepository : ILogEntryRepository
    {
        private static readonly string logEntryDb = Environment.GetEnvironmentVariable("CosmosDBDatabaseName");
        private static readonly string logEntryCollection = Environment.GetEnvironmentVariable("CosmosDBLogEntriesCollectionName");

        private readonly IAsyncCollector<string> logEntries;
        private readonly DocumentClient logEntryDocumentClient;

        public CosmosDBLogEntryRepository(IAsyncCollector<string> logEntries, DocumentClient logEntryDocumentClient)
        {
            this.logEntries = logEntries;
            this.logEntryDocumentClient = logEntryDocumentClient;
        }

        public async Task CreateOrUpdate(IEnumerable<LogEntry> entries, CancellationToken cancellationToken)
        {
            foreach (var logEntry in entries)
            {
                await logEntries.AddAsync(logEntry.Serialize(), cancellationToken);
            }
        }

        private static string LogEntryDocLink(Guid id)
        {
            return $"dbs/{logEntryDb}/colls/{logEntryCollection}/docs/{id}";
        }

        public async Task<LogEntry> CreatePlaceholderById(Guid missionId, Guid id, IEnumerable<LogEntryAttachment> attachments, CancellationToken cancellationToken)
        {
            var docLink = LogEntryDocLink(id);

            var newDoc = await logEntryDocumentClient.CreateDocumentAsync(docLink, 
                new PlaceholderLogEntry { 
                    Id = id,
                    MissionId = missionId,
                    Attachments = attachments.ToList(),
                }, cancellationToken: cancellationToken);

            return LogEntry.Deserialize(newDoc.Resource.ToString());
        }

        public async Task<LogEntry> GetById(Guid id, Guid missionId, CancellationToken cancellationToken)
        {
            var docLink = LogEntryDocLink(id);

            var currentLogEntry = await logEntryDocumentClient.ReadDocumentAsync(docLink, new RequestOptions
            {
                PartitionKey = new PartitionKey(missionId.ToString())
            }, cancellationToken: cancellationToken);

            return LogEntry.Deserialize(currentLogEntry.Resource.ToString());
        }
    }
}

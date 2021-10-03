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

        private static string LogEntryCollection => $"dbs/{logEntryDb}/colls/{logEntryCollection}";
        private static string LogEntryDocLink(Guid id) => LogEntryDocLink(id.ToString());
        private static string LogEntryDocLink(string id) => $"{LogEntryCollection}/docs/{id}";

        public async Task<LogEntry> CreatePlaceholderById(Guid missionId, Guid id, IEnumerable<LogEntryAttachment> attachments, CancellationToken cancellationToken)
        {
            await logEntries.AddAsync(new PlaceholderLogEntry
            {
                Id = id,
                MissionId = missionId,
                Attachments = attachments.ToList(),
            }.Serialize(), cancellationToken);

            return await GetById(id, missionId, cancellationToken);
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

        public Task FinalizeLogEntriesByUserId(string userId, CancellationToken cancellationToken)
        {
            var query = new SqlQuerySpec(
                "SELECT * from l where l.user.id = @userId",
                new SqlParameterCollection(new SqlParameter[] { new SqlParameter { Name = "@userId", Value = userId } }));

            var documents = logEntryDocumentClient.CreateDocumentQuery(LogEntryCollection, query, new FeedOptions { EnableCrossPartitionQuery = true }).AsEnumerable();

            var tasks = new List<Task>();
            foreach (dynamic document in documents)
            {
                document.isFinalized = true;

                tasks.Add(logEntryDocumentClient.UpsertDocumentAsync(LogEntryCollection, document, cancellationToken: cancellationToken));
            }

            Task.WaitAll(tasks.ToArray(), cancellationToken);

            return Task.CompletedTask;
        }
    }
}

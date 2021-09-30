using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Data.Commands;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.SignalRService;
using Microsoft.Extensions.Logging;
using LunarAPIClient.CommandProcessors;
using api.LogEntryRepository;
using api.NotificationClient;

namespace api.CommandProcessing
{
    public static class CommandProcessor
    {
        [FunctionName("CommandProcessor")]
        public static async Task Run(
            [CosmosDBTrigger(
                "%CosmosDBDatabaseName%",
                "%CosmosDBCommandsCollectionName%",
                ConnectionStringSetting = "CosmosDB",
                LeaseCollectionName = "%CosmosDBLeaseCollectionName%",
                CreateLeaseCollectionIfNotExists = true
                )]
            IReadOnlyList<Document> input,
            [SignalR(HubName = "%SignalRHubName%", ConnectionStringSetting = "AzureSignalRConnectionString")] IAsyncCollector<SignalRMessage> messages,
            [CosmosDB("%CosmosDBDatabaseName%", "%CosmosDBLogEntriesCollectionName%", ConnectionStringSetting = "CosmosDB")] IAsyncCollector<string> logEntries,
            [CosmosDB("%CosmosDBDatabaseName%", "%CosmosDBLogEntriesCollectionName%", ConnectionStringSetting = "CosmosDB")] DocumentClient logEntryDocumentClient,
            ILogger log)
        {
            var tokenSource = new CancellationTokenSource();
            var exceptions = new List<Exception>();

            var logEntryRepository = new CosmosDBLogEntryRepository(logEntries, logEntryDocumentClient);
            var signalRNotificationClient = new SignalRNotificationClient(messages);

            var commandProcessor = new LunarAPIClient.CommandProcessors.CommandProcessor(logEntryRepository, signalRNotificationClient);

            foreach (var doc in input)
            {
                try
                {
                    var cmd = Command.Deserialize(doc.ToString());

                    await commandProcessor.ProcessCommand(cmd, tokenSource.Token);
                } 
                catch (Exception ex)
                {
                    exceptions.Add(ex);
                }
            }

            if (!exceptions.Any())
                return;

            if (exceptions.Count == 1)
                throw exceptions.Single();

            throw new AggregateException(exceptions);
        }
    }
}

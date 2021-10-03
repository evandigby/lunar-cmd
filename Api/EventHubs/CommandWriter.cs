using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Data.Commands;
using Microsoft.Azure.Cosmos;
using Azure.Messaging.EventHubs;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Microsoft.Azure.WebJobs.Extensions.SignalRService;
using Microsoft.Azure.Documents.Client;
using System.Threading;
using api.LogEntryRepository;
using api.NotificationClient;

namespace api.EventHubs
{
    public static class CommandWriter
    {
        [FunctionName("CommandWriter")]
        public static async Task Run(
            [EventHubTrigger("%EventHubName%", Connection = "EventHubsWrite")] EventData[] events, 
            [CosmosDB("%CosmosDBDatabaseName%", "%CosmosDBCommandsCollectionName%", ConnectionStringSetting = "CosmosDB")] IAsyncCollector<string> commands,
            [SignalR(HubName = "%SignalRHubName%", ConnectionStringSetting = "AzureSignalRConnectionString")] IAsyncCollector<SignalRMessage> messages,
            [CosmosDB("%CosmosDBDatabaseName%", "%CosmosDBLogEntriesCollectionName%", ConnectionStringSetting = "CosmosDB")] IAsyncCollector<string> logEntries,
            [CosmosDB("%CosmosDBDatabaseName%", "%CosmosDBLogEntriesCollectionName%", ConnectionStringSetting = "CosmosDB")] DocumentClient logEntryDocumentClient,
            ILogger log)
        {
            if (!events.Any())
            {
                return;
            }

            var attachmentConnectionString = Environment.GetEnvironmentVariable("AttachmentBlobStorage");
            var attachmentBlobContainer = Environment.GetEnvironmentVariable("AttachmentBlobContainer");

            var logEntryAttachmentRepository = new AzureBlobStorageLogEntryAttachmentRepository(attachmentConnectionString, attachmentBlobContainer);
            var logEntryRepository = new CosmosDBLogEntryRepository(logEntries, logEntryDocumentClient);
            var signalRNotificationClient = new SignalRNotificationClient(messages);
            var logEntryAttachmentContentTypeRepository = new FileExtenstionContentTypeProvderLogEntryAttachmentContentTypeRepository();

            var tokenSource = new CancellationTokenSource();
            var exceptions = new List<Exception>();
            var commandProcessor = new LunarAPIClient.CommandProcessors.CommandProcessor(
                logEntryRepository,
                signalRNotificationClient,
                logEntryAttachmentRepository,
                logEntryAttachmentContentTypeRepository);

            foreach (EventData eventData in events)
            {
                try
                {
                    var eventString = eventData.EventBody.ToString();
                    var cmd = Command.Deserialize(eventString);
                    await commands.AddAsync(eventString);
                    await commandProcessor.ProcessCommand(cmd, tokenSource.Token);
                }
                catch (Exception e)
                {
                    exceptions.Add(e);
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

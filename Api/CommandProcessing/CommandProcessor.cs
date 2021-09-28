using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using api.Auth;
using Data.Commands;
using Data.Converters;
using Data.Log;
using Microsoft.Azure.Documents;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.SignalRService;
using Microsoft.Extensions.Logging;

namespace api.CommandProcessing
{
    public static class CommandProcessor
    {
        [FunctionName("CommandProcessor")]
        public static async Task Run([CosmosDBTrigger(
            "%CosmosDBDatabaseName%",
            "%CosmosDBCommandsCollectionName%",
            ConnectionStringSetting = "CosmosDB",
            LeaseCollectionName = "%CosmosDBLeaseCollectionName%",
            CreateLeaseCollectionIfNotExists = true
            )]IReadOnlyList<Document> input, 
            [SignalR(HubName = "%SignalRHubName%", ConnectionStringSetting = "AzureSignalRConnectionString")] IAsyncCollector<SignalRMessage> messages,
            [CosmosDB("%CosmosDBDatabaseName%", "%CosmosDBLogEntriesCollectionName%", ConnectionStringSetting = "CosmosDB")] IAsyncCollector<string> logEntries,
            ILogger log)
        {
            var exceptions = new List<Exception>();

            foreach (var doc in input)
            {
                try
                {
                    var cmd = Command.Deserialize(doc.ToString());

                    if (cmd is AppendLogItemCommand appendLogItemCommand)
                    {
                        await ProcessAppendLogItem.Process(appendLogItemCommand, logEntries, messages);
                    }
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

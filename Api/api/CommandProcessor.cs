using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Data.Commands;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.EventHubs;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;

namespace api
{
    public static class CommandProcessor
    {
        private static readonly LazyAsync<Container> commandContainer = new(async () =>
        {

            var client = new CosmosClient(Environment.GetEnvironmentVariable("ConnectionStrings:CosmosDB"), new CosmosClientOptions
            {
                Serializer = new SystemTextCosmosSerializer()
            });

            var database = await client.CreateDatabaseIfNotExistsAsync(Environment.GetEnvironmentVariable("CommandDatabaseName"));

            return await database.Database.CreateContainerIfNotExistsAsync(new ContainerProperties
            {
                Id = Environment.GetEnvironmentVariable("CommandCollectionName"),
                PartitionKeyPath = $"/{Util.PartitionKey()}",
            });
        });

        [FunctionName("CommandProcessor")]
        public static async Task Run([EventHubTrigger("commands", Connection = "EventHubsWrite")] EventData[] events, ILogger log)
        {
            if (!events.Any())
            {
                return;
            }

            var container = await commandContainer.Value;

            var exceptions = new List<Exception>();
            
            foreach (EventData eventData in events)
            {
                try
                {
                    string messageBody = Encoding.UTF8.GetString(eventData.Body.Array, eventData.Body.Offset, eventData.Body.Count);

                    var command = Util.Deserialize<Command>(messageBody);

                    await container.CreateItemAsync(command, new PartitionKey(command.UserId.ToString()));
                }
                catch (Exception e)
                {
                    exceptions.Add(e);
                }
            }

            if (exceptions.Count > 1)
                throw new AggregateException(exceptions);

            if (exceptions.Count == 1)
                throw exceptions.Single();
        }
    }
}

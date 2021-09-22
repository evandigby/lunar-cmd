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

namespace api
{
    public static class CommandWriter
    {
         [FunctionName("CommandWriter")]
        public static async Task Run(
            [EventHubTrigger("commands", Connection = "EventHubsWrite")] EventData[] events, 
            [CosmosDB("lunar-command", "commands", ConnectionStringSetting = "CosmosDB")] IAsyncCollector<string> commands,
            ILogger log)
        {
            if (!events.Any())
            {
                return;
            }

            var exceptions = new List<Exception>();
            
            foreach (EventData eventData in events)
            {
                try
                {
                    await commands.AddAsync(eventData.EventBody.ToString());
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

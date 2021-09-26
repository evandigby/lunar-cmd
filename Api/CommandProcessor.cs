using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Data.Commands;
using Data.Log;
using Microsoft.Azure.Documents;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.SignalRService;
using Microsoft.Extensions.Logging;

namespace api
{
    public static class CommandProcessor
    {
        [FunctionName("CommandProcessor")]
        public static async Task Run([CosmosDBTrigger(
            "lunar-command",
            "commands",
            ConnectionStringSetting = "CosmosDB",
            LeaseCollectionName = "leases",
            CreateLeaseCollectionIfNotExists = true
            )]IReadOnlyList<Document> input, 
            [SignalR(HubName = "chat", ConnectionStringSetting = "AzureSignalRConnectionString")] IAsyncCollector<SignalRMessage> messages,
            [CosmosDB("lunar-command", "logEntries", ConnectionStringSetting = "CosmosDB")] IAsyncCollector<string> logEntries,
            ILogger log)
        {

            foreach (var doc in input)
            {
                try
                {
                    var cmd = Util.Deserialize<Command>(doc.ToString());

                    if (cmd is AppendLogItemCommand appendLogItemCommand)
                    {
                        LogEntry entry = null;

                        if (appendLogItemCommand.Payload is PlaintextPayloadValue plaintextPayloadValue)
                        {
                            entry = new PlaintextLogEntry
                            {
                                Id = Guid.NewGuid(),
                                MissionId = cmd.MissionId,
                                EntryType = LogEntryType.Plaintext,
                                User = cmd.User,
                                LoggedAt = DateTime.UtcNow,
                                Value = plaintextPayloadValue.Value
                            };
                        }
                        else
                        {
                            throw new Exception("Unknown log entry type");
                        }

                        await logEntries.AddAsync(Util.Serialize(entry));

                        await messages.AddAsync(
                            new SignalRMessage
                            {
                                Target = "newMessage",
                                Arguments = new[] { entry }
                            }
                        );
                    }
                } 
                catch (Exception ex)
                {
                    log.LogError(ex.Message, ex.StackTrace);
                }
            }
        }
    }
}

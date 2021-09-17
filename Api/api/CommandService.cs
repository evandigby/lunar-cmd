using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using Data.Commands;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace api
{
    public static class CommandService
    {
        private static readonly JsonSerializerOptions options = new(JsonSerializerDefaults.Web);

        [Authorize]
        [FunctionName("SendCommand")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = "command")] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            var json = await req.ReadAsStringAsync();

            try
            {
                var command = JsonSerializer.Deserialize<Command>(json, options);

                log.LogInformation($"Received {command.Name} command from {command.UserID}");

                if (command is AppendLogItemCommand appendLogItemCommand)
                {
                    log.LogInformation("Received append log item command");
                    if (appendLogItemCommand.Payload is PlaintextPayloadValue plaintextPayloadValue)
                    {
                        log.LogInformation($"Recieved plaintext payload: {plaintextPayloadValue.Value}");
                    }
                }
            }
            catch (Exception ex)
            {
                log.LogError(ex.Message);
            }


            return new OkObjectResult("");
        }
    }
}

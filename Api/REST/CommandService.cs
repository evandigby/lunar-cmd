using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Data.Commands;
using System.Security.Claims;
using System.Linq;
using System.Threading;
using Data.Users;
using api.Auth;
using Data.Converters;
using System.Text;

namespace api.REST
{
    public static class CommandService
    {
        [FunctionName("SendCommand")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "commands")] HttpRequest req,
            [EventHub("%EventHubName%", Connection = "EventHubs")] IAsyncCollector<string> commands,
            ILogger log,
            CancellationToken cancellationToken)
        {
            User user;
            try
            {
                user = await RequestValidation.AuthenticateRequest(req, StandardUsers.Contributor);
            }
            catch (Exception ex)
            {
                return new UnauthorizedObjectResult(ex);
            }

            using var cancellationSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, req.HttpContext.RequestAborted);

            Command command;
            try
            {
                command = await Command.DeserializeAsync(req.Body, cancellationSource.Token);
                command.User = user;
                command.ReceivedAt = DateTime.UtcNow;

                log.LogInformation($"Received {command.Name} command from {command.User.Id} ({command.User.Name})");
            }
            catch (Exception ex)
            {
                return new BadRequestObjectResult(ex);
            }

            try
            {
                var serialized = command.Serialize();
                // Ignore too big for now. Later handle better
                if (Encoding.UTF8.GetByteCount(serialized) < 1024 * 256)
                {
                    await commands.AddAsync(serialized, cancellationSource.Token);
                }
                else
                {
                    log.LogError($"Got too-large message: {Encoding.UTF8.GetByteCount(serialized)}");
                }
            }
            catch (Exception ex)
            {
                return new ObjectResult(ex)
                {
                    StatusCode = StatusCodes.Status502BadGateway
                };
            }

            return new AcceptedResult();
        }
    }
}

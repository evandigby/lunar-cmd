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

namespace api
{
    public static class CommandService
    {
        [FunctionName("SendCommand")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "command")] HttpRequest req,
            [EventHub("commands", Connection = "EventHubs")] IAsyncCollector<string> commands,
            ILogger log,
            CancellationToken cancellationToken)
        {
            ClaimsPrincipal claimsPrincipal;
            try
            {
                claimsPrincipal = await Auth.AuthenticateRequest(req, StandardUsers.Contributor);
            }
            catch (Exception ex)
            {
                return new UnauthorizedObjectResult(ex);
            }

            using var cancellationSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, req.HttpContext.RequestAborted);

            Guid userId;
            string userName = string.Empty;

            try
            {
                var nameIdentifier = claimsPrincipal.Claims.Where(c => c.Type == ClaimTypes.NameIdentifier).SingleOrDefault();

                if (!Guid.TryParse(nameIdentifier?.Value, out userId))
                {
                    throw new Exception("invalid user");
                }

                userName = claimsPrincipal.Claims.Where(c => c.Type == ClaimTypes.Name || c.Type == "name").SingleOrDefault()?.Value;

                if (string.IsNullOrWhiteSpace(userName))
                {
                    throw new Exception("invalid user name");
                }
            }
            catch (Exception ex)
            {
                return new BadRequestObjectResult(ex);
            }

            Command command;
            try
            {
                command = await Util.DeserializeAsync<Command>(req.Body, cancellationSource.Token);
                command.UserId = userId;
                command.CreatedBy = userName;
                command.ReceivedAt = DateTime.UtcNow;

                log.LogInformation($"Received {command.Name} command from {command.UserId}");
            }
            catch (Exception ex)
            {
                return new BadRequestObjectResult(ex);
            }

            try
            {
                await commands.AddAsync(Util.Serialize(command), cancellationSource.Token);
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

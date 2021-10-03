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
using System.Collections.Generic;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.WebJobs.Extensions.SignalRService;
using api.LogEntryRepository;
using api.NotificationClient;
using System.IO;
using Azure.Storage.Blobs;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using Data.Log;
using Data.Notifications;
using LunarAPIClient.NotificationClient;
using LunarAPIClient.CommandProcessors;
using api.CommandWriter;

namespace api.REST
{
    public static class CommandService
    {
        [FunctionName("SendCommand")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "commands")] HttpRequest req,
            [CosmosDB("%CosmosDBDatabaseName%", "%CosmosDBCommandsCollectionName%", ConnectionStringSetting = "CosmosDB")] IAsyncCollector<string> commands,
            [SignalR(HubName = "%SignalRHubName%", ConnectionStringSetting = "AzureSignalRConnectionString")] IAsyncCollector<SignalRMessage> messages,
            [CosmosDB("%CosmosDBDatabaseName%", "%CosmosDBLogEntriesCollectionName%", ConnectionStringSetting = "CosmosDB")] IAsyncCollector<string> logEntries,
            [CosmosDB("%CosmosDBDatabaseName%", "%CosmosDBLogEntriesCollectionName%", ConnectionStringSetting = "CosmosDB")] DocumentClient logEntryDocumentClient,
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

            List<Command> inputCommands;
            try
            {
                inputCommands = await Command.DeserializeListAsync(req.Body, cancellationSource.Token);
            }
            catch (Exception ex)
            {
                return new BadRequestObjectResult(ex);
            }

            var logEntryRepository = new CosmosDBLogEntryRepository(logEntries, logEntryDocumentClient);
            var signalRNotificationClient = new SignalRNotificationClient(messages);
            var logEntryAttachmentContentTypeRepository = new FileExtenstionContentTypeProvderLogEntryAttachmentContentTypeRepository();

            var exceptions = new List<Exception>();
            var commandProcessor = new LunarAPIClient.CommandProcessors.CommandProcessor(
                logEntryRepository,
                signalRNotificationClient,
                logEntryAttachmentContentTypeRepository);

            foreach (var command in inputCommands)
            {
                command.User = user;
                command.ReceivedAt = DateTime.UtcNow;
                try
                {
                    await commands.AddAsync(command.Serialize(), cancellationToken);
                    await commandProcessor.ProcessCommand(command, cancellationToken);
                }
                catch (Exception e)
                {
                    exceptions.Add(e);
                }
            }

            if (!exceptions.Any())
                return new AcceptedResult();

            if (exceptions.Count == 1)
                throw exceptions.Single();

            throw new AggregateException(exceptions);
        }

        [FunctionName("SendAttachmentCommand")]
        public static async Task<IActionResult> SendAttachmentCommand(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "commands/{missionId:guid}/{logEntryId:guid}/attachments")] HttpRequest req,
            [CosmosDB("%CosmosDBDatabaseName%", "%CosmosDBCommandsCollectionName%", ConnectionStringSetting = "CosmosDB")] IAsyncCollector<string> commands,
            [SignalR(HubName = "%SignalRHubName%", ConnectionStringSetting = "AzureSignalRConnectionString")] IAsyncCollector<SignalRMessage> messages,
            Guid missionId, Guid logEntryId,
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

            var attachmentConnectionString = Environment.GetEnvironmentVariable("AttachmentBlobStorage", EnvironmentVariableTarget.Process);
            var attachmentBlobContainer = Environment.GetEnvironmentVariable("AttachmentBlobContainer", EnvironmentVariableTarget.Process);

            if (string.IsNullOrEmpty(attachmentConnectionString))
            {
                return new BadRequestObjectResult("NO CONNECTION STRING");
            }

            if (string.IsNullOrEmpty(attachmentBlobContainer))
            {
                return new BadRequestObjectResult("NO BLOB CONTAINER");
            }
            var logEntryAttachmentRepository = new AzureBlobStorageLogEntryAttachmentRepository(attachmentConnectionString, attachmentBlobContainer);
            var signalRNotificationClient = new SignalRNotificationClient(messages);
            var commandWriter = new AsyncCollectorCommandWriter(commands);

            var processor = new BinaryStreamCommandProcessor(logEntryAttachmentRepository, signalRNotificationClient, commandWriter);

            using var cancellationSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, req.HttpContext.RequestAborted);

            var results = await processor.ProcessCommand(user, missionId, logEntryId, req.Form.Files, cancellationSource.Token).ConfigureAwait(false);

            return new CreatedResult($"api/{missionId}/{logEntryId}/attachments", results);
        }
    }
}

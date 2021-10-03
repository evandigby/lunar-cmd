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

            var attachmentConnectionString = Environment.GetEnvironmentVariable("AttachmentBlobStorage");
            var attachmentBlobContainer = Environment.GetEnvironmentVariable("AttachmentBlobContainer");

            var logEntryAttachmentRepository = new AzureBlobStorageLogEntryAttachmentRepository(attachmentConnectionString, attachmentBlobContainer);
            var signalRNotificationClient = new SignalRNotificationClient(messages);

            using var cancellationSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, req.HttpContext.RequestAborted);

            var uploadResults = new List<LogEntryAttachmentUploadResult>();

            foreach (var file in req.Form.Files)
            {
                if (!Guid.TryParse(file.Name, out Guid attachmentId))
                {
                    uploadResults.Add(new LogEntryAttachmentUploadResult
                    {
                        AttachmentId = Guid.Empty,
                        Success = false,
                        Error = $"Invalid attachment name: {file.Name} (must be Guid)",
                    });
                    continue;
                }

                try
                {

                    var cmd = new UploadAttachmentCommand
                    {
                        Id = Guid.NewGuid(),
                        User = user,
                        ReceivedAt = DateTime.UtcNow,
                        CreatedAt = DateTime.UtcNow,
                        MissionId = missionId,
                        LogEntryId = logEntryId,
                        Payload = new BinaryPayloadReference
                        {
                            AttachmentId = attachmentId,
                            OriginalFileName = file.FileName,
                        }
                    };

                    await commands.AddAsync(cmd.Serialize());

                    await logEntryAttachmentRepository.UploadAttachment(
                        missionId,
                        logEntryId,
                        attachmentId,
                        file.OpenReadStream(),
                        file.ContentType,
                        cancellationToken).ConfigureAwait(false);

                    log.LogInformation($"Uploaded {file.FileName} as {attachmentId}");
                    uploadResults.Add(new LogEntryAttachmentUploadResult
                    {
                        AttachmentId = attachmentId,
                        Success = true
                    });

                    await signalRNotificationClient.SendNotifications(new[] { new Notification
                    {
                        Audience = Audience.Everyone,
                        CommandTarget = NotificationCommands.LogEntryAttachmentUploadComplete,
                        Message = new LogEntryAttachmentUploadComplete
                        {
                            MissionId = missionId,
                            LogEntryId = logEntryId,
                            AttachmentId = attachmentId,
                        }
                    }
                    }, cancellationToken).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    uploadResults.Add(new LogEntryAttachmentUploadResult
                    {
                        AttachmentId = attachmentId,
                        Success = false,
                        Error = ex.Message
                    });
                }
            }
            return new CreatedResult($"api/{missionId}/{logEntryId}/attachments", uploadResults);
        }

        //[FunctionName("ConvertAttachment")]
        //public static async Task<IActionResult> ConvertAttachment(
        //    [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "commands/{missionId:guid}/{logEntryId:guid}/attachments/{attachmentId:guid}")] HttpRequest req,
        //    [CosmosDB("%CosmosDBDatabaseName%", "%CosmosDBCommandsCollectionName%", ConnectionStringSetting = "CosmosDB")] IAsyncCollector<string> commands,
        //    [SignalR(HubName = "%SignalRHubName%", ConnectionStringSetting = "AzureSignalRConnectionString")] IAsyncCollector<SignalRMessage> messages,
        //    [Blob("attachments/{missionId}/{logEntryId}/{attachmentId}", FileAccess.Write, Connection = "AttachmentBlobStorage")] BlobClient attachment,
        //    [Blob("attachments/{missionId}/{logEntryId}/{attachmentId}.medium", FileAccess.Write, Connection = "AttachmentBlobStorage")] BlobClient attachmentMedium,
        //    [Blob("attachments/{missionId}/{logEntryId}/{attachmentId}.small", FileAccess.Write, Connection = "AttachmentBlobStorage")] BlobClient attachmentSmall,
        //    Guid missionId, Guid logEntryId, Guid attachmentId,
        //    ILogger log,
        //    CancellationToken cancellationToken)
        //{
        //    User user;
        //    try
        //    {
        //        user = await RequestValidation.AuthenticateRequest(req, StandardUsers.Contributor);
        //    }
        //    catch (Exception ex)
        //    {
        //        return new UnauthorizedObjectResult(ex);
        //    }

        //    using var cancellationSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, req.HttpContext.RequestAborted);

        //    var exceptions = new List<Exception>();

        //    IImageFormat format;

        //    using (Image<Rgba32> input = Image.Load<Rgba32>(image, out format))
        //    {
        //        ResizeImage(input, imageSmall, ImageSize.Small, format);
        //    }

        //    image.Position = 0;
        //    using (Image<Rgba32> input = Image.Load<Rgba32>(image, out format))
        //    {
        //        ResizeImage(input, imageMedium, ImageSize.Medium, format);
        //    }

        //    foreach (var command in inputCommands)
        //    {
        //        command.User = user;
        //        command.ReceivedAt = DateTime.UtcNow;
        //        try
        //        {
        //            await commands.AddAsync(command.Serialize(), cancellationToken);
        //            await commandProcessor.ProcessCommand(command, cancellationToken);
        //        }
        //        catch (Exception e)
        //        {
        //            exceptions.Add(e);
        //        }
        //    }

        //    if (!exceptions.Any())
        //        return new AcceptedResult();

        //    if (exceptions.Count == 1)
        //        throw exceptions.Single();

        //    throw new AggregateException(exceptions);
        //}
    }
}

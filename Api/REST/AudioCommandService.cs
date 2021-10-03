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
using Newtonsoft.Json;
using System.IO;
using api.Speech;
using Data.Log;
using System.Text;
using Microsoft.Azure.WebJobs.Extensions.SignalRService;
using Microsoft.Azure.Documents.Client;
using api.LogEntryRepository;
using api.NotificationClient;
using System.Collections.Generic;
using LunarAPIClient.CommandProcessors;
using System.Net.Http;
using System.Net.Http.Headers;
using api.CommandWriter;

namespace api.REST
{
    public static class AudioCommandService
    {
        private const long blockSize = 1024 * 128; // 128k -- max event hub message size is 256k
        private const long maxFileSize = 1024 * 1024 * 50; // 50mb

        [FunctionName("SendAudioCommand")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "audiocommands/{missionId:guid}")] HttpRequest req,
            [CosmosDB("%CosmosDBDatabaseName%", "%CosmosDBCommandsCollectionName%", ConnectionStringSetting = "CosmosDB")] IAsyncCollector<string> commands,
            [SignalR(HubName = "%SignalRHubName%", ConnectionStringSetting = "AzureSignalRConnectionString")] IAsyncCollector<SignalRMessage> messages,
            [CosmosDB("%CosmosDBDatabaseName%", "%CosmosDBLogEntriesCollectionName%", ConnectionStringSetting = "CosmosDB")] IAsyncCollector<string> logEntries,
            [CosmosDB("%CosmosDBDatabaseName%", "%CosmosDBLogEntriesCollectionName%", ConnectionStringSetting = "CosmosDB")] DocumentClient logEntryDocumentClient,
            Guid missionId, ILogger log,
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

            try
            {
                IFormFile audioNote = ExtractAudioNoteFromRequest(req);

                var attachments = req.Form.Files?.Select(f =>
                {
                    return new
                    {
                        file = f,
                        attachment = new LogEntryAttachment
                        {
                            Id = Guid.NewGuid(),
                            Name = f.Name,
                            Alt = "Alternate Text"
                        },
                    };
                }).ToList();

                string textFromSpeech = await ConvertAudioNoteToText(audioNote);

                var entryCmd = new AppendLogEntryCommand
                {
                    LogEntryId = Guid.NewGuid(),
                    Attachments = (attachments?.Select(a => a.attachment) ?? Enumerable.Empty<LogEntryAttachment>()).ToList(),
                    Payload = new PlaintextPayload
                    {
                        Value = textFromSpeech
                    },
                    User = user,
                    ReceivedAt = DateTime.UtcNow
                };

                var logEntryRepository = new CosmosDBLogEntryRepository(logEntries, logEntryDocumentClient);
                var signalRNotificationClient = new SignalRNotificationClient(messages);
                var logEntryAttachmentContentTypeRepository = new FileExtenstionContentTypeProvderLogEntryAttachmentContentTypeRepository();

                var exceptions = new List<Exception>();
                var commandProcessor = new LunarAPIClient.CommandProcessors.CommandProcessor(
                    logEntryRepository,
                    signalRNotificationClient,
                    logEntryAttachmentContentTypeRepository);

                try
                {
                    await ProcessAndPropagateCommand(entryCmd, commands, commandProcessor, cancellationToken);
                }
                catch (Exception e)
                {
                    exceptions.Add(e);
                }

                if (exceptions.Any())
                    throw new AggregateException(exceptions);
                
                if (exceptions.Count == 1)
                    throw exceptions.Single();

                try
                {
                    if (attachments != null)
                    {
                        using var content = new MultipartFormDataContent();
                        var attachmentCommands = new List<Command>();
                        var formFileCollection = new FormFileCollection();
                        foreach (var attachment in attachments)
                        {
                            var file = attachment.file;
                            var formFile = new FormFile(file.OpenReadStream(), 0, file.Length, attachment.attachment.Id.ToString(), file.Name);
                            formFile.Headers = file.Headers;
                            formFile.ContentType = file.ContentType;
                            formFile.ContentDisposition = file.ContentDisposition;
                            formFileCollection.Add(formFile);
                        }

                        await SendAttachmentCommand(req, commands, messages, missionId, entryCmd.LogEntryId, formFileCollection, user, cancellationToken);
                    }
                }
                catch (Exception e)
                {
                    exceptions.Add(e);
                }

                if (exceptions.Any())
                    throw new AggregateException(exceptions);

                if (exceptions.Count == 1)
                    throw exceptions.Single();
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

        private static async Task<IActionResult> SendAttachmentCommand(HttpRequest req,
            IAsyncCollector<string> commands,
            IAsyncCollector<SignalRMessage> messages,
            Guid missionId, Guid logEntryId, FormFileCollection files, User user,
            CancellationToken cancellationToken)
        {
            var attachmentConnectionString = Environment.GetEnvironmentVariable("AttachmentBlobStorage");
            var attachmentBlobContainer = Environment.GetEnvironmentVariable("AttachmentBlobContainer");

            var logEntryAttachmentRepository = new AzureBlobStorageLogEntryAttachmentRepository(attachmentConnectionString, attachmentBlobContainer);
            var signalRNotificationClient = new SignalRNotificationClient(messages);
            var commandWriter = new AsyncCollectorCommandWriter(commands);

            var processor = new BinaryStreamCommandProcessor(logEntryAttachmentRepository, signalRNotificationClient, commandWriter);

            using var cancellationSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, req.HttpContext.RequestAborted);

            var results = await processor.ProcessCommand(user, missionId, logEntryId, files, cancellationSource.Token).ConfigureAwait(false);

            return new CreatedResult($"api/{missionId}/{logEntryId}/attachments", results);
        }

        private static IFormFile ExtractAudioNoteFromRequest(HttpRequest req)
        {
            if (req.Form.Files == null || !req.Form.Files.Any())
            {
                throw new Exception("No audio note found.");
            }
            var audioNote = req.Form.Files["file"];
            if (audioNote.ContentType != "audio/wav")
            {
                throw new Exception("Audio note received in an incorrect file type.");
            }

            return audioNote;
        }

        private static async Task ProcessAndPropagateCommand(Command command, 
                                         IAsyncCollector<string> commands, 
                                         CommandProcessor commandProcessor,
                                         CancellationToken cancellationToken)
        {
            try
            {
                await commands.AddAsync(command.Serialize(), cancellationToken);
                await commandProcessor.ProcessCommand(command, cancellationToken);
            }
            catch (Exception)
            {
                throw;
            }
        }

        private static async Task<string> ConvertAudioNoteToText(IFormFile audioNote)
        {
            var tempPath = Path.GetTempPath();

            var filePath = Path.Combine(tempPath, audioNote.FileName + ".wav");
            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await audioNote.CopyToAsync(fileStream);
            }

            var textFromSpeech = await SpeechToTextHelper.RecognizeSpeechFromFileAsync(filePath);
            File.Delete(filePath);
            return textFromSpeech;
        }
    }
}

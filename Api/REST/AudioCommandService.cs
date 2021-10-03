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

namespace api.REST
{
    public static class AudioCommandService
    {
        private const long blockSize = 1024 * 128; // 128k -- max event hub message size is 256k
        private const long maxFileSize = 1024 * 1024 * 50; // 50mb

        [FunctionName("SendAudioCommand")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "audiocommands")] HttpRequest req,
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

            try
            {
                IFormFile audioNote = ExtractAudioNoteFromRequest(req);

                var attachments = req.Form.Files?.Select(f =>
                {
                    var totalBlocks = (int)Math.Ceiling((decimal)f.Length / (decimal)blockSize);
                    return new
                    {
                        file = f,
                        attachment = new LogEntryAttachment
                        {
                            Id = Guid.NewGuid(),
                            Name = f.Name,
                            Alt = "Alternate Text",
                            PartsUploaded = 0,
                            TotalParts = totalBlocks
                        },
                    };
                }).ToList();

                string textFromSpeech = await ConvertAudioNoteToText(audioNote);

                var entryCmd = new AppendLogEntryCommand
                {
                    LogEntryId = Guid.NewGuid(),
                    Attachments = (attachments?.Select(a => a.attachment) ?? Enumerable.Empty<LogEntryAttachment>()).ToList(),
                    Payload = new PlaintextPayloadValue
                    {
                        Value = textFromSpeech
                    },
                    User = user,
                    ReceivedAt = DateTime.UtcNow
                };

                await LogAndPropagateEntries(entryCmd, commands, cancellationSource, log);

                if (attachments != null)
                {
                    foreach (var attachment in attachments)
                    {
                        using Stream input = attachment.file.OpenReadStream();

                        var currentBlock = 1;

                        byte[] buffer = new byte[blockSize];
                        int bytesRead;
                        while ((bytesRead = await input.ReadAsync(buffer, 0, buffer.Length)) > 0)
                        {
                            var fileCmd = new UploadAttachmentPartCommand
                            {
                                LogEntryId = entryCmd.LogEntryId,
                                Payload = new BinaryPayloadValue
                                {
                                    AttachmentId = attachment.attachment.Id,
                                    Value = buffer,
                                    PartNumber = currentBlock++,
                                    TotalParts = attachment.attachment.TotalParts,
                                    OriginalFileName = attachment.attachment.Name
                                },
                                User = user,
                                ReceivedAt = DateTime.UtcNow
                            };

                            await LogAndPropagateEntries(fileCmd, commands, cancellationSource, log);
                        }
                    }
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

        private static async Task LogAndPropagateEntries(Command command, 
                                         IAsyncCollector<string> commands, 
                                         CancellationTokenSource cancellationSource, 
                                         ILogger log)
        {
            log.LogInformation($"Received {command.Name} command from {command.User.Id} ({command.User.Name})");

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
            return textFromSpeech;
        }
    }
}

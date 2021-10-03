using Data.Commands;
using Data.Log;
using Data.Notifications;
using Data.Users;
using LunarAPIClient.LogEntryRepository;
using LunarAPIClient.NotificationClient;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LunarAPIClient.CommandProcessors
{
    public class BinaryStreamCommandProcessor
    {
        private readonly ILogEntryAttachmentRepository logEntryAttachmentRepository;
        private readonly INotificationClient notificationClient;
        private readonly ICommandWriter commandWriter;
        public BinaryStreamCommandProcessor(ILogEntryAttachmentRepository logEntryAttachmentRepository, INotificationClient notificationClient, ICommandWriter commandWriter)
        {
            this.logEntryAttachmentRepository = logEntryAttachmentRepository;
            this.notificationClient = notificationClient;
            this.commandWriter = commandWriter;
        }

        public async Task<List<LogEntryAttachmentUploadResult>> ProcessCommand(
            User user, 
            Guid missionId, 
            Guid logEntryId, 
            IFormFileCollection formFileCollection,
            CancellationToken cancellationToken)
        {
            var uploadResults = new List<LogEntryAttachmentUploadResult>();

            foreach (var file in formFileCollection)
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
                        Payload = new BinaryReferencePayload
                        {
                            AttachmentId = attachmentId,
                            OriginalFileName = file.FileName,
                        }
                    };

                    await commandWriter.WriteCommand(cmd, cancellationToken);

                    await logEntryAttachmentRepository.UploadAttachment(
                        missionId,
                        logEntryId,
                        attachmentId,
                        file.OpenReadStream(),
                        file.ContentType,
                        cancellationToken).ConfigureAwait(false);

                    uploadResults.Add(new LogEntryAttachmentUploadResult
                    {
                        AttachmentId = attachmentId,
                        Success = true
                    });

                    await notificationClient.SendNotifications(new[]
                    {
                        new Notification
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
                    }, cancellationToken);
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
            return uploadResults;
        }
    }
}

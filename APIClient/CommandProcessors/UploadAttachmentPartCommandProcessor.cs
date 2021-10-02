using Data.Commands;
using Data.Log;
using Data.Notifications;
using LunarAPIClient.LogEntryRepository;
using LunarAPIClient.NotificationClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LunarAPIClient.CommandProcessors
{
    internal class UploadAttachmentPartCommandProcessor : LogEntryProducingCommandProcessor, ICommandProcessor
    {
        private readonly ILogEntryRepository _logEntryRepository;
        private readonly ILogEntryAttachmentRepository _logEntryAttachmentRepository;
        private readonly ILogEntryAttachmentContentTypeRepository _logEntryAttachmentContentTypeRepository;

        public UploadAttachmentPartCommandProcessor(
            ILogEntryAttachmentRepository logEntryAttachmentRepository,
            ILogEntryRepository logEntryRepository, 
            ILogEntryAttachmentContentTypeRepository logEntryAttachmentContentTypeRepository)
        {
            this._logEntryAttachmentRepository = logEntryAttachmentRepository;
            this._logEntryRepository = logEntryRepository;
            this._logEntryAttachmentContentTypeRepository = logEntryAttachmentContentTypeRepository;
        }

        private async Task ProcessCommand(UploadAttachmentPartCommand cmd, CancellationToken cancellationToken)
        {
            if (cmd.Payload is BinaryPayloadValue binaryPayloadValue)
            {
                Task<LogEntry> logEntryTask;
                try
                {
                    logEntryTask = _logEntryRepository.GetById(cmd.LogEntryId, cmd.MissionId, cancellationToken);
                }
                catch (Exception)
                {
                    // TODO: Handle specific types of exceptions
                    logEntryTask = _logEntryRepository.CreatePlaceholderById(
                        cmd.MissionId, 
                        cmd.LogEntryId, 
                        new LogEntryAttachment[]
                        {
                            new LogEntryAttachment
                            {
                                Id = binaryPayloadValue.AttachmentId,
                                TotalParts = binaryPayloadValue.TotalParts,
                                ContentType = _logEntryAttachmentContentTypeRepository.GetFileContentType(binaryPayloadValue.OriginalFileName),
                                PartsUploaded = 0,
                                Name = binaryPayloadValue.OriginalFileName,
                            }
                        },
                        cancellationToken);
                }
                var numUploadedTask = _logEntryAttachmentRepository.UploadAttachmentPart(
                    cmd.MissionId, 
                    cmd.LogEntryId, 
                    binaryPayloadValue,
                    _logEntryAttachmentContentTypeRepository.GetFileContentType(binaryPayloadValue.OriginalFileName),
                    cancellationToken);

                var logEntry = await logEntryTask;
                var numUploaded = await numUploadedTask;

                logEntry.Attachments = logEntry.Attachments.Select(a =>
                {
                    if (a.Id != binaryPayloadValue.AttachmentId)
                        return a;

                    a.PartsUploaded = numUploaded;

                    return a;
                }).ToList();

                ProduceLogEntry(logEntry);
                ProduceNotification(new Notification
                {
                    Audience = Audience.Everyone,
                    CommandTarget = NotificationCommands.UpdateLogEntryAttachment,
                    Message = new LogEntryAttachmentPartUploadProgress
                    {
                        LogEntryId = cmd.LogEntryId,
                        AttachmentId = binaryPayloadValue.AttachmentId,
                        NumUploaded = numUploaded,
                        Total = binaryPayloadValue.TotalParts
                    }
                });
            }
            else
            {
                throw new Exception("invalid payload value for attachment");
            }
        }

        public override Task ProcessCommand(Command cmd, CancellationToken cancellationToken) => 
            ProcessCommand((UploadAttachmentPartCommand)cmd, cancellationToken);
    }
}

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
        private readonly ILogEntryRepository logEntryRepository;
        private readonly ILogEntryAttachmentRepository logEntryAttachmentRepository;

        public UploadAttachmentPartCommandProcessor(ILogEntryAttachmentRepository logEntryAttachmentRepository, ILogEntryRepository logEntryRepository)
        {
            this.logEntryAttachmentRepository = logEntryAttachmentRepository;
            this.logEntryRepository = logEntryRepository;
        }

        private async Task ProcessCommand(UploadAttachmentPartCommand cmd, CancellationToken cancellationToken)
        {
            if (cmd.Payload is BinaryPayloadValue binaryPayloadValue)
            {
                var logEntryTask = logEntryRepository.GetById(cmd.LogEntryId, cmd.MissionId, cancellationToken);
                var numUploadedTask = logEntryAttachmentRepository.UploadAttachmentPart(
                    cmd.MissionId, 
                    cmd.LogEntryId, 
                    binaryPayloadValue, 
                    cancellationToken);

                var logEntry = await logEntryTask;
                var numUploaded = await numUploadedTask;

                //var attachment = logEntry.Attachments.Where(a => a.Id == binaryPayloadValue.AttachmentId).SingleOrDefault();

                //if (attachment == null)
                //    throw new Exception("can't find attachment");

                //attachment.PartsUploaded = numUploaded;

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

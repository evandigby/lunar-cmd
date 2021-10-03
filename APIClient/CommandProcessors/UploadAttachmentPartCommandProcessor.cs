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
    internal class UploadAttachmentPartCommandProcessor : NotificationProducingCommandProcessor, ICommandProcessor
    {
        private readonly ILogEntryAttachmentRepository _logEntryAttachmentRepository;
        private readonly ILogEntryAttachmentContentTypeRepository _logEntryAttachmentContentTypeRepository;

        public UploadAttachmentPartCommandProcessor(
            ILogEntryAttachmentRepository logEntryAttachmentRepository,
            ILogEntryAttachmentContentTypeRepository logEntryAttachmentContentTypeRepository)
        {
            this._logEntryAttachmentRepository = logEntryAttachmentRepository;
            this._logEntryAttachmentContentTypeRepository = logEntryAttachmentContentTypeRepository;
        }

        private async Task ProcessCommand(UploadAttachmentPartCommand cmd, CancellationToken cancellationToken)
        {
            if (cmd.Payload is BinaryPayloadValue binaryPayloadValue)
            {
                var complete = await _logEntryAttachmentRepository.UploadAttachmentPart(
                    cmd.MissionId, 
                    cmd.LogEntryId, 
                    binaryPayloadValue,
                    _logEntryAttachmentContentTypeRepository.GetFileContentType(binaryPayloadValue.OriginalFileName),
                    cancellationToken);

                if (!complete)
                    return;

                // Must be upload race conditions
                ProduceNotification(new Notification
                {
                    Audience = Audience.Everyone,
                    CommandTarget = NotificationCommands.LogEntryAttachmentUploadComplete,
                    Message = new LogEntryAttachmentPartUploadComplete
                    {
                        MissionId = cmd.MissionId,
                        LogEntryId = cmd.LogEntryId,
                        AttachmentId = binaryPayloadValue.AttachmentId,
                    }
                });
            }
            else
            {
                throw new Exception("invalid payload value for attachment");
            }
        }

        public Task ProcessCommand(Command cmd, CancellationToken cancellationToken) => 
            ProcessCommand((UploadAttachmentPartCommand)cmd, cancellationToken);
    }
}

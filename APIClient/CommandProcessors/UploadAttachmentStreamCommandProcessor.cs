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
    internal class UploadAttachmentStreamCommandProcessor : NotificationProducingCommandProcessor, ICommandProcessor
    {
        private readonly ILogEntryAttachmentRepository _logEntryAttachmentRepository;
        private readonly ILogEntryAttachmentContentTypeRepository _logEntryAttachmentContentTypeRepository;

        public UploadAttachmentStreamCommandProcessor(
            ILogEntryAttachmentRepository logEntryAttachmentRepository,
            ILogEntryAttachmentContentTypeRepository logEntryAttachmentContentTypeRepository)
        {
            this._logEntryAttachmentRepository = logEntryAttachmentRepository;
            this._logEntryAttachmentContentTypeRepository = logEntryAttachmentContentTypeRepository;
        }

        private Task ProcessCommand(UploadAttachmentCommand cmd, CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        public Task ProcessCommand(Command cmd, CancellationToken cancellationToken) => 
            ProcessCommand((UploadAttachmentCommand)cmd, cancellationToken);
    }
}

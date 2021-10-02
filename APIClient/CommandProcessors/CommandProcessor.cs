﻿using Data.Commands;
using Data.Log;
using Data.Notifications;
using LunarAPIClient.LogEntryRepository;
using LunarAPIClient.NotificationClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace LunarAPIClient.CommandProcessors
{
    public class CommandProcessor
    {
        private readonly ILogEntryRepository _logEntryRepository;
        private readonly INotificationClient _notificationClient;
        private readonly ILogEntryAttachmentRepository _logEntryAttachmentRepository;

        public CommandProcessor(
            ILogEntryRepository logEntryRepository, 
            INotificationClient notificationClient,
            ILogEntryAttachmentRepository logEntryAttachmentRepository)
        {
            _logEntryRepository = logEntryRepository;
            _notificationClient = notificationClient;
            _logEntryAttachmentRepository = logEntryAttachmentRepository;
        }

        public async Task ProcessCommand(Command cmd, CancellationToken cancellationToken)
        {
            ICommandProcessor processor;
            if (cmd is AppendLogEntryCommand)
            {
                processor = new AppendLogEntryCommandProcessor();
            }
            else if (cmd is UpdateLogEntryCommand)
            {
                processor = new UpdateLogEntryCommandProcessor(_logEntryRepository);
            }
            else if (cmd is UploadAttachmentPartCommand)
            {
                processor = new UploadAttachmentPartCommandProcessor(_logEntryAttachmentRepository, _logEntryRepository);
            }
            else
            {
                throw new Exception("Unknown command type");
            }

            await processor.ProcessCommand(cmd, cancellationToken);

            if (processor is ILogEntryProducer logEntryProducer)
            {
                await _logEntryRepository.CreateOrUpdate(logEntryProducer.LogEntries, cancellationToken);
            }

            if (processor is INotificationProducer notificationProducer)
            {
                await _notificationClient.SendNotifications(notificationProducer.Notifications, cancellationToken);
            }
        }
    }
}

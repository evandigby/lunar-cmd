using Data.Commands;
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
        readonly ILogEntryRepository _logEntryRepository;
        readonly INotificationClient _notificationClient;

        public CommandProcessor(ILogEntryRepository logEntryRepository, INotificationClient notificationClient)
        {
            _logEntryRepository = logEntryRepository;
            _notificationClient = notificationClient;
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

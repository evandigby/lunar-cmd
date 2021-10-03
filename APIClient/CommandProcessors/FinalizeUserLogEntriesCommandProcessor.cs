using Data.Commands;
using Data.Converters;
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
    internal class FinalizeUserLogEntriesCommandProcessor : NotificationProducingCommandProcessor, ICommandProcessor
    {
        private readonly ILogEntryRepository logEntryRepository;

        public FinalizeUserLogEntriesCommandProcessor(ILogEntryRepository logEntryRepository)
        {
            this.logEntryRepository = logEntryRepository;
        }
        
        public async Task ProcessCommand(FinalizeUserLogEntriesCommand cmd, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(cmd.User?.Id))
            {
                throw new InvalidDataException(nameof(cmd.User));
            }

            await logEntryRepository.FinalizeLogEntriesByUserId(cmd.User.Id, cancellationToken);

            
            ProduceNotification(new Notification
            {
                Audience = Audience.Everyone,
                CommandTarget = NotificationCommands.RefreshAllLogEntries,
                Message = new UserLogEntriesUpdated
                {
                    UserId = cmd.User.Id,
                }
            });
        }

        public Task ProcessCommand(Command cmd, CancellationToken cancellationToken) =>
            ProcessCommand((FinalizeUserLogEntriesCommand)cmd, cancellationToken);
    }
}

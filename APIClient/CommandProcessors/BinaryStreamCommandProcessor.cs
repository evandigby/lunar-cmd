using Data.Commands;
using LunarAPIClient.NotificationClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LunarAPIClient.CommandProcessors
{
    public class BinaryStreamCommandProcessor
    {
        private readonly INotificationClient _notificationClient;

        public BinaryStreamCommandProcessor(INotificationClient notificationClient)
        {
            _notificationClient = notificationClient;
        }

        public Task ProcessCommand(Command cmd, Stream binaryStream)
        {
            return Task.CompletedTask;
        }
    }
}

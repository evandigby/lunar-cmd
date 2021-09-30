using Data.Notifications;
using LunarAPIClient;
using LunarAPIClient.NotificationClient;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.SignalRService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace api.NotificationClient
{
    internal class SignalRNotificationClient : INotificationClient
    {
        private readonly IAsyncCollector<SignalRMessage> _messages;

        public SignalRNotificationClient(IAsyncCollector<SignalRMessage> messages)
        {
            _messages = messages;
        }

        public async Task SendNotifications(IEnumerable<Notification> notifications, CancellationToken cancellationToken)
        {
            foreach (var notification in notifications)
            {
                await _messages.AddAsync(new SignalRMessage
                {
                    Target = notification.CommandTarget,
                    Arguments = new[]
                    {
                        notification.Message
                    }
                }, cancellationToken);
            }
        }
    }
}

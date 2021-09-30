using Data.Notifications;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace LunarAPIClient.NotificationClient
{
    public interface INotificationClient
    {
        public Task SendNotifications(IEnumerable<Notification> notifications, CancellationToken cancellationToken);
    }
}

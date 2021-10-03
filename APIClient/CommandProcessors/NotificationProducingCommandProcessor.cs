using Data.Notifications;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LunarAPIClient.CommandProcessors
{
    public class NotificationProducingCommandProcessor : INotificationProducer
    {
        private readonly List<Notification> _notifications = new();
        public IEnumerable<Notification> Notifications => _notifications;
        public void ProduceNotification(Notification notification) => _notifications.Add(notification);
    }
}

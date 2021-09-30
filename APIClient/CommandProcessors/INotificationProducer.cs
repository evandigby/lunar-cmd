using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Data.Notifications
{
    internal interface INotificationProducer
    {
        public IEnumerable<Notification> Notifications { get; }
    }
}

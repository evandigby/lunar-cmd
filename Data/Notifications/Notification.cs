using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Data.Notifications
{
    public class Notification
    {
        public string CommandTarget { get; set; }
        public Audience Audience {  get; set; }
        public object Message {  get; set; }
    }
}

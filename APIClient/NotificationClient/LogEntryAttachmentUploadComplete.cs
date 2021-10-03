using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LunarAPIClient.NotificationClient
{
    public class LogEntryAttachmentUploadComplete
    {
        public Guid MissionId { get; set; }
        public Guid LogEntryId { get; set; }
        public Guid AttachmentId { get; set; }
    }
}

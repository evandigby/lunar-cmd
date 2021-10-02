using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LunarAPIClient.NotificationClient
{
    public class LogEntryAttachmentPartUploadProgress
    {
        public Guid LogEntryId { get; set; }
        public Guid AttachmentId { get; set; }
        public int NumUploaded { get; set; }
        public int Total { get; set; }
    }
}

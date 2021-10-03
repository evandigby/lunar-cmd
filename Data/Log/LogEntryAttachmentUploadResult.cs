using Data.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Data.Log
{
    public class LogEntryAttachmentUploadResult
    {
        public Guid AttachmentId { get; set; }
        public bool Success {  get; set; }
        public string Error {  get; set; }
    }
}

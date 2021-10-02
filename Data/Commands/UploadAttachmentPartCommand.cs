using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Data.Commands
{
    public class UploadAttachmentPartCommand : Command
    {
        public UploadAttachmentPartCommand()
        {
            Name = nameof(UploadAttachmentPartCommand);
        }

        public Guid LogEntryId { get; set; }
        public override CommandType CommandType => CommandType.UploadAttachmentPart;
    }
}

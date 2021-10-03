using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Data.Commands
{
    public class UploadAttachmentCommand : Command
    {
        public Guid LogEntryId { get; set; }
        public override CommandType CommandType => CommandType.UploadAttachment;

        public override string Name => nameof(UploadAttachmentCommand);
    }
}

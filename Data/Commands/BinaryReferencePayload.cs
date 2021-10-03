using Data.Log;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Data.Commands
{
    public class BinaryReferencePayload : CommandPayload
    {
        public override PayloadType PayloadType => PayloadType.BinaryReference;
        public Guid AttachmentId { get; set; }
        public string AttachmentLink { get; set; }
        public string OriginalFileName { get; set; }
    }
}

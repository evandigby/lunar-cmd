using Data.Log;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Data.Commands
{
    public class BinaryPayloadValue : CommandPayload
    {
        public override PayloadType PayloadType => PayloadType.Binary;
        public Guid AttachmentId { get; set; }
        public byte[] Value { get; set; }
        public int PartNumber { get; set; }
        public int TotalParts { get; set; }
        public string ContentType { get; set; }
    }
}

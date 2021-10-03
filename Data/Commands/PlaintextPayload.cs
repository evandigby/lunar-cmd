using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Data.Commands
{
    public class PlaintextPayload : CommandPayload
    {
        public override PayloadType PayloadType => PayloadType.Plaintext;

        public string Value {  get; set; }
    }
}

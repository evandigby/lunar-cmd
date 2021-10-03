using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Data.Commands
{
    public class EmptyPayload : CommandPayload
    {
        public override PayloadType PayloadType => PayloadType.Empty;
    }
}

using Data.Converters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Data.Commands
{
    [JsonConverter(typeof(CommandPayloadConverter))]
    public abstract class CommandPayload
    {
        public abstract PayloadType PayloadType { get; }
    }
}

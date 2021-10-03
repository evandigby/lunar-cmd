using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Data.Log
{
    public class PlaintextLogEntry : LogEntry
    {
        public string Value { get; set; }

        public override LogEntryType EntryType => LogEntryType.Plaintext;
    }
}

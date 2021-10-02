using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Data.Log
{
    public class PlaceholderLogEntry : LogEntry
    {
        public override LogEntryType EntryType => LogEntryType.Placeholder;
    }
}

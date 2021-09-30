using Data.Log;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LunarAPIClient.CommandProcessors
{
    public interface ILogEntryProducer
    {
        public IEnumerable<LogEntry> LogEntries {  get; }
    }
}

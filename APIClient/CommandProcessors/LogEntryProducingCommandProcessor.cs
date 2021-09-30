using Data.Commands;
using Data.Log;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace LunarAPIClient.CommandProcessors
{
    abstract class LogEntryProducingCommandProcessor : NotificationProducingCommandProcessor, ICommandProcessor, ILogEntryProducer
    {
        private readonly List<LogEntry> _logEntries = new();
        public IEnumerable<LogEntry> LogEntries => _logEntries;

        public abstract Task ProcessCommand(Command cmd, CancellationToken cancellationToken);
        public void ProduceLogEntry(LogEntry entry) => _logEntries.Add(entry);
    }
}

using Data.Log;
using Client.State.LogState;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Client.State
{
    public class Mission
    {
        private readonly List<LogEntry> _logEntries = new();

        public IEnumerable<Log> Logs { get; set; } = Enumerable.Empty<Log>();
        public Guid Id { get; set; } = Guid.Empty;
        public string CurrentEntryText { get; set; } = string.Empty;

        public void AddLogEntry(LogEntry entry)
        {
            _logEntries.Add(entry);
            UpdateLogs();
        }

        public void AddLogEntries(IEnumerable<LogEntry> entries)
        {
            _logEntries.AddRange(entries);
            UpdateLogs();
        }

        private void UpdateLogs()
        {
            foreach (var log in Logs)
            {
                log.UpdateLogEntries(_logEntries.AsQueryable());
            }
        }
    }
}

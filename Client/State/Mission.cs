using Data.Log;
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
                if (log.Filters.Any())
                {
                    log.LogEntries = _logEntries.Where(e => log.Filters.Any(f => f.Matches(e))).ToList();
                }
                else
                {
                    log.LogEntries = _logEntries.ToList();
                }
            }
        }
    }
}

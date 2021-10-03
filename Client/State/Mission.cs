using Data.Log;
using Client.State.LogState;
using System;
using System.Collections.Generic;
using System.Linq;
using LunarAPIClient.NotificationClient;

namespace Client.State
{
    public class Mission
    {
        private List<LogEntry> _logEntries = new();

        public IEnumerable<Log> Logs { get; set; } = Enumerable.Empty<Log>();
        public Guid Id { get; set; } = Guid.Empty;
        public string CurrentEntryText { get; set; } = string.Empty;

        public void AddLogEntry(LogEntry entry)
        {
            if (!IsLogEntryValid(entry))
            {
                // Should log bad data error probably
                return;
            }
            _logEntries.Add(entry);
            UpdateLogs();
        }
        public void UpdateLogEntry(LogEntry entry)
        {
            _logEntries = _logEntries.Where(e => e.Id != entry.Id).Concat(new[] { entry }).ToList();
            UpdateLogs();
        }

        public void SetLogEntries(IEnumerable<LogEntry> entries)
        {
            _logEntries = entries.Where(IsLogEntryValid).ToList();
            UpdateLogs();
        }

        public static bool IsLogEntryValid(LogEntry entry)
        {
            return entry.User?.Id != default;
        }

        private void UpdateLogs()
        {
            foreach (var log in Logs)
            {
                log.UpdateLogEntries(
                    _logEntries
                        .Where(e => e is not PlaceholderLogEntry)
                        .AsQueryable());
            }
        }
    }
}

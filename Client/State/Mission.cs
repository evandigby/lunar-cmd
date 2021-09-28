using Data.Log;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Client.State
{
    public class Mission : StateObject
    {
        private readonly List<LogEntry> _logEntries = new();
        private readonly List<Log> _logs = new();

        public IReadOnlyCollection<Log> Logs => _logs;

        private Guid _id;
        public Guid Id 
        { 
            get => _id;
            set
            {
                _id = value;
                NotifyStateChanged();
            }
        }

        private string _currentEntryText = "";
        public string CurrentEntryText
        {
            get => _currentEntryText;
            set
            {
                if (_currentEntryText == value)
                    return;

                _currentEntryText = value;
                NotifyStateChanged();
            }
        }

        public void AddLogEntry(LogEntry entry)
        {
            _logEntries.Add(entry);
            UpdateConsoles();
        }

        public void AddLogEntries(IEnumerable<LogEntry> entries)
        {
            _logEntries.AddRange(entries);
            UpdateConsoles();
        }

        public void UpdateLogs(IEnumerable<Log> logs)
        {
            _logs.Clear();
            _logs.AddRange(logs);
            NotifyStateChanged();
        }

        private void UpdateConsoles()
        {
            foreach (var console in Logs)
            {
                if (console.Filters.Any())
                {
                    console.UpdateLogEntries(_logEntries.Where(e => console.Filters.Any(f => f.Matches(e))).ToList());
                }
                else
                {
                    console.UpdateLogEntries(_logEntries);
                }
            }
        }
    }
}

using Data.Log;
using DynamicData.Binding;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Client.State
{
    public class Log : StateObject
    {
        private readonly List<LogEntry> _entries = new();

        public IReadOnlyCollection<LogEntry> LogEntries => _entries;

        public void UpdateLogEntries(IEnumerable<LogEntry> entries)
        {
            _entries.Clear();
            _entries.AddRange(entries);
            NotifyStateChanged();
        }

        private Guid _id = Guid.Empty;
        public Guid Id
        {
            get => _id;
            set
            {
                if (_id == value)
                    return;

                _id = value;
                NotifyStateChanged();
            }
        }

        private string _name = string.Empty;
        public string Name
        {
            get => _name;
            set
            {
                if (_name == value)
                    return;

                _name = value;
                NotifyStateChanged();
            }
        }

        public ObservableCollection<LogFilter> Filters { get; set; } = new();
    }
}

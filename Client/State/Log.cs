using Data.Log;
using DynamicData.Binding;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Client.State
{
    public class Log
    {
        public IEnumerable<LogEntry> LogEntries { get; set; } = Enumerable.Empty<LogEntry>();

        public Guid Id { get; set; } = Guid.Empty;

        public string Name { get; set; } = string.Empty;

        public IEnumerable<LogFilter> Filters { get; set; } = Enumerable.Empty<LogFilter>();
    }
}

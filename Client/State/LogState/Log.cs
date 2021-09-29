using Data.Log;
using DynamicData.Binding;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Dynamic.Core;

namespace Client.State.LogState
{
    public class Log
    {
        public IQueryable<LogEntry> LogEntries { get; set; } = Enumerable.Empty<LogEntry>().AsQueryable();

        public Guid Id { get; set; } = Guid.Empty;

        public string Name { get; set; } = string.Empty;

        public IEnumerable<ILogFilter> Filters { get; set; } = Enumerable.Empty<ILogFilter>();

        public ILogSorter Sorter { get; set; } = new SortLogByExpressions($"{nameof(LogEntry.LoggedAt)} desc");

        public void UpdateLogEntries(IQueryable<LogEntry> newLogEntries)
        {
            LogEntries = Sorter.Sort(newLogEntries.Where(e => Filters.Any(f => f.Matches(e))));
        }
    }
}

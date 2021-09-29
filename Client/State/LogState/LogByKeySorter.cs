using Data.Log;
using DynamicData.Binding;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;

namespace Client.State.LogState
{
    public class SortLogByExpressions : ILogSorter
    {
        public IEnumerable<string> SortExpressions { get; set; }

        public SortLogByExpressions(string expression) : this(new string[] { expression })
        {
        }

        public SortLogByExpressions(IEnumerable<string> sortExpressions)
        {
            SortExpressions = sortExpressions;
        }

        public IQueryable<LogEntry> Sort(IQueryable<LogEntry> records)
        {
            return records.OrderBy(string.Join(", ", SortExpressions));
        }
    }
}

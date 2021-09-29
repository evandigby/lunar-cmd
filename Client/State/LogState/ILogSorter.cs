using Data.Log;
using System.Collections.Generic;
using System.Linq;

namespace Client.State
{
    public interface ILogSorter
    {
        public IQueryable<LogEntry> Sort(IQueryable<LogEntry> records);
    }
}

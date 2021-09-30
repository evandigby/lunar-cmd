using Data.Log;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace LunarAPIClient.LogEntryRepository
{
    public interface ILogEntryRepository
    {
        public Task<LogEntry> GetById(Guid id, Guid missionId, CancellationToken cancellationToken);
        public Task CreateOrUpdate(IEnumerable<LogEntry> entry, CancellationToken cancellationToken);
    }
}

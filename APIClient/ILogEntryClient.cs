using Data.Log;

namespace LunarAPIClient
{
    public interface ILogEntryClient
    {
        public Task<IEnumerable<LogEntry>> GetLogEntriesByMissionId(Guid missionId, CancellationToken cancellationToken);
    }
}

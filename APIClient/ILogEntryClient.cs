using Data.Log;

namespace LunarAPIClient
{

    public interface ILogEntryClient
    {
        public Task<IEnumerable<LogEntry>> GetLogEntriesByMissionId(Guid missionId, CancellationToken cancellationToken);
        public Task<LogEntryAttachmentData> GetLogEntryAttachment(Guid missionId, Guid logEntryId, LogEntryAttachment attachment, CancellationToken cancellationToken);
    }
}

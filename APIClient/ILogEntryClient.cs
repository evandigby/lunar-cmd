using Data.Log;

namespace LunarAPIClient
{
    public interface ILogEntryClient
    {
        public Task<IEnumerable<LogEntry>> GetLogEntriesByMissionId(Guid missionId, CancellationToken cancellationToken);
        public string GetLogEntryAttachmentUri(Guid missionId, Guid logEntryId, Guid attachmentId);
    }
}

using Data.Log;

namespace LunarAPIClient
{
    public class LogEntryAttachmentNotFoundException : Exception
    {

    }

    public interface ILogEntryClient
    {
        public Task<IEnumerable<LogEntry>> GetLogEntriesByMissionId(Guid missionId, CancellationToken cancellationToken);
        public Task<LogEntryAttachmentData?> GetLogEntryAttachment(Guid missionId, Guid logEntryId, Guid attachmentId, CancellationToken cancellationToken);
    }
}

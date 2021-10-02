using Data.Log;
using System.Net.Http.Json;

namespace LunarAPIClient
{
    public class LogEntryClient : ApiClient, ILogEntryClient
    {
        public LogEntryClient(HttpClient client) : base(client)
        {
        }

        protected override string ApiVersion => "v1.0";
        protected override string ApiEndpoint => $"api/{ApiVersion}/log-entries";

        public async Task<IEnumerable<LogEntry>> GetLogEntriesByMissionId(Guid missionId, CancellationToken cancellationToken)
        {
            var logEntries = await httpClient.GetFromJsonAsync<IEnumerable<LogEntry>>($"/{ApiEndpoint}/{missionId}", cancellationToken);

            return logEntries ?? Enumerable.Empty<LogEntry>();
        }

        public async Task<LogEntryAttachmentData> GetLogEntryAttachment(Guid missionId, Guid logEntryId, LogEntryAttachment attachment, CancellationToken cancellationToken)
        {
            var data = await httpClient.GetByteArrayAsync(GetLogEntryAttachmentUri(missionId, logEntryId, attachment.Id), cancellationToken);

            var base64attachment = Convert.ToBase64String(data);
            return new LogEntryAttachmentData
            {
                Attachment = attachment,
                DataURI = $"data:{attachment.ContentType};base64,{base64attachment}"
            };
        }

        private string GetLogEntryAttachmentUri(Guid missionId, Guid logEntryId, Guid attachmentId)
        {
            return $"{httpClient.BaseAddress}{ApiEndpoint}/{missionId}/{logEntryId}/attachments/{attachmentId}";
        }
    }
}

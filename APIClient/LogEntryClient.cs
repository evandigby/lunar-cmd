using Data.Log;
using System.Net;
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

        public async Task<LogEntryAttachmentData?> GetLogEntryAttachment(
            Guid missionId, 
            Guid logEntryId, 
            Guid attachmentId, 
            CancellationToken cancellationToken)
        {
            var response = await httpClient.GetAsync(GetLogEntryAttachmentUri(missionId, logEntryId, attachmentId), cancellationToken);

            if (response.StatusCode == HttpStatusCode.NotFound)
                return null;

            response.EnsureSuccessStatusCode();

            var data = await response.Content.ReadAsByteArrayAsync(cancellationToken);

            var base64attachment = Convert.ToBase64String(data);

            var contentType = response.Content.Headers.ContentType?.MediaType;

            return new LogEntryAttachmentData
            {
                AttachmentId = attachmentId,
                ContentType = contentType ?? "application/octet-stream",
                DataURI = $"data:{contentType};base64,{base64attachment}"
            };
        }

        private string GetLogEntryAttachmentUri(Guid missionId, Guid logEntryId, Guid attachmentId)
        {
            return $"{httpClient.BaseAddress}{ApiEndpoint}/{missionId}/{logEntryId}/attachments/{attachmentId}";
        }
    }
}

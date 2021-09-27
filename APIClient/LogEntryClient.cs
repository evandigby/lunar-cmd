using Data.Log;
using System.Net.Http.Json;

namespace LunarAPIClient
{
    public class LogEntryClient : ApiClient, ILogEntryClient
    {
        public LogEntryClient(HttpClient client) : base(client)
        {
        }

        protected override string ApiEndpoint => "/api/log-entries";

        public async Task<IEnumerable<LogEntry>> GetLogEntriesByMissionId(Guid missionId, CancellationToken cancellationToken)
        {
            var logEntries = await httpClient.GetFromJsonAsync<IEnumerable<LogEntry>>($"{ApiEndpoint}/{missionId}", cancellationToken);

            return logEntries ?? Enumerable.Empty<LogEntry>();
        }
    }
}

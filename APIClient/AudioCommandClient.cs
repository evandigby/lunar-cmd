using Data.Commands;
using Data.Converters;
using Data.Log;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace LunarAPIClient
{
    public class AudioCommandClient : ApiClient, IAudioCommandClient
    {
        public AudioCommandClient(HttpClient client) : base(client)
        {
        }

        protected override string ApiVersion => "v1.0";
        protected override string ApiEndpoint => $"/api/{ApiVersion}/audiocommands";
        protected string AttachmentApiEndpoint(Guid missionId, Guid logEntryid) => $"{ApiEndpoint}/{missionId}/{logEntryid}/attachments";

        public Uri GetEndpoint()
        {
            return new Uri(httpClient.BaseAddress.ToString() + ApiEndpoint.TrimStart('/'));
        }

        public async Task<List<LogEntryAttachmentUploadResult>> SendAttachmentsCommand(Guid missionId, Guid logEntryId, MultipartFormDataContent content, CancellationToken cancellationToken)
        {
            var resp = await httpClient.PostAsync(AttachmentApiEndpoint(missionId, logEntryId), content, cancellationToken);

            resp.EnsureSuccessStatusCode();

            var options = ConverterOptions.JsonSerializerOptions;
            if (options == null)
                throw new Exception("can never happen");

            var respStream = resp.Content.ReadAsStream(cancellationToken);
            var attachmentResults = await JsonSerializer.DeserializeAsync<List<LogEntryAttachmentUploadResult>>(respStream, options, cancellationToken);

            if (attachmentResults == null)
                throw new Exception("attachment results null");

            return attachmentResults;
        }

        public async Task SendCommands(IEnumerable<Command> cmds, CancellationToken cancellationToken)
        {
            await httpClient.PostAsJsonAsync(ApiEndpoint, cmds.ToList(), cancellationToken);
        }
    }
}

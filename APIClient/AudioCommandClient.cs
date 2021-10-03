using Data.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
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

        public Uri GetEndpoint()
        {
            return new Uri(httpClient.BaseAddress.ToString() + ApiEndpoint.TrimStart('/'));
        }

        public async Task SendCommand(Command cmd, CancellationToken cancellationToken)
        {
            await httpClient.PostAsJsonAsync(ApiEndpoint, cmd, cancellationToken);
        }
    }
}

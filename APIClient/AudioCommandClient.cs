using Data.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;

namespace LunarAPIClient
{
    public class AudioCommandClient : ApiClient, ICommandClient
    {
        public AudioCommandClient(HttpClient client) : base(client)
        {
        }

        protected override string ApiVersion => "v1.0";
        protected override string ApiEndpoint => $"/api/{ApiVersion}/audiocommands";

        public async Task SendCommand(Command cmd, CancellationToken cancellationToken)
        {
            await httpClient.PostAsJsonAsync(ApiEndpoint, cmd, cancellationToken);
        }
    }
}

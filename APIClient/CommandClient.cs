using Data.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;

namespace LunarAPIClient
{
    public class CommandClient : ApiClient, ICommandClient
    {
        public CommandClient(HttpClient client) : base(client)
        {
        }

        protected override string ApiEndpoint => "/api/commands";

        public async Task SendCommand(Command cmd, CancellationToken cancellationToken)
        {
            await httpClient.PostAsJsonAsync(ApiEndpoint, cmd, cancellationToken);
        }
    }
}

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

        protected override string ApiVersion => "v1.0";
        protected override string ApiEndpoint => $"/api/{ApiVersion}/commands";

        public async Task SendCommands(IEnumerable<Command> cmds, CancellationToken cancellationToken)
        {
            await httpClient.PostAsJsonAsync(ApiEndpoint, cmds.ToList(), cancellationToken);
        }
    }
}

using Data.Commands;
using Data.Converters;
using Data.Log;
using LunarAPIClient;
using LunarAPIClient.CommandProcessors;
using Microsoft.Azure.WebJobs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace api.CommandWriter
{
    internal class AsyncCollectorCommandWriter : ICommandWriter
    {
        private readonly IAsyncCollector<string> commands;

        public AsyncCollectorCommandWriter(IAsyncCollector<string> commands)
        {
            this.commands = commands;
        }

        public async Task WriteCommand(Command cmd, CancellationToken cancellationToken)
        {
            await commands.AddAsync(cmd.Serialize(), cancellationToken).ConfigureAwait(false);
        }
    }
}

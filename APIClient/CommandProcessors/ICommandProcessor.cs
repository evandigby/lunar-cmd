using Data.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace LunarAPIClient.CommandProcessors
{
    public interface ICommandProcessor
    {
        Task ProcessCommand(Command cmd, CancellationToken cancellationToken);
    }
}

using Data.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LunarAPIClient.CommandProcessors
{
    public interface ICommandWriter
    {
        public Task WriteCommand(Command cmd, CancellationToken cancellationToken);
    }
}

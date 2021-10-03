using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Data.Commands
{
    public class FinalizeUserLogEntriesCommand : Command
    {
        public override string Name => nameof(FinalizeUserLogEntriesCommand);

        public override CommandType CommandType => CommandType.FinalizeUserLogEntries;
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Data.Commands
{
    public class UpdateLogEntryCommand : Command
    {
        public Guid LogEntryId {  get; set; }

        public override CommandType CommandType => CommandType.UpdateLogItem;
        public override string Name => nameof(UpdateLogEntryCommand);

    }
}

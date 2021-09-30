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

        public UpdateLogEntryCommand() {
            Name = nameof(UpdateLogEntryCommand);
        }

        public override CommandType CommandType => CommandType.UpdateLogItem;
    }
}

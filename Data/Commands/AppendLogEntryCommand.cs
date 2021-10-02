using Data.Log;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Data.Commands
{
    public class AppendLogEntryCommand : Command
    {
        public AppendLogEntryCommand() {
            Name = nameof(AppendLogEntryCommand);
        }

        public Guid LogEntryId { get; set; }
        public List<LogEntryAttachment> Attachments { get; set; }

        public override CommandType CommandType => CommandType.AppendLogItem;
    }
}

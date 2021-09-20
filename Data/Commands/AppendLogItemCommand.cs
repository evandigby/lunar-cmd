using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Data.Commands
{
    public class AppendLogItemCommand : Command
    {
        public AppendLogItemCommand() {
            Name = nameof(AppendLogItemCommand);
        }

        public override CommandType CommandType => CommandType.AppendLogItem;
    }
}

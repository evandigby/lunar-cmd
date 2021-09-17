using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Data.Commands
{
    public class AppendLogItemCommand : Command
    {
        public AppendLogItemCommand() { }

        public AppendLogItemCommand(Guid userID) : base(userID)
        {
            Name = nameof(AppendLogItemCommand);
        }
        public AppendLogItemCommand(Guid userID, CommandPayload payload) : this(userID)
        {
            Payload = payload;
        }

        public override CommandType CommandType => CommandType.AppendLogItem;
    }
}

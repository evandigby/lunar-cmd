﻿using System;
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

        public override CommandType CommandType => CommandType.AppendLogItem;
    }
}

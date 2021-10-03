using Data.Commands;
using Data.Log;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LunarAPIClient
{
    public interface ICommandClient
    {
        public Task SendCommands(IEnumerable<Command> cmds, CancellationToken cancellationToken);
        public Task<List<LogEntryAttachmentUploadResult>> SendAttachmentsCommand(Guid missionId, Guid logEntryId, MultipartFormDataContent content, CancellationToken cancellationToken);
    }
}

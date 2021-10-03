using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LunarAPIClient.LogEntryRepository
{
    public interface ILogEntryAttachmentContentTypeRepository
    {
        public string GetFileContentType(string fileName);
    }
}

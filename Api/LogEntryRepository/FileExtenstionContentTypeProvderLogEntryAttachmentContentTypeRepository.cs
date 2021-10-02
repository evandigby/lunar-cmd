using LunarAPIClient.LogEntryRepository;
using Microsoft.AspNetCore.StaticFiles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace api.LogEntryRepository
{
    internal class FileExtenstionContentTypeProvderLogEntryAttachmentContentTypeRepository : ILogEntryAttachmentContentTypeRepository
    {
        private static readonly FileExtensionContentTypeProvider extensionContentTypeProvider = new();

        public string GetFileContentType(string fileName) =>
            extensionContentTypeProvider.TryGetContentType(fileName, out string contentType) ? contentType : "application/octet-stream";

    }
}

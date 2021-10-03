using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Blobs.Specialized;
using Data.Commands;
using LunarAPIClient.LogEntryRepository;
using Microsoft.AspNetCore.StaticFiles;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace api.LogEntryRepository
{
    internal class AzureBlobStorageLogEntryAttachmentRepository : ILogEntryAttachmentRepository
    {
        private readonly BlobContainerClient blobContainerClient;

        public AzureBlobStorageLogEntryAttachmentRepository(string azureStorageConnectionString, string containerName)
        {
            this.blobContainerClient = new BlobContainerClient(azureStorageConnectionString, containerName);
        }

        public async Task UploadAttachment(Guid missionId, Guid logEntryId, Guid attachmentId, Stream attachment, string contentType, CancellationToken cancellationToken)
        {
            var blobClient = blobContainerClient.GetBlobClient(BlobPath(missionId, logEntryId, attachmentId));

            await blobClient.UploadAsync(attachment, new BlobUploadOptions
            {
                HttpHeaders = new BlobHttpHeaders
                {
                    ContentType = contentType
                }
            },
            cancellationToken).ConfigureAwait(false);
        }

        public static string BlobPath(Guid missionId, Guid logEntryId, Guid attachmentId)
        {
            return $"{missionId}/{logEntryId}/{attachmentId}";
        }
    }
}

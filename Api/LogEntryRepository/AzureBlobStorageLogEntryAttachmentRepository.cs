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

        public async Task<int> UploadAttachmentPart(Guid missionId, Guid logEntryId, BinaryPayloadValue binaryPayloadValue, string contentType, CancellationToken cancellationToken)
        {
            var blockBlobClient = blobContainerClient.GetBlockBlobClient(BlobPath(missionId, logEntryId, binaryPayloadValue.AttachmentId));

            using var ms = new MemoryStream(binaryPayloadValue.Value);

            await blockBlobClient.StageBlockAsync(
                BlockId(binaryPayloadValue.PartNumber), 
                ms, 
                cancellationToken: cancellationToken);

            var blockList = await blockBlobClient.GetBlockListAsync(cancellationToken: cancellationToken);

            if (blockList?.Value?.UncommittedBlocks?.Count() == binaryPayloadValue.TotalParts)
            {
                await blockBlobClient.CommitBlockListAsync(
                    blockList.Value.UncommittedBlocks.OrderBy(b => BlockNumFromId(b.Name)).Select(b => b.Name),
                    httpHeaders: new BlobHttpHeaders
                    {
                        ContentType = contentType
                    },
                    cancellationToken: cancellationToken);
            }

            return blockList?.Value?.UncommittedBlocks?.Count() ?? 0;
        }

        public static int BlockNumFromId(string base64Id)
        {
            return BitConverter.ToInt32(Convert.FromBase64String(base64Id).Take(sizeof(int)).ToArray());
        }
        public static string BlockId(int partNum)
        {
            return Convert.ToBase64String(BitConverter.GetBytes(partNum).Concat(Guid.NewGuid().ToByteArray()).ToArray());
        }

        public static string BlobPath(Guid missionId, Guid logEntryId, Guid attachmentId)
        {
            return $"{missionId}/{logEntryId}/{attachmentId}";
        }
    }
}

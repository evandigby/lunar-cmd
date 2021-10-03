using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Data.Log;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Azure.Documents;
using api.Auth;
using System.IO;
using Azure.Storage.Blobs;

namespace api.REST
{
    public static class LogEntriesService
    {
        [FunctionName("LogEntries")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "log-entries/{missionId:guid}")] HttpRequest req,
            [CosmosDB(
                databaseName: "%CosmosDBDatabaseName%",
                collectionName: "%CosmosDBLogEntriesCollectionName%",
                ConnectionStringSetting = "CosmosDB",
                SqlQuery = "SELECT * from l where l.missionId = {missionId}",
                PartitionKey = "{missionId}")]IEnumerable<Document> logEntries,
            ILogger log)
        {
            try
            {
                var user = await RequestValidation.AuthenticateRequest(req, StandardUsers.Contributor);
            }
            catch (Exception ex)
            {
                return new UnauthorizedObjectResult(ex);
            }

            return new OkObjectResult(logEntries.Select(e => LogEntry.Deserialize(e.ToString())).ToArray());
        }

        [FunctionName("LogEntryAttachment")]
        public static async Task<IActionResult> RunLogEntryAttachment(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "log-entries/{missionId:guid}/{logEntryId:guid}/attachments/{attachmentId:guid}")] HttpRequest req,
            [Blob("attachments/{missionId}/{logEntryId}/{attachmentId}", FileAccess.Read, Connection = "AttachmentBlobStorage")] BlobClient attachment,
            ILogger log)
        {
            try
            {
                var user = await RequestValidation.AuthenticateRequest(req, StandardUsers.Contributor);
            }
            catch (Exception ex)
            {
                return new UnauthorizedObjectResult(ex);
            }

            bool blobExists = await attachment.ExistsAsync();

            if (!blobExists)
                return new NotFoundResult();

            var stream = await attachment.OpenReadAsync();
            var properties = await attachment.GetPropertiesAsync();

            return new FileStreamResult(stream, properties.Value.ContentType);
        }
    }
}

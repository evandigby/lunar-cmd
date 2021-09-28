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
    }
}

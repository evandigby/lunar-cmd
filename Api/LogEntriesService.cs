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

namespace api
{
    public static class LogEntriesService
    {
        [FunctionName("LogEntries")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "logEntries/{missionId:guid}")] HttpRequest req,
            [CosmosDB(
                databaseName: "lunar-command",
                collectionName: "logEntries",
                ConnectionStringSetting = "CosmosDB",
                SqlQuery = "SELECT * from logEntries l where l.missionId = {missionId}",
                PartitionKey = "{missionId}")]IEnumerable<Document> logEntries,
            ILogger log)
        {
            try
            {
                var user = await Auth.AuthenticateRequest(req, StandardUsers.Contributor);
            }
            catch (Exception ex)
            {
                return new UnauthorizedObjectResult(ex);
            }

            return new OkObjectResult(logEntries.Select(e => Util.Deserialize<LogEntry>(e.ToString())).ToArray());
        }

        [FunctionName("LawgEntries")]
        public static async Task<IActionResult> LawgEntries(
           [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "lawgEntries")] HttpRequest req,
           ILogger log)
        {
            try
            {
                var user = await Auth.AuthenticateRequest(req, StandardUsers.Contributor);
            }
            catch (Exception ex)
            {
                return new UnauthorizedObjectResult(ex);
            }

            return new OkResult();
        }
    }
}

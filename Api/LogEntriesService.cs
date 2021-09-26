using System;
using System.IO;
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
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

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
            return new OkObjectResult(logEntries.Select(e => Util.Deserialize<LogEntry>(e.ToString())).ToArray());
        }

        [FunctionName("LawgEntries")]
        public static async Task<IActionResult> LawgEntries(
           [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "lawgEntries")] HttpRequest req,
           ILogger log)
        {
            try
            {
                var accessToken = Auth.GetAccessToken(req);
                var claimsPrincipal = await Auth.ValidateAccessToken(accessToken);
                if (claimsPrincipal != null)
                {
                    return (ActionResult)new OkObjectResult(Util.Serialize(claimsPrincipal.Claims.Select(c => new { c.Type, c.Value })));
                }
                else
                {
                    return (ActionResult)new OkObjectResult($"{accessToken} HI. It didn't work");
                }
            }
            catch (Exception ex)
            {
                return (ActionResult)new OkObjectResult(ex);
            }
        }
    }
}

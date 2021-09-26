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
            var accessToken = GetAccessToken(req);
            var claimsPrincipal = await ValidateAccessToken(accessToken, log);
            if (claimsPrincipal != null)
            {
                return (ActionResult)new OkObjectResult(claimsPrincipal.FindFirst("appid").Value);
            }
            else
            {
                return (ActionResult)new OkObjectResult($"{accessToken} HI. It didn't work");
            }
        }

        private static string GetAccessToken(HttpRequest req)
        {
            var authorizationHeader = req.Headers?["Authorization"];
            string[] parts = authorizationHeader?.ToString().Split(null) ?? new string[0];
            if (parts.Length == 2 && parts[0].Equals("Bearer"))
                return parts[1];
            return null;
        }


        private static async Task<ClaimsPrincipal> ValidateAccessToken(string accessToken, ILogger log)
        {
            var clientID = "b6260c01-db46-4416-9fa4-a2e6d8b421cf";
            var authority = "https://login.microsoftonline.com/lunarcommand.onmicrosoft.com";

            // Debugging purposes only, set this to false for production
            Microsoft.IdentityModel.Logging.IdentityModelEventSource.ShowPII = true;
            ConfigurationManager<OpenIdConnectConfiguration> configManager = new($"{authority}/.well-known/openid-configuration", new OpenIdConnectConfigurationRetriever());
            OpenIdConnectConfiguration config = await configManager.GetConfigurationAsync();
            ISecurityTokenValidator tokenValidator = new JwtSecurityTokenHandler();
            // Initialize the token validation parameters
            TokenValidationParameters validationParameters = new()
            {
                // App Id URI and AppId of this service application are both valid audiences.
                ValidAudiences = new[] { "https://lunarcommand.onmicrosoft.com/cnc/LogEntries.Read", "https://lunarcommand.onmicrosoft.com/cnc/LogEntries.Write", clientID },
                // Support Azure AD V1 and V2 endpoints.
                ValidIssuers = new[] { "https://login.microsoftonline.com/a4d31d01-f721-4605-8831-34490dc0b8f5/v2.0", "https://login.microsoftonline.com/lunarcommand.onmicrosoft.com/v2.0" },
                IssuerSigningKeys = config.SigningKeys
            };
            //try
            //{
                var claimsPrincipal = tokenValidator.ValidateToken(accessToken, validationParameters, out SecurityToken securityToken);
                return claimsPrincipal;
            //}
            //catch (Exception ex)
            //{
            //    log.LogError(ex.ToString());
            //}
            //return null;
        }
    }
}

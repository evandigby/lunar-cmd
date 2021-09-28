using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using api.Auth;
using Data.Users;

namespace api.REST
{
    public static class UserService
    {
        [FunctionName("UserService")]
        public static async Task<IActionResult> Run(
            string userId,
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "users/{userId}")] HttpRequest req,
            ILogger log)
        {
            User user;
            try
            {
                user = await RequestValidation.AuthenticateRequest(req, StandardUsers.Contributor);
            }
            catch (Exception ex)
            {
                return new UnauthorizedObjectResult(ex);
            }

            if (userId == "me")
            {
                return new OkObjectResult(user);
            }

            // TODO: Add searching for other users
            return new NotFoundResult();
        }
    }
}

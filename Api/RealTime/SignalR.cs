using Microsoft.AspNetCore.Http;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.SignalRService;

namespace api.RealTime
{
    public static class SignalR
    {
        [FunctionName("negotiate")]
        public static SignalRConnectionInfo GetSignalRInfo(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post")] HttpRequest req,
            [SignalRConnectionInfo(HubName = "%SignalRHubName%", ConnectionStringSetting = "AzureSignalRConnectionString")] 
                SignalRConnectionInfo connectionInfo)
        {
            return connectionInfo;
        }
    }
}

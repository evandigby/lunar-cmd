using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace api
{
    public static class Auth
    {
        public static ClaimsPrincipal Parse(HttpRequest req)
        {
            var principal = new ClientPrincipal();

            if (req.Headers.TryGetValue("x-ms-client-principal", out var header))
            {
                var data = header[0];
                var decoded = Convert.FromBase64String(data);
                var json = Encoding.UTF8.GetString(decoded);
                principal = JsonSerializer.Deserialize<ClientPrincipal>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            } 
            else if (Guid.TryParse(Environment.GetEnvironmentVariable("LocalUserId"), out Guid envUserId))
            {
                principal.UserId = envUserId.ToString();
                principal.UserRoles = (Environment.GetEnvironmentVariable("LocalUserRoles") ?? "").Split(",");
                principal.UserDetails = Environment.GetEnvironmentVariable("LocalUserDetails");
            }

            principal.UserRoles = principal.UserRoles?.Except(new string[] { "anonymous" }, StringComparer.CurrentCultureIgnoreCase);

            if (!principal.UserRoles?.Any() ?? true)
            {
                return new ClaimsPrincipal();
            }

            var identity = new ClaimsIdentity(principal.IdentityProvider);
            identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, principal.UserId));
            identity.AddClaim(new Claim(ClaimTypes.Name, principal.UserDetails));
            identity.AddClaims(principal.UserRoles.Select(r => new Claim(ClaimTypes.Role, r)));

            return new ClaimsPrincipal(identity);
        }

        public static string GetAccessToken(HttpRequest req)
        {
            var authorizationHeader = req.Headers?["Authorization"];
            string[] parts = authorizationHeader?.ToString().Split(null) ?? new string[0];
            if (parts.Length == 2 && parts[0].Equals("Bearer"))
                return parts[1];
            return null;
        }


        public static async Task<ClaimsPrincipal> ValidateAccessToken(string accessToken)
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
                ValidAudiences = new[] { clientID },
                // Support Azure AD V1 and V2 endpoints.
                ValidIssuers = new[] { "https://login.microsoftonline.com/a4d31d01-f721-4605-8831-34490dc0b8f5/v2.0" },
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

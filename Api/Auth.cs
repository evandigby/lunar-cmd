using Data.Users;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace api
{
    public static class AuthClaims
    {
        public static readonly string ReadLogEntires = "LogEntries.Read";
        public static readonly string WriteLogEntires = "LogEntries.Write";
    }

    public static class StandardUsers
    {
        public static readonly string[] Contributor = new[] { AuthClaims.ReadLogEntires, AuthClaims.WriteLogEntires };
    }


    public static class Auth
    {
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
            return tokenValidator.ValidateToken(accessToken, validationParameters, out SecurityToken securityToken);
        }

        public static async Task<User> AuthenticateRequest(HttpRequest req, IEnumerable<string> requiredClaims)
        {
            var accessToken = GetAccessToken(req);
            var claimsPrincipal = await ValidateAccessToken(accessToken);

            var scopes = (claimsPrincipal.Claims
                .Where(c => c.Type == "http://schemas.microsoft.com/identity/claims/scope" || c.Type == "scp")
                .FirstOrDefault()?.Value.Split(" ") ?? Array.Empty<string>()).ToHashSet();

            var missing = requiredClaims.Where(c => !scopes.Contains(c));

            if (missing.Any())
            {
                throw new Exception($"Missing the following claim(s): {string.Join(" ", missing)}");
            }

            var nameIdentifier = claimsPrincipal.Claims.SingleOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrWhiteSpace(nameIdentifier))
            {
                throw new Exception("invalid user");
            }

            string userName = claimsPrincipal.Claims.SingleOrDefault(c => c.Type == ClaimTypes.Name || c.Type == "name")?.Value;

            if (string.IsNullOrWhiteSpace(userName))
            {
                throw new Exception("invalid user name");
            }

            return new User
            { 
                Id = nameIdentifier,
                Name = userName,
                PreferredUserName = claimsPrincipal.Claims.SingleOrDefault(c => c.Type == "preferred_username").Value,
            };
        }
    }
}

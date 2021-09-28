using Client.State;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;

namespace Client
{
    public class CustomAuthorizationMessageHandler : AuthorizationMessageHandler
    {
        public CustomAuthorizationMessageHandler(IAccessTokenProvider provider,
            NavigationManager navigationManager,
            StateContainer state)
            : base(provider, navigationManager)
        {
            ConfigureHandler(
                authorizedUrls: new[] { state.Api.BaseAddress.ToString() },
                scopes: new[] { "https://lunarcommand.onmicrosoft.com/cnc/LogEntries.Read", "https://lunarcommand.onmicrosoft.com/cnc/LogEntries.Write" });
        }
    }
}

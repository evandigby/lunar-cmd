using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;

namespace Client
{
    public class CustomAuthorizationMessageHandler : AuthorizationMessageHandler
    {
        public CustomAuthorizationMessageHandler(IAccessTokenProvider provider,
            NavigationManager navigationManager,
            State state)
            : base(provider, navigationManager)
        {
            ConfigureHandler(
                authorizedUrls: new[] { state.BaseAddress.ToString() },
                scopes: new[] { "https://lunarcommand.onmicrosoft.com/cnc/LogEntries.Read", "https://lunarcommand.onmicrosoft.com/cnc/LogEntries.Write" });
        }
    }
}

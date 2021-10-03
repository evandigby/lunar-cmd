using Client;
using Client.RealTimeCommunication;
using Client.State;
using LunarAPIClient;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Net.Http;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");

var baseAddress = new Uri(builder.Configuration["API_Prefix"] ?? builder.HostEnvironment.BaseAddress);

builder.Services.AddState(
    new StateContainer(
        baseAddress, 
        builder.Configuration["API_Version"] ?? "v1.0", 
        builder.Configuration["SignalRHubName"] ?? "commands"));

builder.Services.AddMsalAuthentication(options =>
{
    builder.Configuration.Bind("AzureAdB2C", options.ProviderOptions.Authentication);
    options.ProviderOptions.LoginMode = "redirect";
    options.ProviderOptions.DefaultAccessTokenScopes.Add("openid");
    options.ProviderOptions.DefaultAccessTokenScopes.Add("offline_access");
    options.ProviderOptions.DefaultAccessTokenScopes.Add("https://lunarcommand.onmicrosoft.com/cnc/LogEntries.Read");
    options.ProviderOptions.DefaultAccessTokenScopes.Add("https://lunarcommand.onmicrosoft.com/cnc/LogEntries.Write");
});

builder.Services.AddScoped<CustomAuthorizationMessageHandler>();

builder.Services.AddHttpClient("WebAPI",
        client => client.BaseAddress = baseAddress)
    .AddHttpMessageHandler<CustomAuthorizationMessageHandler>();

builder.Services.AddTransient(sp => sp.GetRequiredService<IHttpClientFactory>().CreateClient("WebAPI"));

builder.Services.AddTransient<ILogEntryClient, LogEntryClient>();
builder.Services.AddTransient<ICommandClient, CommandClient>();
builder.Services.AddTransient<IAudioCommandClient, AudioCommandClient>();
builder.Services.AddTransient<IHubConnectionFactory, HubConnectionFactory>();
builder.Services.AddTransient<IUserClient, UserClient>();

await builder.Build().RunAsync();

using Client.State;
using Data.Converters;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;

namespace Client.RealTimeCommunication
{
    public class HubConnectionFactory : IHubConnectionFactory
    {
        private readonly Api apiState;

        public HubConnectionFactory(Api apiState)
        {
            this.apiState = apiState;
        }

        public async Task<HubConnection> ConnectHub<T>(string commandName, Action<T> onReceive)
        {
            var hubConnection = new HubConnectionBuilder()
                .WithUrl($"{apiState.BaseAddress}api/{apiState.ApiVersion}/")
                .AddJsonProtocol(options =>
                {
                    options.PayloadSerializerOptions = ConverterOptions.JsonSerializerOptions;
                })
                .Build();

            hubConnection.On<T>(commandName, (item) => onReceive(item));

            await hubConnection.StartAsync();

            return hubConnection;
        }
    }
}

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
        private StateContainer _state;

        public HubConnectionFactory(StateContainer state)
        {
            _state = state;
        }

        public async Task<HubConnection> ConnectHub<T>(string commandName, Action<StateContainer, T> onReceive)
        {
            var hubConnection = new HubConnectionBuilder()
                .WithUrl($"{_state.BaseAddress}api/")
                .AddJsonProtocol(options =>
                {
                    options.PayloadSerializerOptions = ConverterOptions.JsonSerializerOptions;
                })
                .Build();

            hubConnection.On<T>(commandName, (item) => onReceive(_state, item));

            await hubConnection.StartAsync();

            return hubConnection;
        }
    }
}

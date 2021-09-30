using Client.State;
using Data.Converters;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
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

        public async Task<HubConnection> ConnectHub<T>(IEnumerable<HubCommand<T>> hubCommands)
        {
            var hubConnection = new HubConnectionBuilder()
                .WithUrl($"{apiState.BaseAddress}api/{apiState.ApiVersion}/")
                .AddJsonProtocol(options =>
                {
                    options.PayloadSerializerOptions = ConverterOptions.JsonSerializerOptions;
                })
                .Build();

            if (hubConnection == null)
                throw new Exception("could not create hub connection");

            foreach (var command in hubCommands)
            {
                if (command.CommandName == null)
                    throw new Exception("command must have name");

                if (command.OnReceive == null)
                    throw new Exception("command must have onreceive");

                hubConnection.On<T>(command.CommandName, (item) => command.OnReceive(item));
            }

            await hubConnection.StartAsync();

            return hubConnection;
        }
    }
}

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

        public async Task<HubConnection> ConnectHub(Action<HubConnection> hubConfigFunc)
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

            hubConfigFunc(hubConnection);

            await hubConnection.StartAsync();

            return hubConnection;
        }
    }
}

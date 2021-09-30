using Client.State;
using Microsoft.AspNetCore.SignalR.Client;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Client.RealTimeCommunication
{
    public interface IHubConnectionFactory
    {
        Task<HubConnection> ConnectHub<T>(IEnumerable<HubCommand<T>> hubCommands);
    }
}

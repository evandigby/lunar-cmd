using Client.State;
using Microsoft.AspNetCore.SignalR.Client;
using System;
using System.Threading.Tasks;

namespace Client.RealTimeCommunication
{
    public interface IHubConnectionFactory
    {
        Task<HubConnection> ConnectHub<T>(string commandName, Action<StateContainer, T> onReceive);
    }
}

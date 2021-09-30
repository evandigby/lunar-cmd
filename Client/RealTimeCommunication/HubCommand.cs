using System;

namespace Client.RealTimeCommunication
{
    public class HubCommand<T>
    {
        public string? CommandName { get; set; }
        public Action<T>? OnReceive {  get; set; }
    }
}

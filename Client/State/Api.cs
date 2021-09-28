using System;

namespace Client.State
{
    public class Api : StateObject
    {
        public Api(Uri baseAddress, string apiVersion, string hubName)
        {
            BaseAddress = baseAddress;
            ApiVersion = apiVersion;
            HubName = hubName;
        }

        public Uri BaseAddress { get; }

        public string ApiVersion { get; }

        public string HubName { get; }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LunarAPIClient
{
    public abstract class ApiClient
    {
        protected abstract string ApiVersion { get; }
        protected abstract string ApiEndpoint { get; }
        protected readonly HttpClient httpClient;

        public ApiClient(HttpClient client)
        {
            httpClient = client;
        }

        public Uri BaseAddress => httpClient.BaseAddress ?? new Uri("https://www.bing.com"); // Should never happen
    }
}

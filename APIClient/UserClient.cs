using Data.Users;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;

namespace LunarAPIClient
{
    public class UserClient : ApiClient, IUserClient
    {
        public UserClient(HttpClient client) : base(client)
        {
        }

        protected override string ApiVersion => "v1.0";

        protected override string ApiEndpoint => $"/api/{ApiVersion}/users";

        public async Task<User> GetUser(string id, CancellationToken cancellationToken)
        {
            return await httpClient.GetFromJsonAsync<User>($"{ApiEndpoint}/{id}", cancellationToken) ?? throw new Exception($"user {id} not found");
        }

        public Task<User> Me(CancellationToken cancellationToken)
        {
            return GetUser("me", cancellationToken);
        }
    }
}

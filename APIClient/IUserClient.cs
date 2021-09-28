using Data.Users;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LunarAPIClient
{
    public interface IUserClient
    {
        public Task<User> GetUser(string id, CancellationToken cancellationToken);
        public Task<User> Me(CancellationToken cancellationToken);
    }
}

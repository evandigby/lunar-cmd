using Data.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LunarAPIClient
{
    public interface IAudioCommandClient : ICommandClient
    {
        public Uri GetEndpoint();
    }
}

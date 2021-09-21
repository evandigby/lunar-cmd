using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace api
{
    // from https://codeopinion.com/lazy-async/
    public class LazyAsync<T> : Lazy<Task<T>>
    {
        public LazyAsync(Func<Task<T>> taskFactory) : base(
            () => Task.Factory.StartNew(taskFactory).Unwrap())
        { }
    }
}

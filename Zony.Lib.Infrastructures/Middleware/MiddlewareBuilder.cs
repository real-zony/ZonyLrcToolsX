using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Zony.Lib.Infrastructures.Middleware
{
    public class MiddlewareBuilder : IMiddlewareBudiler
    {
        private readonly IList<Func<RequestDelegate, RequestDelegate>> _middlewares = new List<Func<RequestDelegate, RequestDelegate>>();

        public RequestDelegate Build()
        {
            RequestDelegate startDelegate = lyric => Task.CompletedTask;

            foreach (var middle in _middlewares.Reverse())
            {
                startDelegate = middle(startDelegate);
            }

            return startDelegate;
        }

        public IMiddlewareBudiler RegisterMiddleware(Func<RequestDelegate, RequestDelegate> @delegate)
        {
            _middlewares.Add(@delegate);
            return this;
        }
    }
}

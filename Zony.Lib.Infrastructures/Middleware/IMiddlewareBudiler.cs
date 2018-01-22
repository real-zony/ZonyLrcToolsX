using System;
using System.Text;
using System.Threading.Tasks;
using Zony.Lib.Infrastructures.Dependency;

namespace Zony.Lib.Infrastructures.Middleware
{
    public delegate Task RequestDelegate(StringBuilder lyric);
    public interface IMiddlewareBudiler : ISingletonDependency
    {
        /// <summary>
        /// 构建请求管道
        /// </summary>
        /// <returns>完整管道代理</returns>
        RequestDelegate Build();

        /// <summary>
        /// 注册请求
        /// </summary>
        IMiddlewareBudiler RegisterMiddleware(Func<RequestDelegate, RequestDelegate> @delegate);
    }
}

using System;

namespace Zony.Lib.Infrastructures.Common.Exceptions
{
    /// <summary>
    /// 代理异常
    /// </summary>
    public class ProxyException : Exception
    {
        public ProxyException(string message) : base(message)
        {

        }
    }
}

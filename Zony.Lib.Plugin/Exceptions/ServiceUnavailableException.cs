using System;

namespace Zony.Lib.Plugin.Exceptions
{
    /// <summary>
    /// 服务不可用异常，为目标 API 限制导致
    /// </summary>
    public class ServiceUnavailableException : Exception
    {
        /// <summary>
        /// 服务不可用异常，为目标 API 限制导致
        /// </summary>
        /// <param name="message">异常信息</param>
        public ServiceUnavailableException(string message) : base(message)
        {
        }
    }
}

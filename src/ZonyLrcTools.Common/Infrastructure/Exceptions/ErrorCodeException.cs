namespace ZonyLrcTools.Common.Infrastructure.Exceptions
{
    /// <summary>
    /// 带错误码的异常实现。
    /// </summary>
    public class ErrorCodeException : Exception
    {
        public int ErrorCode { get; }

        public object? AttachObject { get; }

        /// <summary>
        /// 构建一个新的 <see cref="ErrorCodeException"/> 对象。
        /// </summary>
        /// <param name="errorCode">错误码，参考 <see cref="ErrorCodes"/> 类的定义。</param>
        /// <param name="message">错误信息。</param>
        /// <param name="attachObj">附加的对象数据。</param>
        public ErrorCodeException(int errorCode, string? message = null, object? attachObj = null) : base(message)
        {
            ErrorCode = errorCode;
            AttachObject = attachObj;
        }
    }
}
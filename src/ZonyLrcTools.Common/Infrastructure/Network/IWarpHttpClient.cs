namespace ZonyLrcTools.Common.Infrastructure.Network
{
    /// <summary>
    /// 基于 <see cref="IHttpClientFactory"/> 封装的 HTTP 请求客户端。
    /// </summary>
    public interface IWarpHttpClient
    {
        /// <summary>
        /// 根据指定的配置执行 POST 请求，并以 <see cref="string"/> 作为返回值。
        /// </summary>
        /// <param name="url">请求的 URL 地址。</param>
        /// <param name="parameters">请求的参数。</param>
        /// <param name="isQueryStringParam">是否以 QueryString 形式携带参数。</param>
        /// <param name="requestOption">请求时的配置动作。</param>
        /// <returns>服务端的响应结果。</returns>
        ValueTask<string> PostAsync(string url,
            object? parameters = null,
            bool isQueryStringParam = false,
            Action<HttpRequestMessage>? requestOption = null);

        /// <summary>
        /// 根据指定的配置执行 POST 请求，并将结果反序列化为 <see cref="TResponse"/> 对象。
        /// </summary>
        /// <param name="url">请求的 URL 地址。</param>
        /// <param name="parameters">请求的参数。</param>
        /// <param name="isQueryStringParam">是否以 QueryString 形式携带参数。</param>
        /// <param name="requestOption">请求时的配置动作。</param>
        /// <typeparam name="TResponse">需要将响应结果反序列化的目标类型。</typeparam>
        /// <returns>服务端的响应结果。</returns>
        ValueTask<TResponse> PostAsync<TResponse>(string url,
            object? parameters = null,
            bool isQueryStringParam = false,
            Action<HttpRequestMessage>? requestOption = null);

        ValueTask<HttpResponseMessage> PostReturnHttpResponseAsync(string url,
            object? parameters = null,
            bool isQueryStringParam = false,
            Action<HttpRequestMessage>? requestOption = null);

        /// <summary>
        /// 根据指定的配置执行 GET 请求，并以 <see cref="string"/> 作为返回值。
        /// </summary>
        /// <param name="url">请求的 URL 地址。</param>
        /// <param name="parameters">请求的参数。</param>
        /// <param name="requestOption">请求时的配置动作。</param>
        /// <returns>服务端的响应结果。</returns>
        ValueTask<string> GetAsync(string url,
            object? parameters = null,
            Action<HttpRequestMessage>? requestOption = null);

        /// <summary>
        /// 根据指定的配置执行 GET 请求，并将结果反序列化为 <see cref="TResponse"/> 对象。
        /// </summary>
        /// <param name="url">请求的 URL 地址。</param>
        /// <param name="parameters">请求的参数。</param>
        /// <param name="requestOption">请求时的配置动作。</param>
        /// <typeparam name="TResponse">需要将响应结果反序列化的目标类型。</typeparam>
        /// <returns>服务端的响应结果。</returns>
        ValueTask<TResponse> GetAsync<TResponse>(
            string url,
            object? parameters = null,
            Action<HttpRequestMessage>? requestOption = null);
    }
}
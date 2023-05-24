using System.Net;
using System.Reflection;
using System.Text;
using Newtonsoft.Json;
using ZonyLrcTools.Common.Infrastructure.DependencyInject;
using ZonyLrcTools.Common.Infrastructure.Exceptions;

namespace ZonyLrcTools.Common.Infrastructure.Network
{
    public class DefaultWarpHttpClient : IWarpHttpClient, ITransientDependency
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public const string HttpClientNameConstant = "WarpClient";

        public DefaultWarpHttpClient(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public async ValueTask<string> PostAsync(string url,
            object? parameters = null,
            bool isQueryStringParam = false,
            Action<HttpRequestMessage>? requestOption = null)
        {
            using var responseMessage = await PostReturnHttpResponseAsync(url, parameters, isQueryStringParam, requestOption);
            var responseContentString = await responseMessage.Content.ReadAsStringAsync();

            return ValidateHttpResponse(responseMessage, parameters, responseContentString);
        }

        public async ValueTask<TResponse> PostAsync<TResponse>(string url,
            object? parameters = null,
            bool isQueryStringParam = false,
            Action<HttpRequestMessage>? requestOption = null)
        {
            var responseString = await PostAsync(url, parameters, isQueryStringParam, requestOption);
            return ConvertHttpResponseToObject<TResponse>(parameters, responseString);
        }

        public async ValueTask<HttpResponseMessage> PostReturnHttpResponseAsync(string url,
            object? parameters = null,
            bool isQueryStringParam = false,
            Action<HttpRequestMessage>? requestOption = null)
        {
            var parametersStr = isQueryStringParam ? BuildQueryString(parameters) : BuildJsonBodyString(parameters);
            var requestMessage = new HttpRequestMessage(HttpMethod.Post, new Uri(url));
            requestMessage.Content = new StringContent(parametersStr);

            requestOption?.Invoke(requestMessage);

            return await BuildHttpClient().SendAsync(requestMessage);
        }

        public async ValueTask<string> GetAsync(string url,
            object? parameters = null,
            Action<HttpRequestMessage>? requestOption = null)
        {
            var requestParamsStr = BuildQueryString(parameters);
            var requestMsg = new HttpRequestMessage(HttpMethod.Get, new Uri($"{url}?{requestParamsStr}"));
            requestOption?.Invoke(requestMsg);

            using var responseMessage = await BuildHttpClient().SendAsync(requestMsg);
            var responseContentString = await responseMessage.Content.ReadAsStringAsync();

            return ValidateHttpResponse(responseMessage, parameters, responseContentString);
        }

        public async ValueTask<TResponse> GetAsync<TResponse>(string url,
            object? parameters = null,
            Action<HttpRequestMessage>? requestOption = null)
        {
            var responseString = await GetAsync(url, parameters, requestOption);
            return ConvertHttpResponseToObject<TResponse>(parameters, responseString);
        }

        protected virtual HttpClient BuildHttpClient()
        {
            return _httpClientFactory.CreateClient(HttpClientNameConstant);
        }

        private string BuildQueryString(object? parameters)
        {
            if (parameters == null)
            {
                return string.Empty;
            }

            var type = parameters.GetType();
            if (type == typeof(string))
            {
                return parameters as string ?? string.Empty;
            }

            var properties = type.GetProperties();
            var paramBuilder = new StringBuilder();

            foreach (var propertyInfo in properties)
            {
                var jsonProperty = propertyInfo.GetCustomAttribute<JsonPropertyAttribute>();
                var propertyName = jsonProperty != null ? jsonProperty.PropertyName : propertyInfo.Name;

                paramBuilder.Append($"{propertyName}={propertyInfo.GetValue(parameters)}&");
            }

            return paramBuilder.ToString().TrimEnd('&');
        }

        private string BuildJsonBodyString(object? parameters)
        {
            if (parameters == null) return string.Empty;
            if (parameters is string result) return result;

            return JsonConvert.SerializeObject(parameters);
        }

        /// <summary>
        /// 校验 Http 响应 <see cref="HttpResponseMessage"/> 是否合法。
        /// </summary>
        /// <param name="responseMessage">执行 Http 请求之后的响应实例。</param>
        /// <param name="requestParameters">执行 Http 请求时传递的参数。</param>
        /// <param name="responseString">执行 Http 请求之后响应内容。</param>
        /// <returns>如果响应正常，则返回具体的响应内容。</returns>
        /// <exception cref="ErrorCodeException">如果 Http 响应不正常，则可能抛出本异常。</exception>
        private string ValidateHttpResponse(HttpResponseMessage responseMessage, object? requestParameters, string responseString)
        {
            return responseMessage.StatusCode switch
            {
                HttpStatusCode.OK => responseString,
                HttpStatusCode.ServiceUnavailable => throw new ErrorCodeException(ErrorCodes.ServiceUnavailable),
                _ => throw new ErrorCodeException(ErrorCodes.HttpRequestFailed, attachObj: new { requestParameters, responseString })
            };
        }

        /// <summary>
        /// 将 Http 响应的字符串反序列化为指定类型 <see cref="TResponse"/> 的对象。
        /// </summary>
        /// <param name="requestParameters">执行 Http 请求时传递的参数。</param>
        /// <param name="responseString">执行 Http 请求之后响应内容。</param>
        /// <typeparam name="TResponse">需要将响应结果反序列化的目标类型。</typeparam>
        /// <exception cref="ErrorCodeException">如果反序列化失败，则可能抛出本异常。</exception>
        private TResponse ConvertHttpResponseToObject<TResponse>(object? requestParameters, string responseString)
        {
            var throwException = new ErrorCodeException(ErrorCodes.HttpResponseConvertJsonFailed, attachObj: new { requestParameters, responseString });

            try
            {
                var responseObj = JsonConvert.DeserializeObject<TResponse>(responseString);
                if (responseObj != null) return responseObj;

                throw throwException;
            }
            catch (JsonSerializationException)
            {
                throw throwException;
            }
        }
    }
}
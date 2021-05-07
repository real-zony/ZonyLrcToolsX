using System;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using ZonyLrcTools.Cli.Infrastructure.DependencyInject;
using ZonyLrcTools.Cli.Infrastructure.Exceptions;

namespace ZonyLrcTools.Cli.Infrastructure.Network
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
            object parameters = null,
            bool isQueryStringParam = false,
            Action<HttpRequestMessage> requestOption = null)
        {
            var parametersStr = isQueryStringParam ? BuildQueryString(parameters) : BuildJsonBodyString(parameters);
            var requestMessage = new HttpRequestMessage(HttpMethod.Post, new Uri(url));
            requestMessage.Content = new StringContent(parametersStr);

            requestOption?.Invoke(requestMessage);

            using var responseMessage = await BuildHttpClient().SendAsync(requestMessage);
            var responseContentString = await responseMessage.Content.ReadAsStringAsync();

            return responseMessage.StatusCode switch
            {
                HttpStatusCode.OK => responseContentString,
                HttpStatusCode.ServiceUnavailable => throw new ErrorCodeException(ErrorCodes.ServiceUnavailable),
                _ => throw new ErrorCodeException(ErrorCodes.ServiceUnavailable, attachObj: new {parametersStr, responseContentString})
            };
        }

        public async ValueTask<TResponse> PostAsync<TResponse>(string url,
            object parameters = null,
            bool isQueryStringParam = false,
            Action<HttpRequestMessage> requestOption = null)
        {
            var responseString = await PostAsync(url, parameters, isQueryStringParam, requestOption);
            var throwException = new ErrorCodeException(ErrorCodes.HttpResponseConvertJsonFailed, attachObj: new {parameters, responseString});

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

        public async ValueTask<string> GetAsync(string url,
            object parameters = null,
            Action<HttpRequestMessage> requestOption = null)
        {
            var requestParamsStr = BuildQueryString(parameters);
            var requestMsg = new HttpRequestMessage(HttpMethod.Get, new Uri($"{url}?{requestParamsStr}"));
            requestOption?.Invoke(requestMsg);

            using (var responseMsg = await BuildHttpClient().SendAsync(requestMsg))
            {
                var responseContent = await responseMsg.Content.ReadAsStringAsync();

                return responseMsg.StatusCode switch
                {
                    HttpStatusCode.OK => responseContent,
                    HttpStatusCode.ServiceUnavailable => throw new ErrorCodeException(ErrorCodes.ServiceUnavailable),
                    _ => throw new ErrorCodeException(ErrorCodes.ServiceUnavailable, attachObj: new {requestParamsStr, responseContent})
                };
            }
        }

        public async ValueTask<TResponse> GetAsync<TResponse>(string url,
            object parameters = null,
            Action<HttpRequestMessage> requestOption = null)
        {
            var responseStr = await GetAsync(url, parameters, requestOption);
            var throwException = new ErrorCodeException(ErrorCodes.HttpResponseConvertJsonFailed, attachObj: new {parameters, responseStr});
            try
            {
                var responseObj = JsonConvert.DeserializeObject<TResponse>(responseStr);
                if (responseObj != null) return responseObj;

                throw throwException;
            }
            catch (JsonSerializationException)
            {
                throw throwException;
            }
        }

        protected virtual HttpClient BuildHttpClient()
        {
            return _httpClientFactory.CreateClient(HttpClientNameConstant);
        }

        private string BuildQueryString(object parameters)
        {
            if (parameters == null)
            {
                return string.Empty;
            }

            var type = parameters.GetType();
            if (type == typeof(string))
            {
                return parameters as string;
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

        private string BuildJsonBodyString(object parameters)
        {
            if (parameters == null) return string.Empty;
            if (parameters is string result) return result;

            return JsonConvert.SerializeObject(parameters);
        }
    }
}
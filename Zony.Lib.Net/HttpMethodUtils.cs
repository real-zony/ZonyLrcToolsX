using Newtonsoft.Json;
using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Zony.Lib.Infrastructures.Common.Exceptions;
using Zony.Lib.Infrastructures.Common.Interfaces;
using Zony.Lib.Infrastructures.Dependency;

namespace Zony.Lib.Net
{
    public class HttpMethodUtils : IDisposable
    {
        private readonly HttpClient _client;

        public HttpMethodUtils()
        {
            var config = IocManager.Instance.Resolve<IConfigurationManager>().ConfigModel;

            if (config != null && !string.IsNullOrEmpty(config.ProxyIP) && config.ProxyPort != 0)
            {
                _client = new HttpClient(new HttpClientHandler
                {
                    Proxy =  new WebProxy(config.ProxyIP,config.ProxyPort)
                });

                return;
            }

            _client = new HttpClient();
        }

        /// <summary>
        /// 对目标URL进行HTTP-GET请求
        /// </summary>
        /// <param name="url">目标URL</param>
        /// <param name="parameters">要提交的参数</param>
        /// <param name="referer">来源地址</param>
        /// <returns>服务器响应结果</returns>
        public string Get(string url, object parameters = null, string referer = null)
        {
            try
            {
                var req = new HttpRequestMessage()
                {
                    Method = HttpMethod.Get,
                    RequestUri = new Uri($"{url}{BaseFormBuildParameters(parameters)}")
                };

                if (referer != null) req.Headers.Referrer = new Uri(referer);

                using (var msg = _client.SendAsync(req).GetAwaiter().GetResult())
                {
                    if (msg.StatusCode != HttpStatusCode.OK) return string.Empty;
                    return msg.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                }
            }
            catch (Exception)
            {
                throw new ProxyException("执行 HTTP 请求时出现异常.");
            }
        }

        /// <summary>
        /// 对目标 URL 进行异步 HTTP-GET 请求
        /// </summary>
        /// <param name="url">目标URL</param>
        /// <param name="parameters">要提交的参数</param>
        /// <param name="referer">来源地址</param>
        /// <returns>服务器响应结果</returns>
        /// <returns></returns>
        public async Task<TJsonModel> GetAsync<TJsonModel>(string url, object parameters = null, string referer = null)
        {
            try
            {
                var request = new HttpRequestMessage
                {
                    Method = HttpMethod.Get,
                    RequestUri = new Uri($"{url}{BaseFormBuildParameters(parameters)}")
                };

                if (referer != null) request.Headers.Referrer = new Uri(referer);
                using (var msg = await _client.SendAsync(request))
                {
                    if (msg.StatusCode != HttpStatusCode.OK) return default(TJsonModel);

                    string jsonStr = await msg.Content.ReadAsStringAsync();
                    return JsonConvert.DeserializeObject<TJsonModel>(jsonStr);
                }
            }
            catch (Exception)
            {
                throw new ProxyException("执行 HTTP 请求时出现异常.");
            }
        }

        /// <summary>
        /// 对目标URL进行HTTP-POST请求
        /// </summary>
        /// <param name="url">目标URL</param>
        /// <param name="parameters">待提交的参数</param>
        /// <param name="referer">来源地址</param>
        /// <param name="mediaTypeValue">提交的内容类型</param>
        /// <returns>服务器响应结果</returns>
        public string Post(string url, object parameters = null, string referer = null, string mediaTypeValue = null)
        {
            try
            {
                string postData;

                var req = new HttpRequestMessage
                {
                    Method = HttpMethod.Post,
                    RequestUri = new Uri(url)
                };

                if (referer != null) req.Headers.Referrer = new Uri(referer);
                // 请求内容构造
                if (mediaTypeValue == "application/json") postData = BaseJsonBuildParameters(parameters);
                else postData = BaseFormBuildParameters(parameters);
                req.Content = new StringContent(postData);
                if (mediaTypeValue != null) req.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(mediaTypeValue);


                using (var res = _client.SendAsync(req).GetAwaiter().GetResult())
                {
                    if (res.StatusCode != HttpStatusCode.OK) return string.Empty;
                    return res.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                }
            }
            catch (Exception)
            {
                throw new ProxyException("执行 HTTP 请求时出现异常.");
            }
        }

        /// <summary>
        /// 对目标URL进行HTTP-GET请求，并对结果进行序列化操作
        /// </summary>
        /// <param name="url">目标URL</param>
        /// <param name="parameters">要提交的参数</param>
        /// <param name="referer">来源地址</param>
        /// <returns>成功序列化的对象</returns>
        public TJsonModel Get<TJsonModel>(string url, object parameters = null, string referer = null)
        {
            return JsonConvert.DeserializeObject<TJsonModel>(Get(url, parameters, referer));
        }

        /// <summary>
        /// 对目标URL进行HTTP-POST请求，并对结果进行序列化操作
        /// </summary>
        /// <param name="url">目标URL</param>
        /// <param name="parameters">待提交的参数</param>
        /// <param name="referer">来源地址</param>
        /// <param name="mediaTypeValue">提交的内容类型</param>
        /// <returns>成功序列化的对象</returns>
        public TJsonModel Post<TJsonModel>(string url, object parameters = null, string referer = null, string mediaTypeValue = null)
        {
            return JsonConvert.DeserializeObject<TJsonModel>(Post(url, parameters, referer, mediaTypeValue));
        }

        /// <summary>
        /// 对目标 URL 进行异步 HTTP-POST 请求，并对结果进行序列化操作
        /// </summary>
        /// <param name="url">目标URL</param>
        /// <param name="parameters">待提交的参数</param>
        /// <param name="referer">来源地址</param>
        /// <param name="mediaTypeValue">提交的内容类型</param>
        /// <returns>成功序列化的对象</returns>
        public async Task<TJsonModel> PostAsync<TJsonModel>(string url, object parameters = null, string referer = null, string mediaTypeValue = null)
        {
            string postData;
            var request = new HttpRequestMessage()
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri(url)
            };

            if (mediaTypeValue == "application/json") postData = BaseFormBuildParameters(parameters);
            else postData = BaseJsonBuildParameters(parameters);

            if (!string.IsNullOrEmpty(postData)) request.Content = new StringContent(postData);
            if (!string.IsNullOrEmpty(referer)) request.Headers.Referrer = new Uri(referer);
            if (!string.IsNullOrEmpty(mediaTypeValue)) request.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(mediaTypeValue);

            using (var response = await _client.SendAsync(request))
            {
                if (response.StatusCode != HttpStatusCode.OK) return default(TJsonModel);
                return JsonConvert.DeserializeObject<TJsonModel>(await response.Content.ReadAsStringAsync());
            }
        }

        /// <summary>
        /// 构建URL编码字符串
        /// </summary>
        /// <param name="srcText">待编码的字符串</param>
        /// <param name="encoding">编码方式</param>
        [Obsolete("这个方法会导致将英文编码，会产生歌曲无法正常搜索的情况!")]
        public string URL_Encoding_Old(string srcText, Encoding encoding)
        {
            StringBuilder builder = new StringBuilder();
            byte[] bytes = encoding.GetBytes(srcText);

            foreach (var _byte in bytes)
            {
                builder.Append('%').Append(_byte.ToString("x2"));
            }

            return builder.ToString().TrimEnd('%');
        }

        /// <summary>
        /// 构建URL编码字符串
        /// </summary>
        /// <param name="srcText">待编码的字符串</param>
        /// <param name="encoding">编码方式</param>
        public string URL_Encoding(string srcText, Encoding encoding)
        {
            return HttpUtility.UrlEncode(srcText, encoding);
        }

        /// <summary>
        /// 基于 QueryString 形式构建查询参数
        /// </summary>
        /// <param name="parameters">参数对象</param>
        private string BaseFormBuildParameters(object parameters)
        {
            if (parameters == null) return string.Empty;
            var type = parameters.GetType();
            if (type == typeof(string)) return parameters as string;

            var properties = type.GetProperties();
            StringBuilder paramBuidler = new StringBuilder("?");

            // 反射构建参数
            foreach (var property in properties)
            {
                paramBuidler.Append($"{property.Name}={property.GetValue(parameters)}&");
            }
            return paramBuidler.ToString().Trim('&');
        }

        /// <summary>
        /// 基于 JSON 形式构建查询参数
        /// </summary>
        /// <param name="parameters">参数对象</param>
        private string BaseJsonBuildParameters(object parameters)
        {
            if (parameters == null) return string.Empty;
            if (parameters is string s) return s;
            return JsonConvert.SerializeObject(parameters);
        }

        public void Dispose()
        {
            _client.Dispose();
        }
    }
}
using Newtonsoft.Json;
using System;
using System.Net;
using System.Net.Http;
using System.Text;

namespace Zony.Lib.Net
{
    public class HttpMethodUtils
    {
        private readonly HttpClient m_client;

        public HttpMethodUtils() => m_client = new HttpClient();

        /// <summary>
        /// 对目标URL进行HTTP-GET请求
        /// </summary>
        /// <param name="url">目标URL</param>
        /// <param name="parameters">要提交的参数</param>
        /// <param name="referer">来源地址</param>
        /// <returns>服务器响应结果</returns>
        public string Get(string url, object parameters = null, string referer = null)
        {
            var _req = new HttpRequestMessage()
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri($"{url}{parametersBuildForm(parameters)}")
            };

            if (referer != null) _req.Headers.Referrer = new Uri(referer);

            using (var _msg = m_client.SendAsync(_req).Result)
            {
                if (_msg.StatusCode != HttpStatusCode.OK) return string.Empty;
                return _msg.Content.ReadAsStringAsync().Result;
            };
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
            string _postData = string.Empty;

            var _req = new HttpRequestMessage()
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri(url)
            };

            if (referer != null) _req.Headers.Referrer = new Uri(referer);
            // 请求内容构造
            if (mediaTypeValue == "application/json") _postData = parametersBuildJson(parameters);
            else _postData = parametersBuildForm(parameters);
            _req.Content = new StringContent(_postData);
            if (mediaTypeValue != null) _req.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(mediaTypeValue);


            using (var _res = m_client.SendAsync(_req).Result)
            {
                if (_res.StatusCode != HttpStatusCode.OK) return string.Empty;
                return _res.Content.ReadAsStringAsync().Result;
            }
        }

        public string Post(string url, string parameters = null, string referer = null, string mediaTypeValue = null)
        {
            var _req = new HttpRequestMessage()
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri(parameters)
            };

            if (referer != null) _req.Headers.Referrer = new Uri(referer);
            if (parameters != null)
            {
                _req.Content = new StringContent(parameters);
                _req.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(mediaTypeValue);
            }

            using (var _res = m_client.SendAsync(_req).Result)
            {
                if (_res.StatusCode != HttpStatusCode.OK) return string.Empty;
                return _res.Content.ReadAsStringAsync().Result;
            }
        }

        /// <summary>
        /// 构建URL编码字符串
        /// </summary>
        /// <param name="srcText">待编码的字符串</param>
        /// <param name="encoding">编码方式</param>
        public string URL_Encoding(string srcText, Encoding encoding)
        {
            StringBuilder _builder = new StringBuilder();
            byte[] _bytes = encoding.GetBytes(srcText);

            foreach (var _byte in _bytes)
            {
                _builder.Append('%').Append(_byte.ToString("x2"));
            }

            return _builder.ToString().TrimEnd('%');
        }

        private string parametersBuildForm(object parameters)
        {
            if (parameters == null) return string.Empty;
            var _type = parameters.GetType();
            if (_type == typeof(string)) return parameters as string;

            var _properties = _type.GetProperties();
            StringBuilder _paramBuidler = new StringBuilder("?");

            // 反射构建参数
            foreach (var _property in _properties)
            {
                _paramBuidler.Append($"{_property.Name}={_property.GetValue(parameters)}&");
            }
            return _paramBuidler.ToString().Trim('&');
        }

        private string parametersBuildJson(object parameters)
        {
            if (parameters == null) return string.Empty;
            if (parameters.GetType() == typeof(string)) return parameters as string;
            return JsonConvert.SerializeObject(parameters);
        }
    }
}

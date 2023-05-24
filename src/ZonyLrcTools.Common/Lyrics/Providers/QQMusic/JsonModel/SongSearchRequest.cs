using System.Text;
using System.Web;
using Newtonsoft.Json;

namespace ZonyLrcTools.Common.Lyrics.Providers.QQMusic.JsonModel
{
    public class SongSearchRequest
    {
        [JsonProperty("format")]
        public string Format { get; protected set; }

        [JsonProperty("inCharset")]
        public string InCharset { get; protected set; }

        [JsonProperty("outCharset")]
        public string OutCharset { get; protected set; }

        [JsonProperty("platform")]
        public string Platform { get; protected set; }

        [JsonProperty("key")]
        public string Keyword { get; protected set; } = null!;

        protected SongSearchRequest()
        {
            Format = "json";
            InCharset = OutCharset = "utf-8";
            Platform = "yqq.json";
        }

        public SongSearchRequest(string musicName, string artistName) : this()
        {
            Keyword = HttpUtility.UrlEncode($"{musicName}+{artistName}", Encoding.UTF8);
        }
    }
}
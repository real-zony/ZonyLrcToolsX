using System.Text;
using System.Web;
using Newtonsoft.Json;

namespace ZonyLrcTools.Cli.Infrastructure.Lyric.QQMusic.JsonModel
{
    public class SongSearchRequest
    {
        [JsonProperty("remoteplace")] public string RemotePlace { get; set; }

        [JsonProperty("p")] public int Page { get; set; }

        [JsonProperty("n")] public int Limit { get; set; }

        [JsonProperty("w")] public string Keyword { get; set; }

        [JsonProperty("format")] public string ResultFormat { get; set; }

        [JsonProperty("inCharset")] public string InCharset { get; set; }

        [JsonProperty("outCharset")] public string OutCharset { get; set; }

        [JsonProperty("platform")] public string Platform { get; set; }


        protected SongSearchRequest()
        {
            RemotePlace = "txt.yqq.song";
            Page = 1;
            Limit = 5;
            ResultFormat = "json";
            InCharset = "utf8";
            OutCharset = "utf8";
            Platform = "yqq";
        }

        public SongSearchRequest(string musicName, string artistName) : this()
        {
            Keyword = HttpUtility.UrlEncode($"{musicName}+{artistName}", Encoding.UTF8);
        }
    }
}
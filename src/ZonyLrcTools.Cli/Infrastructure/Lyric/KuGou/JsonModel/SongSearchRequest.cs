using System.Text;
using System.Web;
using Newtonsoft.Json;

namespace ZonyLrcTools.Cli.Infrastructure.Lyric.KuGou.JsonModel
{
    public class SongSearchRequest
    {
        [JsonProperty("filter")]
        public int Filter { get; }

        [JsonProperty("platform")]
        public string Platform { get; }

        [JsonProperty("keyword")]
        public string Keyword { get; }

        public SongSearchRequest(string musicName, string artistName)
        {
            Filter = 2;
            Platform = "WebFilter";
            Keyword = HttpUtility.UrlEncode($"{musicName}+{artistName}", Encoding.UTF8);
        }
    }
}
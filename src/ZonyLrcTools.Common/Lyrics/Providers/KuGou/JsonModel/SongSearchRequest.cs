using System.Text;
using System.Web;
using Newtonsoft.Json;

namespace ZonyLrcTools.Common.Lyrics.Providers.KuGou.JsonModel
{
    public class SongSearchRequest
    {
        [JsonProperty("filter")] public int Filter { get; }

        [JsonProperty("platform")] public string Platform { get; }

        [JsonProperty("keyword")] public string Keyword { get; }

        [JsonProperty("pagesize")] public int PageSize { get; }

        [JsonProperty("page")] public int Page { get; }

        public SongSearchRequest(string musicName, string artistName, int pageSize = 30)
        {
            Filter = 2;
            Platform = "WebFilter";
            Keyword = HttpUtility.UrlEncode($"{musicName}+{artistName}", Encoding.UTF8);
            PageSize = pageSize;
            Page = 1;
        }
    }
}
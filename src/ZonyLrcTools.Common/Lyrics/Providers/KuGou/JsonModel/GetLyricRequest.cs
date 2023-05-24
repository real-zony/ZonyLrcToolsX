using Newtonsoft.Json;

namespace ZonyLrcTools.Common.Lyrics.Providers.KuGou.JsonModel
{
    public class GetLyricRequest
    {
        [JsonProperty("ver")] public int UnknownParameters1 { get; }

        [JsonProperty("client")] public string UnknownParameters2 { get; }

        [JsonProperty("fmt")] public string UnknownParameters3 { get; }

        [JsonProperty("charset")] public string UnknownParameters4 { get; }

        [JsonProperty("id")] public string? Id { get; }

        [JsonProperty("accesskey")] public string? AccessKey { get; }

        public GetLyricRequest(string? id, string? accessKey)
        {
            UnknownParameters1 = 1;
            UnknownParameters2 = "iphone";
            UnknownParameters3 = "lrc";
            UnknownParameters4 = "utf8";
            Id = id;
            AccessKey = accessKey;
        }
    }

    public class GetLyricResponse
    {
    }
}
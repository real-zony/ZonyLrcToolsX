using Newtonsoft.Json;

namespace ZonyLrcTools.Common.Lyrics.Providers.KuGou.JsonModel
{
    public class SongSearchResponse
    {
        [JsonProperty("status")] public int Status { get; set; }

        [JsonProperty("data")] public SongSearchResponseInnerData? Data { get; set; }

        [JsonProperty("error_code")] public int ErrorCode { get; set; }

        [JsonProperty("error_msg")] public string? ErrorMessage { get; set; }
    }

    public class SongSearchResponseInnerData
    {
        [JsonProperty("lists")] public List<SongSearchResponseSongDetail>? List { get; set; }
    }

    public class SongSearchResponseSongDetail
    {
        public string? FileHash { get; set; }
    }
}
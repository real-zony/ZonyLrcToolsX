using Newtonsoft.Json;

namespace ZonyLrcTools.Common.Lyrics.Providers.KuWo.JsonModel;

public class GetLyricsResponse
{
    [JsonProperty("status")] public int Status { get; set; }

    [JsonProperty("data")] public GetLyricsResponseInnerData? Data { get; set; }

    [JsonProperty("msg")] public string? ErrorMessage { get; set; }

    [JsonProperty("msgs")] public string? ErrorMessage2 { get; set; }
}

public class GetLyricsResponseInnerData
{
    [JsonProperty("lrclist")] public ICollection<GetLyricsItem>? Lyrics { get; set; }
}

public class GetLyricsItem
{
    [JsonProperty("lineLyric")] public string? Text { get; set; }

    [JsonProperty("time")] public string Position { get; set; } = null!;
}
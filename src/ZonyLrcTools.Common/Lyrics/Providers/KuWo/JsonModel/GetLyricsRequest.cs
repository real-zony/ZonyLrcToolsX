using Newtonsoft.Json;

namespace ZonyLrcTools.Common.Lyrics.Providers.KuWo.JsonModel;

public class GetLyricsRequest
{
    [JsonProperty("musicId")] public long MusicId { get; }

    public GetLyricsRequest(long musicId)
    {
        MusicId = musicId;
    }
}
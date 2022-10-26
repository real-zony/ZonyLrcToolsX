using Newtonsoft.Json;

namespace ZonyLrcTools.Common.Lyrics.Providers.KuWo.JsonModel;

public class SongSearchResponse
{
    [JsonProperty("code")] public int Code { get; set; }

    [JsonProperty("data")] public SongSearchResponseInnerData InnerData { get; set; }

    [JsonProperty("msg")] public string? ErrorMessage { get; set; }

    public long GetMatchedMusicId(string musicName, string artistName, long? duration)
    {
        var prefectMatch = InnerData.SongItems.FirstOrDefault(x => x.Name == musicName && x.Artist == artistName);
        if (prefectMatch != null)
        {
            return prefectMatch.MusicId;
        }

        if (duration is null or 0)
        {
            return InnerData.SongItems.First().MusicId;
        }

        return InnerData.SongItems.OrderBy(t => Math.Abs(t.Duration - duration.Value)).First().MusicId;
    }
}

public class SongSearchResponseInnerData
{
    [JsonProperty("total")] public string Total { get; set; }

    [JsonProperty("list")] public ICollection<SongSearchResponseDetail> SongItems { get; set; }
}

public class SongSearchResponseDetail
{
    [JsonProperty("artist")] public string? Artist { get; set; }

    [JsonProperty("name")] public string? Name { get; set; }

    [JsonProperty("rid")] public long MusicId { get; set; }

    [JsonProperty("duration")] public long Duration { get; set; }
}
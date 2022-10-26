using Newtonsoft.Json;

namespace ZonyLrcTools.Common.Lyrics.Providers.KuWo.JsonModel;

public class SongSearchRequest
{
    [JsonProperty("key")] public string Keyword { get; set; }

    [JsonProperty("pn")] public int PageNumber { get; }

    [JsonProperty("rn")] public int PageSize { get; }

    public SongSearchRequest(string name, string artist, int pageNumber = 1, int pageSize = 20)
    {
        Keyword = $"{name} {artist}";
        PageNumber = pageNumber;
        PageSize = pageSize;
    }
}
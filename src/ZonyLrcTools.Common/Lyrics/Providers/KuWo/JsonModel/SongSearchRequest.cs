using Newtonsoft.Json;

namespace ZonyLrcTools.Common.Lyrics.Providers.KuWo.JsonModel;

public class SongSearchRequest
{
    [JsonProperty("all")] public string Keyword { get; set; }

    [JsonProperty("pn")] public int PageNumber { get; }

    [JsonProperty("rn")] public int PageSize { get; }

    [JsonProperty("ft")] public string Unknown1 { get; } = "music";

    [JsonProperty("newsearch")] public string Unknown2 { get; } = "1";

    [JsonProperty("alflac")] public string Unknown3 { get; } = "1";

    [JsonProperty("itemset")] public string Unknown4 { get; } = "web_2013";

    [JsonProperty("client")] public string Unknown5 { get; } = "kt";

    [JsonProperty("cluster")] public string Unknown6 { get; } = "0";

    [JsonProperty("vermerge")] public string Unknown7 { get; } = "1";

    [JsonProperty("rformat")] public string Unknown8 { get; } = "json";

    [JsonProperty("encoding")] public string Unknown9 { get; } = "utf8";

    [JsonProperty("show_copyright_off")] public string Unknown10 { get; } = "1";

    [JsonProperty("pcmp4")] public string Unknown11 { get; } = "1";

    [JsonProperty("ver")] public string Unknown12 { get; } = "mbox";

    [JsonProperty("plat")] public string Unknown13 { get; } = "pc";

    [JsonProperty("vipver")] public string Unknown14 { get; } = "MUSIC_9.2.0.0_W6";

    [JsonProperty("devid")] public string Unknown15 { get; } = "11404450";

    [JsonProperty("newver")] public string Unknown16 { get; } = "1";

    [JsonProperty("issubtitle")] public string Unknown17 { get; } = "1";

    [JsonProperty("pcjson")] public string Unknown18 { get; } = "1";

    public SongSearchRequest(string name, string artist, int pageNumber = 0, int pageSize = 20)
    {
        Keyword = $"{name} {artist}";
        PageNumber = pageNumber;
        PageSize = pageSize;
    }
}
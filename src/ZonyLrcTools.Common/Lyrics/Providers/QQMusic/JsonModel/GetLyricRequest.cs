using Newtonsoft.Json;

namespace ZonyLrcTools.Common.Lyrics.Providers.QQMusic.JsonModel
{
    public class GetLyricRequest
    {
        [JsonProperty("nobase64")] public int IsNoBase64Encoding { get; set; }

        [JsonProperty("songmid")] public string? SongId { get; set; }

        [JsonProperty("platform")] public string ClientPlatform { get; set; }

        [JsonProperty("inCharset")] public string InCharset { get; set; }

        [JsonProperty("outCharset")] public string OutCharset { get; set; }

        [JsonProperty("g_tk")] public int Gtk { get; set; }

        public GetLyricRequest(string? songId)
        {
            IsNoBase64Encoding = 1;
            SongId = songId;
            ClientPlatform = "yqq";
            InCharset = "utf8";
            OutCharset = "utf-8";
            Gtk = 5381;
        }
    }
}
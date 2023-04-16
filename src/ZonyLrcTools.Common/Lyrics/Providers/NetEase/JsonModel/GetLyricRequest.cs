using Newtonsoft.Json;

// ReSharper disable InconsistentNaming

namespace ZonyLrcTools.Common.Lyrics.Providers.NetEase.JsonModel
{
    public class GetLyricRequest
    {
        public GetLyricRequest(long songId)
        {
            OS = "pc";
            Id = songId;
            Lv = Kv = Tv = Rv = -1;
        }

        /// <summary>
        /// 请求的操作系统。
        /// </summary>
        [JsonProperty("os")]
        public string OS { get; }

        /// <summary>
        /// 歌曲的 SID 值。
        /// </summary>
        [JsonProperty("id")]
        public long Id { get; }

        [JsonProperty("lv")] public int Lv { get; }

        [JsonProperty("kv")] public int Kv { get; }

        [JsonProperty("tv")] public int Tv { get; }

        [JsonProperty("rv")] public int Rv { get; set; }

        [JsonProperty("crypto")] public string Protocol { get; set; } = "api";
    }
}
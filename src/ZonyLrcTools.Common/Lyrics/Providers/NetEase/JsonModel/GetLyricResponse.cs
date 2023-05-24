using Newtonsoft.Json;

namespace ZonyLrcTools.Common.Lyrics.Providers.NetEase.JsonModel
{
    public class GetLyricResponse
    {
        /// <summary>
        /// 原始的歌词。
        /// </summary>
        [JsonProperty("lrc")]
        public InnerLyric? OriginalLyric { get; set; }

        /// <summary>
        /// 卡拉 OK 歌词。
        /// </summary>
        [JsonProperty("klyric")]
        public InnerLyric? KaraokeLyric { get; set; }

        /// <summary>
        /// 如果存在翻译歌词，则本项内容为翻译歌词。
        /// </summary>
        [JsonProperty("tlyric")]
        public InnerLyric? TranslationLyric { get; set; }

        /// <summary>
        /// 如果存在罗马音歌词，则本项内容为罗马音歌词。
        /// </summary>
        [JsonProperty("romalrc")]
        public InnerLyric? RomaLyric { get; set; }

        /// <summary>
        /// 状态码。
        /// </summary>
        [JsonProperty("code")]
        public string? StatusCode { get; set; }
    }

    /// <summary>
    /// 歌词 JSON 类型
    /// </summary>
    public class InnerLyric
    {
        [JsonProperty("version")] public string? Version { get; set; }

        /// <summary>
        /// 具体的歌词数据。
        /// </summary>
        [JsonProperty("lyric")]
        public string? Text { get; set; }
    }
}
namespace Zony.Lib.Net.JsonModels.NetEase
{
    /// <summary>
    /// 网易云音乐 API 歌词 JSON 模型
    /// </summary>
    public class NetEaseLyricModel
    {
        /// <summary>
        /// 原始歌词
        /// </summary>
        public InnerLyric lrc { get; set; }
        public InnerLyric klyric { get; set; }
        /// <summary>
        /// 翻译歌词
        /// </summary>
        public InnerLyric tlyric { get; set; }
        /// <summary>
        /// 状态码
        /// </summary>
        public string code { get; set; }
    }

    /// <summary>
    /// 歌词 JSON 类型
    /// </summary>
    public class InnerLyric
    {
        public string version { get; set; }
        public string lyric { get; set; }
    }
}

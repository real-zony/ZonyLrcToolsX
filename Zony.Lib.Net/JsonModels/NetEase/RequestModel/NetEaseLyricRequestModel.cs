namespace Zony.Lib.Net.JsonModels.NetEase.RequestModel
{
    /// <summary>
    /// Lyric API 请求模型
    /// </summary>
    public class NetEaseLyricRequestModel
    {
        /// <summary>
        /// Lyric API 请求模型 
        /// </summary>
        /// <param name="sid">歌曲 SID</param>
        public NetEaseLyricRequestModel(int sid)
        {
            os = "osx";
            id = sid;
            lv = kv = tv = -1;
        }

        public string os { get; }
        public int id { get; }
        public int lv { get; }
        public int kv { get; }
        public int tv { get; }
    }
}

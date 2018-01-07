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

        public string os { get; private set; }
        public int id { get; private set; }
        public int lv { get; private set; }
        public int kv { get; private set; }
        public int tv { get; private set; }
    }
}

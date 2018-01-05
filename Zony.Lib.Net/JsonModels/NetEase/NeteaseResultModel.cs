using System.Collections.Generic;

namespace Zony.Lib.Net.JsonModels.NetEase
{
    public class NetEaseResultModel
    {
        public NetEaseInnerResultModel result { get; set; }
        public string code { get; set; }
    }

    public class NetEaseInnerResultModel
    {
        /// <summary>
        /// 歌曲列表
        /// </summary>
        public List<NetEaseSongModel> songs { get; set; }
        /// <summary>
        /// 歌曲数量
        /// </summary>
        public int songsCount { get; set; }
        public List<NetEaseSongModel> tracks { get; set; }
    }
}

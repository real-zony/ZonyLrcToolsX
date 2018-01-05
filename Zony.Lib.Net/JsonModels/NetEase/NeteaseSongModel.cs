using System.Collections.Generic;

namespace Zony.Lib.Net.JsonModels.NetEase
{
    public class NetEaseSongModel
    {
        /// <summary>
        /// 歌曲名称
        /// </summary>
        public string name { get; set; }
        /// <summary>
        /// 歌曲 SID
        /// </summary>
        public int id { get; set; }
        /// <summary>
        /// 歌手
        /// </summary>
        public List<NetEaseArtistModel> artists { get; set; }
        /// <summary>
        /// 专辑
        /// </summary>
        public NetEaseAlbumModel album { get; set; }
    }
}
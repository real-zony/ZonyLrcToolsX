using System.Collections.Generic;
namespace Zony.Lib.Net.JsonModels.NetEase
{
    /// <summary>
    /// 歌曲详细信息 JSON 模型
    /// </summary>
    public class NetEaseSongDetailResultModel
    {
        /// <summary>
        /// 状态码
        /// </summary>
        public string code { get; set; }

        /// <summary>
        /// 歌曲信息列表
        /// </summary>
        public List<NetEaseSongModel> songs { get; set; }
    }
}

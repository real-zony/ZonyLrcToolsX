namespace Zony.Lib.Net.JsonModels.NetEase.RequestModel
{
    /// <summary>
    /// 歌曲搜索请求模型
    /// </summary>
    public class NetEaseSearchRequestModel
    {
        /// <summary>
        /// 歌曲搜索请求模型
        /// </summary>
        /// <param name="searchStr">搜索关键字</param>
        public NetEaseSearchRequestModel(string searchStr)
        {
            csrf_token = string.Empty;
            s = searchStr;
            type = 1;
            offset = 0;
            total = true;
            limit = 5;
        }

        public string csrf_token { get; set; }
        /// <summary>
        /// 搜索关键字
        /// </summary>
        public string s { get; set; }
        /// <summary>
        /// 页偏移
        /// </summary>
        public int offset { get; set; }
        /// <summary>
        /// 页容量
        /// </summary>
        public int limit { get; set; }
        /// <summary>
        /// 是否获得全部
        /// </summary>
        public bool total { get; set; }
        /// <summary>
        /// 搜索类型
        /// </summary>
        public int type { get; set; }
    }
}

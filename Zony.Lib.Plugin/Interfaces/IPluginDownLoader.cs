namespace Zony.Lib.Plugin.Interfaces
{
    /// <summary>
    /// 下载器插件
    /// </summary>
    public interface IPluginDownLoader
    {
        /// <summary>
        /// 下载歌词
        /// </summary>
        /// <param name="songName">歌曲名称</param>
        /// <param name="artistName">歌手/艺术家</param>
        /// <param name="data">输出的数据</param>
        void DownLoad(string songName, string artistName, out byte[] data);
    }
}
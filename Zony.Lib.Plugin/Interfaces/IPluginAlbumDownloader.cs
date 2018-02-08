using Zony.Lib.Plugin.Models;

namespace Zony.Lib.Plugin.Interfaces
{
    /// <summary>
    /// 专辑图像下载器
    /// </summary>
    public interface IPluginAlbumDownloader
    {
        /// <summary>
        /// 下载专辑图像
        /// </summary>
        /// <param name="info">音乐信息</param>
        /// <param name="imageData">下载完成的专辑图像数据流</param>
        /// <returns>是否下载成功</returns>
        bool DownlaodAblumImage(MusicInfoModel info, out byte[] imageData);
    }
}
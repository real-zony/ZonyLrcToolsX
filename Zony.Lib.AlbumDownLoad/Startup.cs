using Zony.Lib.Plugin.Attributes;
using Zony.Lib.Plugin.Interfaces;
using Zony.Lib.Plugin.Models;

namespace Zony.Lib.AlbumDownLoad
{
    [PluginInfo("专辑图像下载插件", "Zony", "1.0.0.0", "http://www.myzony.com", "从网易云音乐下载专辑图像")]
    public class Startup : IPluginAlbumDownloader
    {
        public bool DownlaodAblumImage(MusicInfoModel info, out byte[] imageData)
        {
            throw new System.NotImplementedException();
        }
    }
}

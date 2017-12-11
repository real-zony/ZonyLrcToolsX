using Zony.Lib.Plugin.Models;

namespace Zony.Lib.Plugin.Interfaces
{
    public interface IPluginAlbumDownloader
    {
        bool DownlaodAblumImage(MusicInfoModel info, out byte[] imageData);
    }
}

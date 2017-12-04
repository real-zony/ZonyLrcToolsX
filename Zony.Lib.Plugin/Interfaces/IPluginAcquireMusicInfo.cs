using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Zony.Lib.Plugin.Models;

namespace Zony.Lib.Plugin.Interfaces
{
    public interface IPluginAcquireMusicInfo
    {
        List<MusicInfoModel> GetMusicInfos(Dictionary<string, List<string>> musicFiles);
        Task<List<MusicInfoModel>> GetMusicInfosAsync(Dictionary<string, List<string>> musicFiles);
        MusicInfoModel GetMusicInfo(string filePath);
        MusicInfoModel GetMusicInfo(byte[] musicBytes);
        MusicInfoModel GetMusicInfo(Stream musicStream);
        Stream LoadAlbumImage(string filePath);
    }
}

using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Zony.Lib.Plugin.Attributes;
using Zony.Lib.Plugin.Interfaces;
using Zony.Lib.Plugin.Models;
using FileInfoLoader = TagLib.File;

namespace Zony.Lib.TagLib
{
    [PluginInfo("歌曲信息提取插件", "Zony", "1.0.0.0", "http://www.myzony.com", "提取歌曲信息的插件")]
    public class StartUp : IPluginAcquireMusicInfo
    {
        public MusicInfoModel GetMusicInfo(string filePath)
        {
            MusicInfoModel _info = new MusicInfoModel();
            string _fileName = Path.GetFileNameWithoutExtension(filePath);

            try
            {
                FileInfoLoader _loader = FileInfoLoader.Create(filePath);
                _info.Song = _loader.Tag.Title;
                _info.Artist = _loader.Tag.FirstPerformer;
                _info.Album = _loader.Tag.Album;

                if (string.IsNullOrEmpty(_info.Song)) _info.Song = _fileName;
                if (string.IsNullOrEmpty(_info.Artist)) _info.Artist = _fileName;

                _info.TagType = string.Join(@"/", _loader.TagTypes);
                _info.IsAlbumImg = _loader.Tag.Pictures.Length > 0 ? true : false;
                _info.IsBuildInLyric = string.IsNullOrEmpty(_loader.Tag.Lyrics);
            }
            catch (System.Exception)
            {
                _info.Song = _fileName;
                _info.Artist = _fileName;
                _info.TagType = "无";
                _info.IsAlbumImg = false;
                _info.IsBuildInLyric = false;
            }
            finally
            {
                _info.Extensions = Path.GetExtension(filePath);
                _info.FilePath = filePath;
            }

            return _info;
        }

        public MusicInfoModel GetMusicInfo(byte[] musicBytes)
        {
            throw new System.NotImplementedException();
        }

        public MusicInfoModel GetMusicInfo(Stream musicStream)
        {
            throw new System.NotImplementedException();
        }

        public List<MusicInfoModel> GetMusicInfos(Dictionary<string, List<string>> musicFiles)
        {
            throw new System.NotImplementedException();
        }

        public Task<List<MusicInfoModel>> GetMusicInfosAsync(Dictionary<string, List<string>> musicFiles)
        {
            throw new System.NotImplementedException();
        }

        public Stream LoadAlbumImage(string filePath)
        {
            throw new System.NotImplementedException();
        }
    }
}

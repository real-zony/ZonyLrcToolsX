using System;
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
                var _loader = FileInfoLoader.Create(filePath);
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

        public List<MusicInfoModel> GetMusicInfos(Dictionary<string, List<string>> musicFiles)
        {
            List<MusicInfoModel> _result = new List<MusicInfoModel>();

            foreach (var key in musicFiles)
            {
                foreach (var file in musicFiles[key.Key])
                {
                    _result.Add(GetMusicInfo(file));
                }
            }

            return _result;
        }

        public async Task<List<MusicInfoModel>> GetMusicInfosAsync(Dictionary<string, List<string>> musicFiles)
        {
            List<MusicInfoModel> _result = new List<MusicInfoModel>();

            foreach (var key in musicFiles)
            {
                foreach (var file in musicFiles[key.Key])
                {
                    var _info = await Task.Run(() => GetMusicInfo(file));
                    _result.Add(_info);
                }
            }

            return _result;
        }

        public Stream LoadAlbumImage(string filePath)
        {
            FileInfoLoader _fileInfo = FileInfoLoader.Create(filePath);
            if (_fileInfo.Tag.Pictures.Length == 0) return null;


        }

        public bool SaveAlbumImage(string filePath, Stream imageStream)
        {
            throw new NotImplementedException();
        }

        public bool SaveAlbumImage(string filePath, byte[] imgBytes)
        {
            throw new NotImplementedException();
        }

        public bool SaveMusicInfo(string filePath, MusicInfoModel musicInfo, out Exception E)
        {
            throw new NotImplementedException();
        }

        private MusicInfoModel LoadMusicInfo(FileInfoLoader loader, string filePath)
        {
            throw new NotImplementedException();
        }
    }
}

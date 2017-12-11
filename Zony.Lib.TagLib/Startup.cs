using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Zony.Lib.Plugin.Attributes;
using Zony.Lib.Plugin.Interfaces;
using Zony.Lib.Plugin.Models;

using TagFile = TagLib.File;
using TagPicture = TagLib.Picture;

namespace Zony.Lib.TagLib
{
    [PluginInfo(@"歌曲信息提取/写入插件", "Zony", "1.0.0.0", "http://www.myzony.com", "提取歌曲信息的插件")]
    public class StartUp : IPluginAcquireMusicInfo, IPlugin
    {
        public MusicInfoModel GetMusicInfo(string filePath)
        {
            MusicInfoModel _info = new MusicInfoModel();
            string _fileName = Path.GetFileNameWithoutExtension(filePath);

            try
            {
                var _file = TagFile.Create(filePath);
                _info.Song = _file.Tag.Title;
                _info.Artist = _file.Tag.FirstPerformer;
                _info.Album = _file.Tag.Album;

                if (string.IsNullOrEmpty(_info.Song)) _info.Song = _fileName;
                if (string.IsNullOrEmpty(_info.Artist)) _info.Artist = _fileName;

                _info.TagType = string.Join(@"/", _file.TagTypes);
                _info.IsAlbumImg = _file.Tag.Pictures.Length > 0 ? true : false;
                _info.IsBuildInLyric = string.IsNullOrEmpty(_file.Tag.Lyrics);
                if (_info.IsBuildInLyric) _info.BuildInLyric = _file.Tag.Lyrics;
            }
            catch (Exception)
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

            int _index = 0;
            foreach (var key in musicFiles)
            {
                foreach (var file in musicFiles[key.Key])
                {
                    MusicInfoModel _info = GetMusicInfo(file);
                    _info.Index = _index;
                    _result.Add(_info);
                    _index++;
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
            try
            {
                TagFile _file = TagFile.Create(filePath);
                if (_file.Tag.Pictures.Length == 0) return null;

                MemoryStream _result = new MemoryStream(_file.Tag.Pictures[0].Data.Data);
                return _result;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public bool SaveAlbumImage(string filePath, Stream imageStream)
        {
            byte[] _buffer = new byte[1024 * 16];
            using (MemoryStream _ms = new MemoryStream())
            {
                int _readCount = 0;
                while ((_readCount = imageStream.Read(_buffer, 0, _buffer.Length)) > 0)
                {
                    _ms.Write(_buffer, 0, _readCount);
                }

                return SaveAlbumImage(filePath, _ms.ToArray());
            }
        }

        public bool SaveAlbumImage(string filePath, byte[] imgBytes)
        {
            try
            {
                TagFile _file = TagFile.Create(filePath);

                var _picList = new List<TagPicture>() { new TagPicture(imgBytes) };
                _file.Tag.Pictures = _picList.ToArray();
                _file.Save();

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public bool SaveMusicInfo(string filePath, MusicInfoModel musicInfo, out Exception E)
        {
            E = null;
            if (musicInfo == null) return false;

            try
            {
                TagFile _file = TagFile.Create(filePath);

                _file.Tag.Title = musicInfo.Song;
                _file.Tag.Performers[0] = musicInfo.Artist;
                _file.Tag.Album = musicInfo.Album;

                _file.Save();

                return true;
            }
            catch (Exception InnerE)
            {
                E = InnerE;
                return false;
            }
        }
    }
}

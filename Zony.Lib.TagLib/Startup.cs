using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Zony.Lib.Plugin.Attributes;
using Zony.Lib.Plugin.Interfaces;
using Zony.Lib.Plugin.Models;
using TagFile = TagLib.File;
using TagPicture = TagLib.Picture;

namespace Zony.Lib.TagLib
{
    [PluginInfo(@"歌曲信息提取/写入插件", "Zony", "1.2.3.0", "http://www.myzony.com", "提取歌曲信息的插件")]
    public class StartUp : IPluginAcquireMusicInfo, IPlugin
    {
        public Dictionary<string, Dictionary<string, object>> PluginOptions { get; set; }

        public MusicInfoModel GetMusicInfo(string filePath)
        {
            MusicInfoModel info = new MusicInfoModel();
            string fileName = Path.GetFileNameWithoutExtension(filePath);

            try
            {
                var _file = TagFile.Create(filePath);
                info.Song = _file.Tag.Title;
                info.Artist = _file.Tag.FirstPerformer;
                if (!string.IsNullOrEmpty(_file.Tag.FirstAlbumArtist)) info.Artist = _file.Tag.FirstAlbumArtist;
                info.Album = _file.Tag.Album;

                if (string.IsNullOrEmpty(info.Song)) info.Song = fileName;
                if (string.IsNullOrEmpty(info.Artist)) info.Artist = fileName;

                info.TagType = string.Join(@"/", _file.TagTypes);
                info.IsAlbumImg = _file.Tag.Pictures.Length > 0 ? true : false;
                info.IsBuildInLyric = string.IsNullOrEmpty(_file.Tag.Lyrics);
                if (info.IsBuildInLyric) info.BuildInLyric = _file.Tag.Lyrics;
            }
            catch (Exception)
            {
                info.Song = fileName;
                info.Artist = fileName;
                info.TagType = "无";
                info.IsAlbumImg = false;
                info.IsBuildInLyric = false;
            }
            finally
            {
                info.Extensions = Path.GetExtension(filePath);
                info.FilePath = filePath;
            }

            return info;
        }

        public List<MusicInfoModel> GetMusicInfos(Dictionary<string, List<string>> musicFiles)
        {
            List<MusicInfoModel> result = new List<MusicInfoModel>();

            int _index = 0;
            foreach (var key in musicFiles)
            {
                foreach (var file in musicFiles[key.Key])
                {
                    MusicInfoModel info = GetMusicInfo(file);
                    info.Index = _index;
                    result.Add(info);
                    _index++;
                }
            }

            return result;
        }

        public async Task<List<MusicInfoModel>> GetMusicInfosAsync(Dictionary<string, List<string>> musicFiles)
        {
            return await Task.Run(() =>
            {
                ConcurrentDictionary<int, string> files = new ConcurrentDictionary<int, string>();
                int index = 0;
                foreach (var key in musicFiles)
                {
                    foreach (var _file in musicFiles[key.Key])
                    {
                        files.TryAdd(index, _file);
                        index++;
                    }
                }

                ConcurrentBag<MusicInfoModel> reuslt = new ConcurrentBag<MusicInfoModel>();

                Parallel.For(0, files.Count, (_index) =>
                 {
                     MusicInfoModel info = GetMusicInfo(files[_index]);
                     info.Index = _index;
                     reuslt.Add(info);
                 });

                return reuslt.ToList();
            });
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

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
                var file = TagFile.Create(filePath);
                info.Song = file.Tag.Title;
                info.Artist = file.Tag.FirstPerformer;
                if (!string.IsNullOrEmpty(file.Tag.FirstAlbumArtist)) info.Artist = file.Tag.FirstAlbumArtist;
                info.Album = file.Tag.Album;

                if (string.IsNullOrEmpty(info.Song)) info.Song = fileName;
                if (string.IsNullOrEmpty(info.Artist)) info.Artist = fileName;

                info.TagType = string.Join(@"/", file.TagTypes);
                info.IsAlbumImg = file.Tag.Pictures.Length > 0;
                info.IsBuildInLyric = string.IsNullOrEmpty(file.Tag.Lyrics);
                if (info.IsBuildInLyric) info.BuildInLyric = file.Tag.Lyrics;
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

            int index = 0;
            foreach (var key in musicFiles)
            {
                foreach (var file in musicFiles[key.Key])
                {
                    MusicInfoModel info = GetMusicInfo(file);
                    info.Index = index;
                    result.Add(info);
                    index++;
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
                    foreach (var file in musicFiles[key.Key])
                    {
                        files.TryAdd(index, file);
                        index++;
                    }
                }

                ConcurrentBag<MusicInfoModel> reuslt = new ConcurrentBag<MusicInfoModel>();

                Parallel.For(0, files.Count, i =>
                 {
                     MusicInfoModel info = GetMusicInfo(files[i]);
                     info.Index = i;
                     reuslt.Add(info);
                 });

                return reuslt.ToList();
            });
        }

        public Stream LoadAlbumImage(string filePath)
        {
            try
            {
                TagFile file = TagFile.Create(filePath);
                if (file.Tag.Pictures.Length == 0) return null;

                MemoryStream result = new MemoryStream(file.Tag.Pictures[0].Data.Data);
                return result;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public bool SaveAlbumImage(string filePath, Stream imageStream)
        {
            byte[] buffer = new byte[1024 * 16];
            using (MemoryStream ms = new MemoryStream())
            {
                int readCount;
                while ((readCount = imageStream.Read(buffer, 0, buffer.Length)) > 0)
                {
                    ms.Write(buffer, 0, readCount);
                }

                return SaveAlbumImage(filePath, ms.ToArray());
            }
        }

        public bool SaveAlbumImage(string filePath, byte[] imgBytes)
        {
            try
            {
                var file = TagFile.Create(filePath);

                var picList = new List<TagPicture> { new TagPicture(imgBytes) };
                file.Tag.Pictures = picList.ToArray();
                file.Save();

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public bool SaveMusicInfo(string filePath, MusicInfoModel musicInfo, out Exception e)
        {
            e = null;
            if (musicInfo == null) return false;

            try
            {
                TagFile file = TagFile.Create(filePath);

                file.Tag.Title = musicInfo.Song;
                file.Tag.Performers[0] = musicInfo.Artist;
                file.Tag.Album = musicInfo.Album;

                file.Save();

                return true;
            }
            catch (Exception innerE)
            {
                e = innerE;
                return false;
            }
        }
    }
}

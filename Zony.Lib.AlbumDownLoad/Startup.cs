using System.Collections.Generic;
using System.Net;
using System.Text;
using Zony.Lib.Net;
using Zony.Lib.Net.JsonModels.NetEase;
using Zony.Lib.Net.JsonModels.NetEase.RequestModel;
using Zony.Lib.Plugin.Attributes;
using Zony.Lib.Plugin.Interfaces;
using Zony.Lib.Plugin.Models;

namespace Zony.Lib.AlbumDownLoad
{
    [PluginInfo("专辑图像下载插件", "Zony", "1.1.1.0", "http://www.myzony.com", "从网易云音乐下载专辑图像")]
    public class Startup : IPluginAlbumDownloader, IPlugin
    {
        private readonly HttpMethodUtils m_netUtils = new HttpMethodUtils();

        public Dictionary<string, Dictionary<string, object>> PluginOptions { get; set; }

        public bool DownlaodAblumImage(MusicInfoModel info, out byte[] imageData)
        {
            imageData = null;

            var _songModel = GetMusicInfoByNetease(info.Artist, info.Song);
            if (_songModel.id == 0) return false;

            string _albumImg = GetAblumImageUrl(_songModel.id);

            imageData = new WebClient().DownloadData(_albumImg);
            return true;
        }

        /// <summary>
        /// 获取歌曲 SID 信息
        /// </summary>
        /// <param name="artist">艺术家</param>
        /// <param name="songName">歌曲名</param>
        private NetEaseSongModel GetMusicInfoByNetease(string artist, string songName)
        {
            string _artist = m_netUtils.URL_Encoding(artist, Encoding.UTF8);
            string _title = m_netUtils.URL_Encoding(songName, Encoding.UTF8);
            string _searchKey = $"{_artist}+{_title}";

            var _result = m_netUtils.Post<NetEaseResultModel>(url: @"http://music.163.com/api/search/get/web",
                                                parameters: new NetEaseSearchRequestModel(_searchKey),
                                                referer: "http://music.163.com",
                                                mediaTypeValue: "application/x-www-form-urlencoded");
            return _result.result.songs[0];
        }

        /// <summary>
        /// 获取专辑图像 URL 路径
        /// </summary>
        /// <param name="sid">歌曲 SID</param>
        /// <returns></returns>
        private string GetAblumImageUrl(int sid)
        {
            string _requestUrl = $"http://music.163.com/api/song/detail/";
            var _result = m_netUtils.GetAsync<NetEaseSongDetailResultModel>(_requestUrl, new
            {
                id = sid,
                ids = $"%5B{sid}%5D"
            }).Result;

            return _result?.songs?[0]?.album?.picUrl;
        }
    }
}
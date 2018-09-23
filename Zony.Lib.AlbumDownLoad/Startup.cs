using System.Collections.Generic;
using System.Net;
using System.Text;
using Zony.Lib.Net;
using Zony.Lib.Net.JsonModels.NetEase;
using Zony.Lib.Net.JsonModels.NetEase.RequestModel;
using Zony.Lib.Plugin.Attributes;
using Zony.Lib.Plugin.Exceptions;
using Zony.Lib.Plugin.Interfaces;
using Zony.Lib.Plugin.Models;

namespace Zony.Lib.AlbumDownLoad
{
    [PluginInfo("专辑图像下载插件", "Zony", "1.1.1.0", "http://www.myzony.com", "从网易云音乐下载专辑图像")]
    public class Startup : IPluginAlbumDownloader, IPlugin
    {
        private readonly HttpMethodUtils _mNetUtils = new HttpMethodUtils();

        public Dictionary<string, Dictionary<string, object>> PluginOptions { get; set; }

        public bool DownlaodAblumImage(MusicInfoModel info, out byte[] imageData)
        {
            imageData = null;

            var songModel = GetMusicInfoByNetease(info.Artist, info.Song);
            if (songModel.id == 0) return false;

            string albumImg = GetAblumImageUrl(songModel.id);

            if(albumImg == null) throw new ServiceUnavailableException("专辑图像下载：没有找到图像的 Url 地址.");

            imageData = new WebClient().DownloadData(albumImg);
            return true;
        }

        /// <summary>
        /// 获取歌曲 SID 信息
        /// </summary>
        /// <param name="artist">艺术家</param>
        /// <param name="songName">歌曲名</param>
        private NetEaseSongModel GetMusicInfoByNetease(string artist, string songName)
        {
            string encodingAritst = _mNetUtils.URL_Encoding(artist, Encoding.UTF8);
            string encodingTitle = _mNetUtils.URL_Encoding(songName, Encoding.UTF8);
            string searchKey = $"{encodingAritst}+{encodingTitle}";

            var result = _mNetUtils.Post<NetEaseResultModel>(url: @"http://music.163.com/api/search/get/web",
                                                parameters: new NetEaseSearchRequestModel(searchKey),
                                                referer: "http://music.163.com",
                                                mediaTypeValue: "application/x-www-form-urlencoded");

            var sidInfo = result?.result?.songs ?? throw new ServiceUnavailableException("专辑图像下载：请求 SID 时出错，没有 SID 数据.");
            if (sidInfo.Count == 0) throw new ServiceUnavailableException("专题图像下载：请求 SID 时出错，SID 集合没有数据.");

            return result.result.songs[0];
        }

        /// <summary>
        /// 获取专辑图像 URL 路径
        /// </summary>
        /// <param name="sid">歌曲 SID</param>
        /// <returns></returns>
        private string GetAblumImageUrl(int sid)
        {
            string requestUrl = $"http://music.163.com/api/song/detail/";
            var result = _mNetUtils.GetAsync<NetEaseSongDetailResultModel>(requestUrl, new
            {
                id = sid,
                ids = $"%5B{sid}%5D"
            }).Result;

            return result?.songs?[0]?.album?.picUrl;
        }
    }
}
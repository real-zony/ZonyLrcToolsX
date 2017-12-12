using Newtonsoft.Json.Linq;
using System.Net;
using System.Text;
using Zony.Lib.Net;
using Zony.Lib.Plugin.Attributes;
using Zony.Lib.Plugin.Interfaces;
using Zony.Lib.Plugin.Models;

namespace Zony.Lib.AlbumDownLoad
{
    [PluginInfo("专辑图像下载插件", "Zony", "1.0.0.0", "http://www.myzony.com", "从网易云音乐下载专辑图像")]
    public class Startup : IPluginAlbumDownloader,IPlugin
    {
        private readonly HttpMethodUtils m_netUtils = new HttpMethodUtils();

        public bool DownlaodAblumImage(MusicInfoModel info, out byte[] imageData)
        {
            imageData = null;

            string _resultStr = GetMusicInfoByNetease(info.Artist, info.Song);
            string _sid = GetSID(_resultStr);
            if (_sid == null) return false;

            string _albumImg = GetAblumImageUrl(_sid);

            imageData = new WebClient().DownloadData(_albumImg);
            return true;
        }

        private string GetMusicInfoByNetease(string artist, string songName)
        {
            string _artist = m_netUtils.URL_Encoding(artist, Encoding.UTF8);
            string _title = m_netUtils.URL_Encoding(songName, Encoding.UTF8);
            string _searchKey = $"{_artist}+{_title}";

            string _requestUrl = @"http://music.163.com/api/search/get/web";
            string _resultStr = m_netUtils.Post(url: _requestUrl,
                                                parameters: new
                                                {
                                                    csrf_token = string.Empty,
                                                    s = _searchKey,
                                                    type = 1,
                                                    offset = 0,
                                                    total = true,
                                                    limit = 5
                                                },
                                                referer: "http://music.163.com",
                                                mediaTypeValue: "application/x-www-form-urlencoded");
            return _resultStr;
        }

        private string GetSID(string _resultStr)
        {
            JObject _jsonSid = JObject.Parse(_resultStr);
            if (!_resultStr.Contains("result")) return null;
            if (_jsonSid["result"]["songCount"].Value<int>() == 0)
            {
                return null;
            }

            JArray _jarraySID = (JArray)_jsonSid["result"]["songs"];
            return _jarraySID[0]["id"].ToString();
        }

        private string GetAblumImageUrl(string sid)
        {
            string _requestUrl = $"http://music.163.com/api/song/detail/";
            string _songDetailJson = m_netUtils.Get(_requestUrl, new
            {
                id = sid,
                ids = $"%5B{sid}%5D"
            });

            JArray _songs = (JArray)JObject.Parse(_songDetailJson)["songs"];
            return _songs[0]["album"]["picUrl"].ToString();
        }
    }
}
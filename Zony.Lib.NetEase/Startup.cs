using Newtonsoft.Json.Linq;
using System;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Zony.Lib.Net;
using Zony.Lib.Plugin.Attributes;
using Zony.Lib.Plugin.Exceptions;
using Zony.Lib.Plugin.Interfaces;

namespace Zony.Lib.NetEase.Plugin
{
    [PluginInfo("网易云音乐歌词下载插件", "Zony", "2.0.0.0", "http://www.myzony.com", "可以从网易云音乐下载指定歌曲的歌词信息.")]
    public class Startup : IPluginDownLoader
    {
        private readonly HttpMethodUtils m_netUtils = new HttpMethodUtils();

        public void DownLoad(string songName, string artistName, out byte[] data)
        {
            var _param = buildParameters(songName, artistName);
            var _json = getLyricJsonObject(_param);
            var _sourceLyric = getSourceLyric(_json);
            var _translateLyric = getTranslateLyric(_json);

            data = null;
        }

        private object buildParameters(string songName, string artistName)
        {
            string _encodeArtistName = m_netUtils.URL_Encoding(songName, Encoding.UTF8);
            string _encodeSongName = m_netUtils.URL_Encoding(songName, Encoding.UTF8);
            return new
            {
                crsf_token = string.Empty,
                s = $"{_encodeArtistName}{_encodeSongName}",
                type = 1,
                offset = 0,
                total = true,
                limit = 5
            };
        }

        /// <summary>
        /// 获得歌词列表当中首位歌曲JSON对象
        /// </summary>
        /// <param name="postParam">提交访问的参数</param>
        /// <returns>返回的JSON对象</returns>
        private JObject getLyricJsonObject(object postParam)
        {
            var _result = m_netUtils.Post(@"http://music.163.com/api/search/get/web", postParam, @"http://music.163.com");
            if (!string.IsNullOrWhiteSpace(_result)) throw new NullReferenceException("在getLyricJsonObject当中无法获得请求的资源,_result");

            // 获得歌曲SID
            string _sid = getSongID(JObject.Parse(_result));
            // 请求歌词JSON数据
            var _params = new
            {
                os = "osx",
                id = _sid,
                lv = -1,
                kv = -1,
                tv = -1
            };
            string _lyric = m_netUtils.Get(@"http://music.163.com/api/song/lyric", _params, @"http://music.163.com");
            // 数据验证
            if (_lyric.Contains("nolyric")) throw new NotFoundLyricException("歌曲不存在歌词数据.");
            if (_lyric.Contains("uncollected")) throw new NotFoundLyricException("歌曲不存在歌词数据.");
            if (string.IsNullOrWhiteSpace(_lyric)) throw new NotFoundLyricException("歌曲不存在歌词数据.");

            var _jsonObject = JObject.Parse(_result);
            if (!jObjectIsContainsProperty(_jsonObject, "lrc")) throw new NotFoundLyricException("歌曲不存在歌词数据.");

            return _jsonObject["lrc"] as JObject;
        }

        /// <summary>
        /// 获得歌曲的SID
        /// </summary>
        /// <param name="sourceObj">返回的歌曲列表Json对象</param>
        /// <returns>匹配到的首位SID</returns>
        private string getSongID(JObject sourceObj)
        {
            if (sourceObj["result"]["songCount"].Value<int>() == 0) throw new ArgumentException("歌曲未搜索到任何结果，无法获取SID.");
            var _sids = (JArray)sourceObj["result"]["songs"];
            return _sids[0]["id"].Value<string>();
        }

        /// <summary>
        /// 获得原始歌词
        /// </summary>
        /// <param name="lyricJObj">歌词Json对象</param>
        /// <returns>获取到的原始歌词文本</returns>
        private string getSourceLyric(JObject lyricJObj)
        {
            if (!jObjectIsContainsProperty(lyricJObj, "lyric")) throw new NotFoundLyricException("歌曲不存在歌词数据.");
            return fixedLyricTimeFormat(lyricJObj["lyric"].Value<string>());
        }

        /// <summary>
        /// 获得翻译歌词
        /// </summary>
        /// <param name="lyricJObj"></param>
        /// <returns></returns>
        private string getTranslateLyric(JObject lyricJObj)
        {
            if (!jObjectIsContainsProperty(lyricJObj, "tlyric")) return string.Empty;
            if (lyricJObj["tlyric"]["lyric"] == null) return string.Empty;
            return fixedLyricTimeFormat(lyricJObj["tlyric"]["lyric"].Value<string>());
        }

        /// <summary>
        /// 歌词时间轴修复
        /// </summary>
        /// <param name="srcLyricText">待修复的三位时间轴文本</param>
        /// <returns>修复完成的二位时间轴文本</returns>
        private string fixedLyricTimeFormat(string srcLyricText)
        {
            Regex _regex = new Regex(@"\[\d+:\d+.\d+\]");
            return _regex.Replace(srcLyricText, new MatchEvaluator((Match _match) =>
            {
                string[] _strs = _match.Value.Split('.');
                string _value = _strs[1].Remove(_strs[1].Length - 2);
                int _iValue = int.Parse(_value);
                return string.Format("{0}.{1:D2}]", _strs[0], _iValue);
            }));
        }

        /// <summary>
        /// 判断一个JSON对象内部是否包含某个元素
        /// </summary>
        /// <param name="propertyName">元素名称</param>
        /// <returns>包含的话返回TRUE，不包含的返回FALSE</returns>
        private bool jObjectIsContainsProperty(JObject jObejct, string propertyName)
        {
            var _tmpCheckObj = jObejct.Properties().FirstOrDefault(x => x.Name == propertyName);
            return _tmpCheckObj != null ? true : false;
        }
    }
}
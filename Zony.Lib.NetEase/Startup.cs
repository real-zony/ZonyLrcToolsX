using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Zony.Lib.Net;
using Zony.Lib.Net.JsonModels.NetEase;
using Zony.Lib.Net.JsonModels.NetEase.RequestModel;
using Zony.Lib.Plugin.Attributes;
using Zony.Lib.Plugin.Exceptions;
using Zony.Lib.Plugin.Interfaces;

namespace Zony.Lib.NetEase
{
    [PluginInfo("网易云音乐歌词下载插件", "Zony", "2.0.5.0", "http://www.myzony.com", "可以从网易云音乐下载指定歌曲的歌词信息.")]
    public class Startup : IPluginDownLoader, IPlugin
    {
        private readonly HttpMethodUtils _httpClient = new HttpMethodUtils();
        private int m_FaildCount = 0;

        public void DownLoad(string songName, string artistName, out byte[] data)
        {
            var _param = BuildParameters(songName, artistName);

            try
            {
                var _json = GetLyricJsonObject(_param);
                var _sourceLyric = FixedLyricTimeFormat(_json.Item1);
                var _translateLyric = FixedLyricTimeFormat(_json.Item2);
                var _result = BuildLyricText(_sourceLyric, _translateLyric);
                data = Encoding.UTF8.GetBytes(ReplaceLF(_result));
            }
            catch (NotFoundLyricException)
            {
                if (m_FaildCount == 1)
                {
                    SongNameIsEmptyDownLoad(songName, out byte[] twiceData);
                    data = twiceData;
                }
                else
                {
                    throw new NotFoundLyricException();
                }
            }
        }

        /// <summary>
        /// 重试下载，不带 ArtistName 进行下载
        /// </summary>
        public void SongNameIsEmptyDownLoad(string songName, out byte[] data)
        {
            var _param = BuildParameters(songName, string.Empty);
            var _json = GetLyricJsonObject(_param);
            var _sourceLyric = FixedLyricTimeFormat(_json.Item1);
            var _translateLyric = FixedLyricTimeFormat(_json.Item2);
            var _result = BuildLyricText(_sourceLyric, _translateLyric);
            data = Encoding.UTF8.GetBytes(_result);
        }

        /// <summary>
        /// 构建查询参数
        /// </summary>
        /// <param name="songName">歌曲名</param>
        /// <param name="artistName">歌手</param>
        private object BuildParameters(string songName, string artistName)
        {
            string _encodeArtistName = _httpClient.URL_Encoding(artistName, Encoding.UTF8);
            string _encodeSongName = _httpClient.URL_Encoding(songName, Encoding.UTF8);
            return new NetEaseSearchRequestModel($"{_encodeArtistName}+{_encodeSongName}");
        }

        /// <summary>
        /// 获得歌词列表当中首位歌曲JSON对象
        /// </summary>
        /// <param name="postParam">提交访问的参数</param>
        /// <returns>返回的JSON对象</returns>
        private (string, string) GetLyricJsonObject(object postParam)
        {
            NetEaseResultModel _result = _httpClient.Post<NetEaseResultModel>(@"http://music.163.com/api/search/get/web", postParam, @"http://music.163.com", "application/x-www-form-urlencoded");
            if (_result == null) throw new ServiceUnavailableException("在getLyricJsonObject当中无法获得请求的资源,_result");
            if (_result?.result.songCount == 0)
            {
                m_FaildCount++;
                throw new NotFoundLyricException("歌曲未搜索到任何结果，无法获取SID.");
            }

            // 请求歌词JSON数据
            NetEaseLyricModel _lyric = _httpClient.Get<NetEaseLyricModel>(@"http://music.163.com/api/song/lyric", new NetEaseLyricRequestModel(_result.result.songs[0].id), @"http://music.163.com");
            if (_lyric.lrc == null) throw new NotFoundLyricException("歌曲不存在歌词数据.");
            return (_lyric.lrc.lyric, _lyric.tlyric.lyric);
        }

        /// <summary>
        /// 歌词时间轴修复
        /// </summary>
        /// <param name="srcLyricText">待修复的三位时间轴文本</param>
        /// <returns>修复完成的二位时间轴文本</returns>
        private string FixedLyricTimeFormat(string srcLyricText)
        {
            if (string.IsNullOrEmpty(srcLyricText)) return string.Empty;

            Regex _regex = new Regex(@"\[\d+:\d+.\d+\]");
            return _regex.Replace(srcLyricText, new MatchEvaluator((Match _match) =>
            {
                string[] _strs = _match.Value.Split('.');
                if (_strs.Length <= 1) return _match.Value;
                if (_strs[1].Length == 3) return _match.Value;

                string _value = _strs[1].Remove(_strs[1].Length - 2);
                if (!int.TryParse(_value, out int _iValue)) return _match.Value;

                return string.Format("{0}.{1:D2}]", _strs[0], _iValue);
            }));
        }

        /// <summary>
        /// 双语歌词合并
        /// </summary>
        /// <param name="srcLyric">原始歌词文本</param>
        /// <param name="transLyric">翻译中文歌词文本</param>
        /// <returns>构建完毕的歌词数据</returns>
        private string BuildLyricText(string srcLyric, string transLyric)
        {
            if (transLyric == string.Empty) return srcLyric;
            Dictionary<string, string> _srcDic = new Dictionary<string, string>();
            Dictionary<string, string> _transDic = new Dictionary<string, string>();
            StringBuilder _resultBuilder = new StringBuilder();

            // 键值对获取方法
            Dictionary<string, string> generateKey_Value(string lyric)
            {
                var _regex = new Regex(@"\[\d+:\d+.\d+\].+");
                var _matches = _regex.Matches(lyric);
                var _dict = new Dictionary<string, string>();

                foreach (var _match in _matches)
                {
                    var _value = _match.ToString();
                    // 切割歌词，分隔时间轴与歌词内容
                    int _pos = _value.IndexOf(']') + 1;
                    string _timeline = _value.Substring(0, _pos);
                    string _lyricText = _value.Substring(_pos, _value.Length - _pos);
                    if (_dict.ContainsKey(_timeline))
                    {
                        _dict[_timeline] = $"{_dict[_timeline]},{_lyricText}";
                    }
                    else _dict.Add(_timeline, _lyricText);
                }

                return _dict;
            }

            // 获取原始歌词与翻译歌词的键值对
            _srcDic = generateKey_Value(srcLyric);
            _transDic = generateKey_Value(transLyric);

            // 合并时间轴与歌词数据，以原始歌词为基准
            int _syncIndex = 0;
            foreach (var _src in _srcDic)
            {
                bool _isAppend = false; // 用于跳过不必要的循环，避免重复
                for (int _p = _syncIndex; _p < _srcDic.Count; _p++)
                {
                    var _trans = _transDic.ElementAtOrDefault(_p);
                    if (_isAppend == true) break;
                    if (_trans.Key == _src.Key) _resultBuilder.Append($"{_src.Key}{_src.Value},{_trans.Value}\n");
                    else
                    {
                        _resultBuilder.Append($"{_src.Key}{_src.Value}\n");
                        _resultBuilder.Append($"{_trans.Key}{_trans.Value}\n");
                    }
                    _isAppend = true;
                    _syncIndex++;
                }
            }

            return _resultBuilder.ToString();
        }

        /// <summary>
        /// 替换换行符
        /// </summary>
        /// <param name="srcString">原始字符串</param>
        /// <returns></returns>
        private string ReplaceLF(string srcString)
        {
            return new Regex("\n").Replace(srcString, new MatchEvaluator((Match _match) =>
            {
                return "\r\n";
            }));
        }
    }
}
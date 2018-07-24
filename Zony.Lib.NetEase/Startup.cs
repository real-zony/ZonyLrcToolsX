using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Zony.Lib.Net;
using Zony.Lib.Net.JsonModels.NetEase;
using Zony.Lib.Net.JsonModels.NetEase.RequestModel;
using Zony.Lib.Plugin.Attributes;
using Zony.Lib.Plugin.Common.Extensions;
using Zony.Lib.Plugin.Exceptions;
using Zony.Lib.Plugin.Interfaces;

namespace Zony.Lib.NetEase
{
    [PluginInfo("网易云音乐歌词下载插件", "Zony", "2.1.5.0", "http://www.myzony.com", "可以从网易云音乐下载指定歌曲的歌词信息.")]
    public class Startup : IPluginDownLoader, IPlugin
    {
        private readonly HttpMethodUtils _httpClient = new HttpMethodUtils();
        private int _mFaildCount;

        public Dictionary<string, Dictionary<string, object>> PluginOptions { get; set; }

        public void DownLoad(string songName, string artistName, out byte[] data)
        {
            var param = BuildParameters(songName, artistName);

            try
            {
                var json = GetLyricJsonObject(param);
                var sourceLyric = FixedLyricTimeFormat(json.Item1);
                var translateLyric = FixedLyricTimeFormat(json.Item2);
                var result = BuildLyricText(sourceLyric, translateLyric);
                bool isReplaceLf = PluginOptions.GetOptionValue<bool>(typeof(Startup).Assembly, "ReplaceLF");
                if (isReplaceLf) data = Encoding.UTF8.GetBytes(ReplaceLF(result));
                else data = Encoding.UTF8.GetBytes(result);
            }
            catch (NotFoundLyricException)
            {
                if (_mFaildCount == 1)
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
            var param = BuildParameters(songName, string.Empty);
            var json = GetLyricJsonObject(param);
            var sourceLyric = FixedLyricTimeFormat(json.Item1);
            var translateLyric = FixedLyricTimeFormat(json.Item2);
            var result = BuildLyricText(sourceLyric, translateLyric);
            bool isReplaceLf = PluginOptions.GetOptionValue<bool>(typeof(Startup).Assembly, "ReplaceLF");
            if (isReplaceLf) data = Encoding.UTF8.GetBytes(ReplaceLF(result));
            data = Encoding.UTF8.GetBytes(result);
        }

        /// <summary>
        /// 构建查询参数
        /// </summary>
        /// <param name="songName">歌曲名</param>
        /// <param name="artistName">歌手</param>
        private object BuildParameters(string songName, string artistName)
        {
            string encodeArtistName = _httpClient.URL_Encoding(artistName, Encoding.UTF8);
            string encodeSongName = _httpClient.URL_Encoding(songName, Encoding.UTF8);
            return new NetEaseSearchRequestModel($"{encodeArtistName}+{encodeSongName}");
        }

        /// <summary>
        /// 获得歌词列表当中首位歌曲JSON对象
        /// </summary>
        /// <param name="postParam">提交访问的参数</param>
        /// <returns>返回的JSON对象</returns>
        private (string, string) GetLyricJsonObject(object postParam)
        {
            NetEaseResultModel result = _httpClient.Post<NetEaseResultModel>(@"http://music.163.com/api/search/get/web", postParam, @"http://music.163.com", "application/x-www-form-urlencoded");
            if (result == null) throw new ServiceUnavailableException("在getLyricJsonObject当中无法获得请求的资源,_result");
            if (result.result.songCount == 0)
            {
                _mFaildCount++;
                throw new NotFoundLyricException("歌曲未搜索到任何结果，无法获取SID.");
            }

            // 请求歌词JSON数据
            NetEaseLyricModel lyric = _httpClient.Get<NetEaseLyricModel>(@"http://music.163.com/api/song/lyric", new NetEaseLyricRequestModel(result.result.songs[0].id), @"http://music.163.com");
            if (lyric == null) throw new ServiceUnavailableException("服务限制");
            if (lyric.lrc == null) throw new NotFoundLyricException("歌曲不存在歌词数据.");
            return (lyric.lrc.lyric, lyric.tlyric.lyric);
        }

        /// <summary>
        /// 歌词时间轴修复
        /// </summary>
        /// <param name="srcLyricText">待修复的三位时间轴文本</param>
        /// <returns>修复完成的二位时间轴文本</returns>
        private string FixedLyricTimeFormat(string srcLyricText)
        {
            if (string.IsNullOrEmpty(srcLyricText)) return string.Empty;

            Regex regex = new Regex(@"\[\d+:\d+.\d+\]");
            return regex.Replace(srcLyricText, match =>
            {
                string[] strs = match.Value.Split('.');
                if (strs.Length <= 1) return match.Value;
                if (strs[1].Length == 3) return match.Value;

                string value = strs[1].Remove(strs[1].Length - 2);
                if (!int.TryParse(value, out int iValue)) return match.Value;

                return $"{strs[0]}.{iValue:D2}]";
            });
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
            StringBuilder resultBuilder = new StringBuilder();

            // 键值对获取方法
            Dictionary<string, string> GenerateKeyValue(string lyric)
            {
                var regex = new Regex(@"\[\d+:\d+.\d+\].+");
                var matches = regex.Matches(lyric);
                var dict = new Dictionary<string, string>();

                foreach (var match in matches)
                {
                    var value = match.ToString();
                    // 切割歌词，分隔时间轴与歌词内容
                    int pos = value.IndexOf(']') + 1;
                    string timeline = value.Substring(0, pos);
                    string lyricText = value.Substring(pos, value.Length - pos);
                    if (dict.ContainsKey(timeline))
                    {
                        dict[timeline] = $"{dict[timeline]},{lyricText}";
                    }
                    else dict.Add(timeline, lyricText);
                }

                return dict;
            }

            // 获取原始歌词与翻译歌词的键值对
            var srcDic = GenerateKeyValue(srcLyric);
            var transDic = GenerateKeyValue(transLyric);

            // 合并时间轴与歌词数据，以原始歌词为基准
            int syncIndex = 0;
            foreach (var src in srcDic)
            {
                bool isAppend = false; // 用于跳过不必要的循环，避免重复
                for (int p = syncIndex; p < srcDic.Count; p++)
                {
                    var trans = transDic.ElementAtOrDefault(p);
                    if (isAppend) break;
                    if (trans.Key == src.Key) resultBuilder.Append($"{src.Key}{src.Value},{trans.Value}\n");
                    else
                    {
                        resultBuilder.Append($"{src.Key}{src.Value}\n");
                        resultBuilder.Append($"{trans.Key}{trans.Value}\n");
                    }
                    isAppend = true;
                    syncIndex++;
                }
            }

            return resultBuilder.ToString();
        }

        /// <summary>
        /// 替换换行符
        /// </summary>
        /// <param name="srcString">原始字符串</param>
        /// <returns></returns>
        private string ReplaceLF(string srcString)
        {
            return new Regex("\n").Replace(srcString, match => "\r\n");
        }
    }
}
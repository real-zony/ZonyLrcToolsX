using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using Zony.Lib.Infrastructures.Lyrics;
using Zony.Lib.Net;
using Zony.Lib.Net.JsonModels.NetEase;
using Zony.Lib.Net.JsonModels.NetEase.RequestModel;
using Zony.Lib.Plugin.Attributes;
using Zony.Lib.Plugin.Common.Extensions;
using Zony.Lib.Plugin.Exceptions;
using Zony.Lib.Plugin.Interfaces;

namespace Zony.Lib.NetEase
{
    [PluginInfo("网易云音乐歌词下载插件", "Zony", "2.2.1.0", "http://www.myzony.com", "可以从网易云音乐下载指定歌曲的歌词信息.")]
    public class Startup : IPluginDownLoader, IPlugin
    {
        private readonly HttpMethodUtils _httpClient = new HttpMethodUtils();

        private int _mFaildCount;
        private bool _isInline;
        private bool _isReplaceLf;
        private bool _isOpenTransLyric;

        public Dictionary<string, Dictionary<string, object>> PluginOptions { get; set; }

        public void DownLoad(string songName, string artistName, out byte[] data)
        {
            _isReplaceLf = PluginOptions.GetOptionValue<bool>(typeof(Startup).Assembly, "ReplaceLF");
            _isInline = PluginOptions.GetOptionValue<bool>(typeof(Startup).Assembly, "Inline");
            _isOpenTransLyric = PluginOptions.GetOptionValue<bool>(typeof(Startup).Assembly, "IsOpenTransLyric");

            var param = BuildParameters(songName, artistName);

            try
            {
                var json = GetLyricJsonObject(param);
                var result = BuildLyricText(new LyricItemCollection(json.Item1), new LyricItemCollection(json.Item2));
                if (_isReplaceLf) data = Encoding.UTF8.GetBytes(ReplaceLF(result));
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
            var result = BuildLyricText(new LyricItemCollection(json.Item1), new LyricItemCollection(json.Item2));
            if (_isReplaceLf) data = Encoding.UTF8.GetBytes(ReplaceLF(result));
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
            if (result.result == null || result.result.songCount == 0)
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
        /// 双语歌词合并
        /// </summary>
        /// <param name="srcLyric">原始歌词文本</param>
        /// <param name="transLyric">翻译中文歌词文本</param>
        /// <returns>构建完毕的歌词数据</returns>
        private string BuildLyricText(LyricItemCollection srcLyric, LyricItemCollection transLyric)
        {
            if (!_isOpenTransLyric)
            {
                srcLyric.Sort();
                return srcLyric.ToString();
            }

            srcLyric.Sort();
            transLyric.Sort();

            var result = srcLyric.Merge(transLyric, _isInline).ToString();
            return result;
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
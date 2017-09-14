using Newtonsoft.Json.Linq;
using Zony.Lib.Plugin.Interfaces;

namespace Zony.Lib.NetEase.Plugin
{
    public class Startup : IPluginDownLoader
    {
        public bool DownLoad(string songName, string artistName, out byte[] data)
        {
            var _param = buildParameters(songName, artistName);
            var _json = getLyricJsonObject(_param);
            var _sourceLyric = getSourceLyric(_json);
            var _translateLyric = getTranslateLyric(_json);

            data = null;
            return true;
        }

        private object buildParameters(string songName, string artistName)
        {
            return new
            {

            };
        }

        private JObject getLyricJsonObject(object postParam)
        {
            return null;
        }

        private string getSongID(JObject sourceObj)
        {
            return null;
        }

        private string getSourceLyric(JObject lyricJObj)
        {
            return null;
        }

        private string getTranslateLyric(JObject lyricJObj)
        {
            return null;
        }
    }
}
using System.Text;
using System.Web;
using Newtonsoft.Json;

namespace ZonyLrcTools.Cli.Infrastructure.Lyric.QQMusic.JsonModel
{
    public class SongSearchRequest
    {
        [JsonProperty("ct")] public int UnknownParameter1 { get; set; }

        [JsonProperty("qqmusic_ver")] public int ClientVersion { get; set; }

        [JsonProperty("new_json")] public int UnknownParameter2 { get; set; }

        [JsonProperty("remoteplace")] public string RemotePlace { get; set; }

        [JsonProperty("t")] public int UnknownParameter3 { get; set; }

        [JsonProperty("aggr")] public int UnknownParameter4 { get; set; }

        [JsonProperty("cr")] public int UnknownParameter5 { get; set; }

        [JsonProperty("catZhida")] public int UnknownParameter6 { get; set; }

        [JsonProperty("lossless")] public int LossLess { get; set; }

        [JsonProperty("flag_qc")] public int UnknownParameter7 { get; set; }

        [JsonProperty("p")] public int Page { get; set; }

        [JsonProperty("n")] public int Limit { get; set; }

        [JsonProperty("w")] public string Keyword { get; set; }

        [JsonProperty("g_tk")] public int UnknownParameter8 { get; set; }

        [JsonProperty("hostUin")] public int UnknownParameter9 { get; set; }

        [JsonProperty("format")] public string ResultFormat { get; set; }

        [JsonProperty("inCharset")] public string InCharset { get; set; }

        [JsonProperty("outCharset")] public string OutCharset { get; set; }

        [JsonProperty("notice")] public int UnknownParameter10 { get; set; }

        [JsonProperty("platform")] public string Platform { get; set; }

        [JsonProperty("needNewCode")] public int UnknownParameter11 { get; set; }

        protected SongSearchRequest()
        {
            UnknownParameter1 = 24;
            ClientVersion = 1298;
            UnknownParameter2 = 1;
            RemotePlace = "txt.yqq.song";
            UnknownParameter3 = 0;
            UnknownParameter4 = 1;
            UnknownParameter5 = 1;
            UnknownParameter6 = 1;
            LossLess = 0;
            UnknownParameter7 = 0;
            Page = 1;
            Limit = 5;
            UnknownParameter8 = 5381;
            UnknownParameter9 = 0;
            ResultFormat = "json";
            InCharset = "utf8";
            OutCharset = "utf8";
            UnknownParameter10 = 0;
            Platform = "yqq";
            UnknownParameter11 = 0;
        }

        public SongSearchRequest(string musicName, string artistName) : this()
        {
            Keyword = HttpUtility.UrlEncode($"{musicName}+{artistName}", Encoding.UTF8);
        }
    }
}
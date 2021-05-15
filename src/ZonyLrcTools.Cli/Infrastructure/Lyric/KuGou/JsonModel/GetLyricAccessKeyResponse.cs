using System.Collections.Generic;
using Newtonsoft.Json;

namespace ZonyLrcTools.Cli.Infrastructure.Lyric.KuGou.JsonModel
{
    public class GetLyricAccessKeyResponse
    {
        [JsonProperty("status")]
        public int Status { get; set; }

        [JsonProperty("errcode")]
        public int ErrorCode { get; set; }

        [JsonProperty("candidates")]
        public List<GetLyricAccessKeyDataObject> AccessKeyDataObjects { get; set; }
    }

    public class GetLyricAccessKeyDataObject
    {
        [JsonProperty("accesskey")]
        public string AccessKey { get; set; }

        [JsonProperty("id")]
        public string Id { get; set; }
    }
}
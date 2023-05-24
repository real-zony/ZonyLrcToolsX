using Newtonsoft.Json;

namespace ZonyLrcTools.Common.Lyrics.Providers.KuGou.JsonModel
{
    public class GetLyricAccessKeyRequest
    {
        [JsonProperty("ver")] public int UnknownParameters1 { get; }

        [JsonProperty("man")] public string UnknownParameters2 { get; }

        [JsonProperty("client")] public string UnknownParameters3 { get; }

        [JsonProperty("hash")] public string? FileHash { get; }

        public GetLyricAccessKeyRequest(string? fileHash)
        {
            UnknownParameters1 = 1;
            UnknownParameters2 = "yes";
            UnknownParameters3 = "mobi";
            FileHash = fileHash;
        }
    }
}
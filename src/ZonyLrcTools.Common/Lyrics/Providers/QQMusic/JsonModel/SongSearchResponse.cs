using Newtonsoft.Json;

namespace ZonyLrcTools.Common.Lyrics.Providers.QQMusic.JsonModel
{
    public class SongSearchResponse
    {
        [JsonProperty("code")] public int StatusCode { get; set; }

        [JsonProperty("data")] public QQMusicInnerDataModel? Data { get; set; }
    }

    public class QQMusicInnerDataModel
    {
        [JsonProperty("song")] public QQMusicInnerSongModel? Song { get; set; }
    }

    public class QQMusicInnerSongModel
    {
        [JsonProperty("itemlist")] public List<QQMusicInnerSongItem>? SongItems { get; set; }
    }

    public class QQMusicInnerSongItem
    {
        [JsonProperty("mid")] public string? SongId { get; set; }
    }
}
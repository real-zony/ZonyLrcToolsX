using System.Collections.Generic;
using Newtonsoft.Json;

namespace ZonyLrcTools.Cli.Infrastructure.Lyric.QQMusic.JsonModel
{
    public class SongSearchResponse
    {
        [JsonProperty("code")] public int StatusCode { get; set; }

        [JsonProperty("data")] public QQMusicInnerDataModel Data { get; set; }
    }

    public class QQMusicInnerDataModel
    {
        [JsonProperty("song")] public QQMusicInnerSongModel Song { get; set; }
    }

    public class QQMusicInnerSongModel
    {
        [JsonProperty("list")] public List<QQMusicInnerSongItem> SongItems { get; set; }
    }

    public class QQMusicInnerSongItem
    {
        [JsonProperty("songmid")] public string SongId { get; set; }
    }
}
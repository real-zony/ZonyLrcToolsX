using Newtonsoft.Json;

namespace ZonyLrcTools.Common.Lyrics.Providers.NetEase.JsonModel
{
    public class SongSearchResponse
    {
        [JsonProperty("result")] public InnerListItemModel? Items { get; set; }

        [JsonProperty("code")] public int StatusCode { get; set; }

        public int GetFirstMatchSongId(string songName, long? duration)
        {
            var perfectMatch = Items.SongItems.FirstOrDefault(x => x.Name == songName);
            if (perfectMatch != null)
            {
                return perfectMatch.Id;
            }

            if (duration is null or 0)
            {
                return Items.SongItems.First().Id;
            }

            return Items.SongItems.OrderBy(t => Math.Abs(t.Duration - duration.Value)).First().Id;
        }
    }

    public class InnerListItemModel
    {
        [JsonProperty("songs")] public IList<SongModel>? SongItems { get; set; }

        [JsonProperty("songCount")] public int SongCount { get; set; }
    }

    public class SongModel
    {
        /// <summary>
        /// 歌曲的名称。
        /// </summary>
        [JsonProperty("name")]
        public string Name { get; set; }

        /// <summary>
        /// 歌曲的 Sid (Song Id)。
        /// </summary>
        [JsonProperty("id")]
        public int Id { get; set; }

        /// <summary>
        /// 歌曲的演唱者。
        /// </summary>
        [JsonProperty("artists")]
        public IList<SongArtistModel> Artists { get; set; }

        /// <summary>
        /// 歌曲的专辑信息。
        /// </summary>
        [JsonProperty("album")]
        public SongAlbumModel Album { get; set; }

        /// <summary>
        /// 歌曲的实际长度。
        /// </summary>
        [JsonProperty("duration")]
        public long Duration { get; set; }
    }

    public class SongArtistModel
    {
        /// <summary>
        /// 歌手/艺术家的名称。
        /// </summary>
        [JsonProperty("name")]
        public string Name { get; set; }
    }

    public class SongAlbumModel
    {
        /// <summary>
        /// 专辑的名称。
        /// </summary>
        [JsonProperty("name")]
        public string Name { get; set; }

        /// <summary>
        /// 专辑图像的 Url 地址。
        /// </summary>
        [JsonProperty("img1v1Url")]
        public string PictureUrl { get; set; }
    }
}
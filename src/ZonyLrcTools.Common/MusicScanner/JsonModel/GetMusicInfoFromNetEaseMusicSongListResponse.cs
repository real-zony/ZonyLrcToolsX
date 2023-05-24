using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace ZonyLrcTools.Common.MusicScanner.JsonModel;

public sealed class GetMusicInfoFromNetEaseMusicSongListResponse
{
    /// <summary>
    /// 请求结果代码，为 200 时请求成功。
    /// </summary>
    [JsonProperty("code")]
    public int Code { get; set; }

    /// <summary>
    /// 歌单信息。
    /// </summary>
    [JsonProperty("playlist")]
    public PlayListModel? PlayList { get; set; }
}

public sealed class PlayListModel
{
    /// <summary>
    /// 歌单的歌曲列表。
    /// </summary>
    [JsonProperty("tracks")]
    public ICollection<PlayListSongModel>? SongList { get; set; }
}

public sealed class PlayListSongModel
{
    /// <summary>
    /// 歌曲的名称。
    /// </summary>
    [JsonProperty("name")]
    public string? Name { get; set; }

    /// <summary>
    /// 歌曲的艺术家信息，可能会有多位艺术家/歌手。
    /// </summary>
    [JsonProperty("ar")]
    [JsonConverter(typeof(PlayListSongArtistModelJsonConverter))]
    public ICollection<PlayListSongArtistModel>? Artists { get; set; }
}

public sealed class PlayListSongArtistModel
{
    /// <summary>
    /// 艺术家的名称。
    /// </summary>
    [JsonProperty("name")]
    public string? Name { get; set; }
}

public class PlayListSongArtistModelJsonConverter : JsonConverter
{
    public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
    {
        throw new NotImplementedException();
    }

    public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
    {
        var token = JToken.Load(reader);
        return token.Type switch
        {
            JTokenType.Array => token.ToObject(objectType),
            JTokenType.Object => new List<PlayListSongArtistModel> { token.ToObject<PlayListSongArtistModel>()! },
            _ => null
        };
    }

    public override bool CanConvert(Type objectType)
    {
        return objectType == typeof(ICollection<PlayListSongArtistModel>);
    }
}
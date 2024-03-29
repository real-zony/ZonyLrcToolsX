﻿using Newtonsoft.Json;

namespace ZonyLrcTools.Common.Lyrics.Providers.NetEase.JsonModel
{
    public class GetSongDetailsRequest
    {
        public GetSongDetailsRequest(int songId)
        {
            SongId = songId;
            SongIds = $"%5B{songId}%5D";
        }

        [JsonProperty("id")] public int SongId { get; }

        [JsonProperty("ids")] public string SongIds { get; }
    }
}
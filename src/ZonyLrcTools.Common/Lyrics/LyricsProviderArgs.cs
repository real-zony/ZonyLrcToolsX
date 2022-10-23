namespace ZonyLrcTools.Common.Lyrics
{
    public class LyricsProviderArgs
    {
        public string SongName { get; set; }

        public string Artist { get; set; }

        public long Duration { get; set; }

        public LyricsProviderArgs(string songName, string artist, long duration)
        {
            SongName = songName;
            Artist = artist;
            Duration = duration;
        }
    }
}
namespace ZonyLrcTools.Cli.Infrastructure.Lyric
{
    public class LyricDownloaderArgs
    {
        public string SongName { get; set; }

        public string Artist { get; set; }

        public long Duration { get; set; }

        public LyricDownloaderArgs(string songName, string artist, long duration)
        {
            SongName = songName;
            Artist = artist;
            Duration = duration;
        }
    }
}
namespace ZonyLrcTools.Cli.Infrastructure.Lyric
{
    public class LyricDownloaderArgs
    {
        public string SongName { get; set; }

        public string Artist { get; set; }

        public LyricDownloaderArgs(string songName, string artist)
        {
            SongName = songName;
            Artist = artist;
        }
    }
}
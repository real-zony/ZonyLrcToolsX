namespace ZonyLrcTools.Cli.Infrastructure
{
    public class MusicInfo
    {
        public string FilePath { get; }

        public string Name { get; }

        public string Artist { get; }

        public MusicInfo(string filePath, string name, string artist)
        {
            FilePath = filePath;
            Name = name;
            Artist = artist;
        }
    }
}
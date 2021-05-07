namespace ZonyLrcTools.Cli.Infrastructure.Lyric
{
    public interface ILyricTextResolver
    {
        LyricItemCollection Resolve(string lyricText);
    }
}
namespace ZonyLrcTools.Common.Lyrics
{
    public interface ILyricTextResolver
    {
        LyricItemCollection Resolve(string lyricText);
    }
}
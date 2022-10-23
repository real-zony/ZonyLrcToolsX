namespace ZonyLrcTools.Common.Lyrics
{
    public interface ILyricsTextResolver
    {
        LyricItemCollection Resolve(string lyricText);
    }
}
namespace ZonyLrcTools.Common.Lyrics
{
    public interface ILyricsTextResolver
    {
        LyricsItemCollection Resolve(string lyricText);
    }
}
namespace ZonyLrcTools.Common.Lyrics
{
    /// <summary>
    /// 构建 <see cref="LyricsItemCollection"/> 对象的工厂。
    /// </summary>
    public interface ILyricsItemCollectionFactory
    {
        /// <summary>
        /// 根据指定的歌曲数据构建新的 <see cref="LyricsItemCollection"/> 实例。
        /// </summary>
        /// <param name="sourceLyric">原始歌词数据。</param>
        /// <returns>构建完成的 <see cref="LyricsItemCollection"/> 对象。</returns>
        LyricsItemCollection Build(string? sourceLyric);

        /// <summary>
        /// 根据指定的歌曲数据构建新的 <see cref="LyricsItemCollection"/> 实例。
        /// </summary>
        /// <param name="sourceLyric">原始歌词数据。</param>
        /// <param name="translationLyric">翻译歌词数据。</param>
        /// <returns>构建完成的 <see cref="LyricsItemCollection"/> 对象。</returns>
        LyricsItemCollection Build(string? sourceLyric, string? translationLyric);
    }
}
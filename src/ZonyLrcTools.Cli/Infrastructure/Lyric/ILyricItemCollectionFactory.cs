namespace ZonyLrcTools.Cli.Infrastructure.Lyric
{
    /// <summary>
    /// 构建 <see cref="LyricItemCollection"/> 对象的工厂。
    /// </summary>
    public interface ILyricItemCollectionFactory
    {
        /// <summary>
        /// 根据指定的歌曲数据构建新的 <see cref="LyricItemCollection"/> 实例。
        /// </summary>
        /// <param name="sourceLyric">原始歌词数据。</param>
        /// <param name="translateLyric">翻译歌词数据。</param>
        /// <returns>构建完成的 <see cref="LyricItemCollection"/> 对象。</returns>
        LyricItemCollection Build(string sourceLyric, string translateLyric = null);
    }
}
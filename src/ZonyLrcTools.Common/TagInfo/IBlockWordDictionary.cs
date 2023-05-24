namespace ZonyLrcTools.Cli.Infrastructure.Tag
{
    /// <summary>
    /// 屏蔽词字典。
    /// </summary>
    public interface IBlockWordDictionary
    {
        /// <summary>
        /// 根据 <paramref name="key"/> 获得屏蔽词结果。
        /// </summary>
        /// <remarks>
        /// 例: 原歌曲的 "fuckking" ，在网易云实际为 "***kking"。
        /// </remarks>
        /// <param name="key">原始单词。</param>
        /// <returns>原始单词对应的屏蔽词。</returns>
        string? GetValue(string key);
    }
}
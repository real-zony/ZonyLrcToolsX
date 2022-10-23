namespace ZonyLrcTools.Common.TagInfo
{
    /// <summary>
    /// 标签加载器，用于加载文件的音乐标签信息。
    /// </summary>
    public interface ITagLoader
    {
        /// <summary>
        /// 加载歌曲文件的标签信息。
        /// </summary>
        /// <param name="filePath">歌曲文件的路径。</param>
        /// <returns>加载完成的歌曲信息。</returns>
        ValueTask<MusicInfo?> LoadTagAsync(string filePath);
    }
}
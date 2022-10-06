namespace ZonyLrcTools.Common.Infrastructure.IO
{
    /// <summary>
    /// 音乐文件扫描器，用于扫描音乐文件。
    /// </summary>
    public interface IFileScanner
    {
        /// <summary>
        /// 扫描指定路径下面的歌曲文件。
        /// </summary>
        /// <param name="path">等待扫描的路径。</param>
        /// <param name="extensions">需要搜索的歌曲后缀名。</param>
        Task<List<FileScannerResult>> ScanAsync(string path, IEnumerable<string> extensions);
    }
}
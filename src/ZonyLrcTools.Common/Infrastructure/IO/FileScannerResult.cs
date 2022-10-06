namespace ZonyLrcTools.Common.Infrastructure.IO
{
    /// <summary>
    /// 文件扫描结果对象。
    /// </summary>
    public class FileScannerResult
    {
        /// <summary>
        /// 当前路径对应的扩展名。
        /// </summary>
        public string ExtensionName { get; }

        /// <summary>
        /// 当前扩展名下面的所有文件路径集合。
        /// </summary>
        public List<string> FilePaths { get; }

        /// <summary>
        /// 构造一个新的 <see cref="FileScannerResult"/> 对象。
        /// </summary>
        /// <param name="extensionName">当前路径对应的扩展名。</param>
        /// <param name="filePaths">当前扩展名下面的所有文件路径集合。</param>
        public FileScannerResult(string extensionName, List<string> filePaths)
        {
            ExtensionName = extensionName;
            FilePaths = filePaths;
        }
    }
}
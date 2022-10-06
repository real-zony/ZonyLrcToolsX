namespace ZonyLrcTools.Common.Configuration
{
    public class GlobalOptions
    {
        /// <summary>
        /// 支持的音乐文件后缀集合。
        /// </summary>
        public List<string> SupportFileExtensions { get; set; }

        /// <summary>
        /// 网络代理相关的配置信息。
        /// </summary>
        public NetworkOptions NetworkOptions { get; set; }

        /// <summary>
        /// 定义下载器的相关配置信息。
        /// </summary>
        public ProviderOptions Provider { get; set; }
    }
}
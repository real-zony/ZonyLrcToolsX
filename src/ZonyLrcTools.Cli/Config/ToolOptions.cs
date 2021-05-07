using ZonyLrcTools.Cli.Infrastructure.Lyric;
using ZonyLrcTools.Cli.Infrastructure.Network;
using ZonyLrcTools.Cli.Infrastructure.Tag;

namespace ZonyLrcTools.Cli.Config
{
    public class ToolOptions
    {
        /// <summary>
        /// 支持的音乐文件后缀集合，以 ; 进行分隔。
        /// </summary>
        public string SupportFileExtensions { get; set; }

        /// <summary>
        /// 歌词下载相关的配置信息。
        /// </summary>
        public LyricItemCollectionOption LyricOption { get; set; }

        /// <summary>
        /// 标签加载器的加载配置项。
        /// </summary>
        public TagInfoProviderOptions TagInfoProviderOptions { get; set; }

        /// <summary>
        /// 网络代理相关的配置信息。
        /// </summary>
        public NetworkOptions NetworkOptions { get; set; }
    }
}
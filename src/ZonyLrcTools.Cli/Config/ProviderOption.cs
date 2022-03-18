using ZonyLrcTools.Cli.Infrastructure.Lyric;
using ZonyLrcTools.Cli.Infrastructure.Tag;

namespace ZonyLrcTools.Cli.Config;

public class ProviderOption
{
    /// <summary>
    /// 标签加载器相关的配置属性。
    /// </summary>
    public TagOption Tag { get; set; }

    /// <summary>
    /// 歌词下载相关的配置信息。
    /// </summary>
    public LyricOption Lyric { get; set; }
}
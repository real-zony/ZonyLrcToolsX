namespace ZonyLrcTools.Common.Configuration;

public class ProviderOptions
{
    /// <summary>
    /// 标签加载器相关的配置属性。
    /// </summary>
    public TagInfoOptions Tag { get; set; } = null!;

    /// <summary>
    /// 歌词下载相关的配置信息。
    /// </summary>
    public LyricsOptions Lyric { get; set; } = null!;
}
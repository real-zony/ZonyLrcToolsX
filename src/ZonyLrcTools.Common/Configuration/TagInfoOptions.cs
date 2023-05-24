namespace ZonyLrcTools.Common.Configuration;

public class TagInfoOptions
{
    public IEnumerable<TagInfoProviderOptions> Plugin { get; set; } = null!;

    /// <summary>
    /// 屏蔽词功能相关配置。
    /// </summary>
    public BlockWordOptions BlockWord { get; set; } = null!;
}
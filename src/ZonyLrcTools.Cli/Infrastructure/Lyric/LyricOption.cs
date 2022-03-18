using System.Collections.Generic;

namespace ZonyLrcTools.Cli.Infrastructure.Lyric;

public class LyricOption
{
    public IEnumerable<LyricProviderOption> Plugin { get; set; }

    public LyricConfigOption Config { get; set; }
}

public class LyricConfigOption
{
    /// <summary>
    /// 双语歌词是否合并为一行。
    /// </summary>
    public bool IsOneLine { get; set; } = false;

    /// <summary>
    /// 换行符格式，取值来自 <see cref="LineBreakType"/> 常量类。
    /// </summary>
    public string LineBreak { get; set; } = LineBreakType.Windows;

    /// <summary>
    /// 是否启用歌词翻译功能。
    /// </summary>
    public bool IsEnableTranslation { get; set; } = false;
}
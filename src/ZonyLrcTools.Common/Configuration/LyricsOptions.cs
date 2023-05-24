namespace ZonyLrcTools.Common.Configuration;

public class LyricsOptions
{
    public IEnumerable<LyricsProviderOptions> Plugin { get; set; } = null!;

    public GlobalLyricsConfigOptions Config { get; set; } = null!;

    public LyricsProviderOptions GetLyricProviderOption(string name)
    {
        return Plugin.FirstOrDefault(x => x.Name == name)!;
    }
}
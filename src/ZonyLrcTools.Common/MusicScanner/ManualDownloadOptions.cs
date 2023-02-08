namespace ZonyLrcTools.Common.MusicScanner;

public class ManualDownloadOptions
{
    public string OutputFileNamePattern { get; set; } = "{Artist} - {Name}.lrc";
    public string OutputDirectory { get; set; } = "DownloadedLrc";
}